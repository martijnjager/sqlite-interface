using Database.Clauses;
using Database.Contracts;
using Database.Contracts.Connection;
using Database.Enums;
using Database.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Relations;

namespace Database.Connection
{
    internal class Reader : BaseConnection, IReader
    {
        public Reader(string? location = null, string? source = null) : base(location, source)
        {
        }

        /// <summary>
        /// Processes the query and reads the data from the database.
        /// </summary>
        /// <typeparam name="T">The type of the models.</typeparam>
        /// <param name="query">The query to process.</param>
        /// <returns>A list of models.</returns>
        public List<IModel> Read<T>(ClauseManager clauseManager) where T : IModel
        {
            List<IModel> items = new();

            Transaction transaction = clauseManager.Compile();
            QueryResult<SaveStatus> result = base.RunQuery(transaction);

            if (result.Reader is null)
            {
                return items;
            }

            DbDataReader reader = result.Reader;

            var defaultColumns = this.GetColumns(reader, clauseManager);

            string table = defaultColumns.FirstOrDefault().BaseTableName;

            table = table.RemoveSpecialCharacters();

            while (result.Reader.Read())
            {
                IModel? mainModel = MapToModel<T>(result.Reader, table, defaultColumns);

                if (mainModel is not null)
                {
                    items.Add(mainModel);
                }
            }

            if (items.Count > 0 && items.First().Relations.CanEagerLoad())
            { 
                foreach (ToLoad relation in items.First().Relations.ToEagerLoad())
                {
                    RelationManager.LoadRelations(relation, items);
                }
            }

            result.Reader.Close();
            transaction.Close();

            return items;
        }

        public async Task<QueryResult<SaveStatus>> ReadAsync<T>(Transaction transaction) where T : IModel
        {
            List<IModel> items = new();
            Result = new QueryResult<SaveStatus>();
            Result.SetStatus(SaveStatus.Pending);

            try
            {
                DbDataReader reader = await transaction.Query.ExecuteReaderAsync();
                Result.SetStatus(SaveStatus.Success);
                Result.SetReader(reader);

                while (reader.Read())
                {
                    var schemaTable = reader.GetColumnSchema();
                    if (schemaTable is not null)
                    {
                        string table = schemaTable.FirstOrDefault().BaseTableName;
                        IModel? model = MapToModel<T>(reader, table, reader.GetColumnSchema());
                        if (model is not null)
                        {
                            items.Add(model);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Result.SetStatus(SaveStatus.Error);
                Result.SetMessage(e.Message);
            }
            finally
            {
                transaction.Close();
            }

            return Result;
        }

        private static IModel? MapToModel<T>(DbDataReader reader, string table, ReadOnlyCollection<DbColumn> defaultColumns) where T : IModel
        {
            ParamBag paramBag = new();
            IModel? mainModel = InstanceContainer.ModelByKey(table, defaultColumns);

            if (mainModel is null)
            {
                return null;
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string key = reader.GetName(i);

                if (defaultColumns.Any(column => column.ColumnName == key) is false)
                {
                    continue;
                }

                object? value = reader[i] is DBNull ? null : reader[i].ToString();

                paramBag.Add(key, value);
            }

            mainModel.Assign(paramBag);

            return mainModel;
        }

        /// <summary>
        /// Gets the columns from the reader without any columns excluded by user input.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="clauseManager"></param>
        /// <returns></returns>
        protected ReadOnlyCollection<DbColumn>? GetColumns(DbDataReader reader, ClauseManager clauseManager)
        {
            var columns = reader.GetColumnSchema();

            List<DbColumn> modifiableColumns = new List<DbColumn>(columns);

            modifiableColumns.RemoveAll(column => clauseManager.ExcludeColumns.Contains(column.ColumnName));

            return new ReadOnlyCollection<DbColumn>(modifiableColumns);
        }
    }
}
