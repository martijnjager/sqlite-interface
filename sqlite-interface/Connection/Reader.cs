using Database.Clauses;
using Database.Contracts;
using Database.Contracts.Connection;
using Database.Enums;
using Database.Transactions;
using System.Collections.ObjectModel;
using System.Data.Common;
using Database.Extensions;
using Database.Relations;

namespace Database.Connection
{
    internal class Reader(string? location = null, string? source = null) : BaseConnection(location, source), IReader
    {

        /// <summary>
        /// Processes the query and reads the data from the database.
        /// </summary>
        /// <typeparam name="T">The type of the models.</typeparam>
        /// <param name="query">The query to process.</param>
        /// <returns>A list of models.</returns>
        public Collection Read(Transactions.Transaction transaction)
        {
            Collection items = new();

            QueryResult<SaveStatus> result = base.RunQuery(transaction);

            if (result.Reader is null)
            {
                return items;
            }

            DbDataReader reader = result.Reader;

            var defaultColumns = this.GetColumns(reader);

            string table = this.GetTableName(reader);

            while (result.Reader.Read())
            {
                if (MapToModel(result.Reader, table, defaultColumns) is IModel mainModel)
                {
                    items.Add(mainModel);
                }
            }

            RelationManager.LoadRelations(items, transaction);

            result.Reader.Close();

            return items;
        }

        public async Task<QueryResult<SaveStatus>> ReadAsync(Transaction transaction)
        {
            Collection items = new();
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
                        IModel? model = MapToModel(reader, table, reader.GetColumnSchema());
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

        private static IModel? MapToModel(DbDataReader reader, string table, ReadOnlyCollection<DbColumn> defaultColumns)
        {
            ParamBag paramBag = new();
            IModel? mainModel = ModelRegistry.ModelByKey(table, defaultColumns);

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

            mainModel.Attributes.AssignByReader(paramBag);

            return mainModel;
        }

        /// <summary>
        /// Gets the columns from the reader without any columns excluded by user input.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="clauseManager"></param>
        /// <returns></returns>
        protected ReadOnlyCollection<DbColumn>? GetColumns(DbDataReader reader)
        {
            var columns = reader.GetColumnSchema();

            List<DbColumn> modifiableColumns = [.. columns];

            //modifiableColumns.RemoveAll(column => builder.ExcludeColumns.Contains(column.ColumnName));

            return new ReadOnlyCollection<DbColumn>(modifiableColumns);
        }

        protected string GetTableName(DbDataReader reader)
        {
            return reader.GetColumnSchema().FirstOrDefault().BaseTableName?.RemoveSpecialCharacters() ?? string.Empty;
        }
    }
}
