
using Database.Contracts;
using Database.Relations;
using Database.Attribute;
using Database.Transactions;
using Database.Extensions;
using System.Reflection;
using System.Diagnostics;
using Database.Clauses;
using Database.Enums;
using Database.Extensions.Model;
using Database.Contracts.Attribute;
using System;
using Database.Extensions.Model.Attribute;

namespace Database
{
    /// <summary>
    /// Represents a fluid interface query builder class for interacting with the database.
    /// </summary>
    public class Eloquent : IEloquent
    {
        public RelationManager Relations { get; }

        public IAttributeManager Attributes { get; }

        protected ClauseManager clauses;

        /// <summary>
        /// Initializes a new instance of the <see cref="Eloquent"/> class.
        /// </summary>
        /// <param name="table">The table name.</param>
        public Eloquent()
        {
            this.Relations = new RelationManager();
            this.Attributes = new AttributeManager();
            this.Attributes.SetPrimaryKey(string.Empty);
            this.clauses = new ClauseManager(this.SetTable());
        }

        private string SetTable(string table = null)
        {
            if (string.IsNullOrEmpty(table))
            {
                TableNameAttribute tableName = (TableNameAttribute)System.Attribute.GetCustomAttribute(this.GetType(), typeof(TableNameAttribute));
                table = tableName.Name;
            }

            return table;
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        /// <returns>The table name.</returns>
        public string GetTable()
        {
            return this.clauses.GetTable();
        }

        /// <summary>
        /// Returns the columns of a model.
        /// </summary>
        /// <returns>The columns of the model.</returns>
        public ICollection<string> GetKeys()
        {
            return this.Attributes.Attributes.Keys;
        }

        public IDictionary<string, string> GetAttributes()
        {
            return this.Attributes.Attributes;
        }

        public IList<string> GetRawAttributes()
        {
            return this.Attributes.RawAttributes;
        }

        /// <summary>
        /// Finds a model by the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="key">The key to search for.</param>
        /// <returns>The found model.</returns>
        public IModel? Find<T>(string key) where T : IModel
        {
            List<IModel> result = this.Where("id", key).Get<T>();

            return result.FirstOrDefault();
        }

        /// <summary>
        /// Sets the columns to select.
        /// </summary>
        /// <param name="columns">The columns to select.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Select(params string[] columns)
        {
            foreach (var column in columns)
            {
                this.clauses.AddSelectColumn(column);
            }

            return this;
        }

        /// <summary>
        /// Sets the columns to select.
        /// </summary>
        /// <param name="columns">The columns to select with alias</param>
        /// <returns></returns>
        public Eloquent Select(IDictionary<string, string> columns)
        {
            foreach (var column in columns)
            {
                this.clauses.AddSelectColumn(column.Key, column.Value);
            }

            return this;
        }

        /// <summary>
        /// Add a join condition to the query.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="first"></param>
        /// <param name="op"></param>
        /// <param name="second"></param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Join(string table, string first, string op, string second)
        {
            this.clauses.AddJoinClause(table, first, op, second);

            return this;
        }

        #region Where methods
        /// <summary>
        /// Adds a "where" condition to the query.
        /// </summary>
        /// <param name="where">The column to compare.</param>
        /// <param name="op">The operator to use.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Where(string where, string op, string value = null)
        {
            if (value == null)
            {
                value = op;

                op = "=";
            }

            this.clauses.AddWhereClause(where, op, value);

            return this;
        }

        public Eloquent WhereIn(string key, params object[] values)
        {
            this.clauses.AddWhereInClause(key, values);

            return this;
        }

        /// <summary>
        /// Adds a "where null" condition to the query.
        /// </summary>
        /// <param name="where"></param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent WhereNull(string where)
        {
            this.Where(where, "is", "null");

            return this;
        }

        /// <summary>
        /// Adds a "where not null" condition to the query.
        /// </summary>
        /// <param name="where"></param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent WhereNotNull(string where)
        {
            this.Where(where, "is not", "null");

            return this;
        }

        public Eloquent OrWhere(string where, string op, string value = null)
        {
            if (value == null)
            {
                value = op;

                op = "=";
            }

            this.clauses.AddOrWhereClause(where, op, value);

            return this;
        }

        /// <summary>
        /// Adds a "where" condition to the query using a callback.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Where(Action<Eloquent> callback)
        {
            this.clauses.AddNestedWhereClause(callback, this);
            return this;
        }
        #endregion

        /// <summary>
        /// Sets the column to group by.
        /// </summary>
        /// <param name="column">The column to group by.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent GroupBy(string column)
        {
            this.clauses.AddGroupBy(column);
            return this;
        }

        #region Having methods
        /// <summary>
        /// Adds a "having" condition to the query.
        /// </summary>
        /// <param name="have">The column to compare.</param>
        /// <param name="op">The operator to use.</param>
        /// <param name="value">The value to compare.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Having(string have, string op, string value = null)
        {
            if (value == null)
            {
                value = op;
                op = "=";
            }

            this.clauses.AddHavingClause(have, op, value);

            return this;
        }

        public Eloquent OrHaving(string have, string op, string value = null)
        {
            if (value == null)
            {
                value = op;
                op = "=";
            }

            this.clauses.AddOrHavingClause(have, op, value);

            return this;
        }

        public Eloquent Having(Action<Eloquent> callback)
        {
            this.clauses.AddNestedHavingClause(callback, this);

            return this;
        }
        #endregion

        /// <summary>
        /// Sets the column to order by.
        /// </summary>
        /// <param name="column">The column to order by.</param>
        /// <param name="order">The order to use.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent OrderBy(string column, string order = "asc")
        {
            this.AddOrder(column, order);

            return this;
        }

        /// <summary>
        /// Sets the limit of the query.
        /// </summary>
        /// <param name="limit">The limit of the query.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Limit(int limit)
        {
            this.clauses.SetLimit(limit);

            return this;
        }

        /// <summary>
        /// Adds a "with trashed" condition to the query.
        /// </summary>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent WithTrashed()
        {
            if (this.Attributes.TimestampsEnabled())
            {
                this.WhereNotNull(TimestampManager.DELETED_AT);
            }

            return this;
        }

        /// <summary>
        /// Executes the query and returns a list of models.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <returns>A list of models.</returns>
        public List<IModel> Get<T>() where T : IModel
        {
            this.HandlePrecautions();

            var result = InstanceContainer.Instance.ConnectionManager().Run<T>(this.clauses);

            //var modelIds = result.Select(m => m.GetValue("id")).Distinct().ToArray();

            this.clauses.ClearClauses();
            this.Relations.Reset();

            return result;
        }

        /// <summary>
        /// Deletes the model from the database.
        /// </summary>
        /// <returns>The result of the delete operation.</returns>
        public QueryResult<SaveStatus> Delete()
        {
            var result = new QueryResult<SaveStatus>();
            try
            {
                // check soft delete, then delete
                if (this is IUseTimestamps)
                {
                    this.clauses.AddSoftDeleteClause();
                } else
                {
                    this.clauses.AddDeleteClause();
                }

                this.CompileAttributesToClause(Transactions.Type.TYPE_DELETE);
                result = TransactionManager.Run(this.clauses);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return result;
        }

        private void CompileAttributesToClause(Transactions.Type operation)
        {
            bool modelExists = this.Attributes.HasPrimaryKey();
            if (this is IUseTimestamps)
            {
                this.Attributes.HandleTimestamps(operation, modelExists);
            }

            foreach (var item in Attributes.GetChanges())
            {
                if (item.Key == this.Attributes.PrimaryKey)
                {
                    this.Where(item.Key, item.Value);
                    continue;
                }

                switch (operation)
                {
                    case Transactions.Type.TYPE_INSERT:
                        this.clauses.AddInsertClause(item.Key, item.Value);
                        break;
                    case Transactions.Type.TYPE_UPDATE:
                        this.clauses.AddUpdateClause(item.Key, item.Value);
                        break;
                    case Transactions.Type.TYPE_DELETE:
                        if (item.Key == TimestampManager.DELETED_AT && this.CanBeSoftDeleted())
                        {
                            this.clauses.AddUpdateClause(item.Key, item.Value);
                        }
                        else
                        {
                            this.Where(item.Key, item.Value);
                        }
                        break;
                }
                //if (this is IUseTimestamps)
                //{
                //    HandleTimestamps(item, operation, modelExists);
                //}
                //else
                //{
                    //if (modelExists)
                    //{
                    //    this.clauses.AddUpdateClause(item.Key, item.Value);
                    //}
                    //else
                    //{
                    //    this.clauses.AddInsertClause(item.Key, item.Value);
                    //}
                //}
            }
        }

        //private void HandleTimestamps(KeyValuePair<string, string> item, Transactions.Type operation, bool modelExists)
        //{
        //    if (operation == Transactions.Type.TYPE_INSERT || operation == Transactions.Type.TYPE_UPDATE)
        //    {
        //        if (modelExists)
        //        {
        //            this.clauses.AddUpdateClause(item.Key, item.Value);
        //        } else
        //        {
        //            this.clauses.AddInsertClause(item.Key, item.Value);
        //        }
        //    }
        //    else if (operation == Transactions.Type.TYPE_DELETE)
        //    {
        //        this.clauses.AddUpdateClause(item.Key, item.Value);
        //    }
        //}

        /// <summary>
        /// Saves the model to the database.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <returns>The result of the save operation.</returns>
        public QueryResult<SaveStatus> Save<T>() where T : IModel
        {
            string primaryKey = null;
            Transactions.Type type = Transactions.Type.TYPE_INSERT;
            if (this.Attributes.HasPrimaryKey())
            {
                primaryKey = this.Attributes.GetValue(this.Attributes.PrimaryKey);
                type = Transactions.Type.TYPE_UPDATE;
            }
            
            this.CompileAttributesToClause(type);

            QueryResult<SaveStatus> result = TransactionManager.Run(this.clauses);

            IModel model = GetModel<T>(primaryKey);

            result.SetData(model);

            return result;
        }

        /// <summary>
        /// Gets the model based on whether it is new or not.
        /// </summary>
        /// <param name="isNew">True if the model is new, false otherwise.</param>
        /// <param name="primaryKey">The primary key of the model.</param>
        /// <returns>The model.</returns>
        private IModel? GetModel<T>(string? id = null) where T : IModel
        {
            var tempModel = InstanceContainer.ModelByKey(this.GetTable()) as Model;

            if (string.IsNullOrEmpty(id))
            {
                return tempModel?.OrderBy("id", "desc")?.First<T>();
            }

            return tempModel?.Find<T>(id);
        }

        /// <summary>
        /// Executes the query and returns the first model.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <returns>The first model.</returns>
        public IModel? First<T>() where T : IModel
        {
            return this.Get<T>().FirstOrDefault();
        }

        private void HandlePrecautions()
        {
            if (this.Attributes.TimestampsEnabled() &&
                this is IUseTimestamps &&
                !this.clauses.HasWhereColumn(TimestampManager.SOFT_DELETE_COLUMN))
            {
                this.WhereNull(TimestampManager.SOFT_DELETE_COLUMN);
            }
        }

        private void AddOrder(string column, string order)
        {
            this.clauses.AddOrderBy(column, order);
        }

        /// <summary>
        /// Returns the relations of the model.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Relation> GetRelations()
        {
            return this.Relations.GetLoadedRelations();
        }

        /// <summary>
        /// Checks if the model can be soft deleted.
        /// </summary>
        /// <returns></returns>
        public bool CanBeSoftDeleted()
        {
            return this is IUseTimestamps && this.Attributes.CanBeSoftDeleted() && 
                this.Attributes.GetValue(TimestampManager.SOFT_DELETE_COLUMN) is not null && !this.Attributes.IsTrashed();
        }
    }
}
