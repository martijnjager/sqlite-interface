
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
using Database.Contracts.Event;
using System.Data.Entity.Core.Objects.DataClasses;

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

            if (this is IEvent)
            {
                this.RegisterEvents();
            }
        }

        /// <summary>
        /// Sets the table name.
        /// </summary>
        /// <param name="table"></param>
        /// <returns>The name of the table</returns>
        private string? SetTable(string? table = null)
        {
            if (string.IsNullOrEmpty(table))
            {
                var tableName = System.Attribute.GetCustomAttribute(this.GetType(), typeof(TableNameAttribute));

                if (tableName is TableNameAttribute attribute)
                {
                    table = attribute.Name;
                }
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
        /// <param name="value">The value to search for.</param>
        /// <returns>The found model.</returns>
        public IModel? Find<T>(string value) where T : IModel
        {
            string primaryKey = this.Attributes.PrimaryKey;
            List<IModel> result = this.Where(primaryKey, value).Get<T>();

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
        /// Sets the columns to select except the ones specified.
        /// </summary>
        /// <param name="columns">The columns not selected</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent DontSelect(params string[] columns)
        {
            foreach (var column in columns)
            {
                this.clauses.DontSelectColumn(column);
            }

            return this;
        }

        public Eloquent RemoveDontSelect(params string[] columns)
        {
            foreach (var column in columns)
            {
                this.clauses.RemoveDontSelect(column);
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
        public Eloquent Where(string where, string op, string? value = null)
        {
            if (value is null)
            {
                value = op;

                op = Operator.Equals;
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
            this.Where(where, Operator.Is, "null");

            return this;
        }

        /// <summary>
        /// Adds a "where not null" condition to the query.
        /// </summary>
        /// <param name="where"></param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent WhereNotNull(string where)
        {
            this.Where(where, Operator.IsNot, "null");

            return this;
        }

        public Eloquent OrWhere(string where, string op, string? value = null)
        {
            if (value is null)
            {
                value = op;

                op = Operator.Equals;
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
        public Eloquent Having(string have, string op, string? value = null)
        {
            if (value is null)
            {
                value = op;
                op = Operator.Equals;
            }

            this.clauses.AddHavingClause(have, op, value);

            return this;
        }

        public Eloquent OrHaving(string have, string op, string? value = null)
        {
            if (value is null)
            {
                value = op;
                op = Operator.Equals;
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
        /// <exception cref="">Thrown when an exception occurs.</exception>
        public List<IModel> Get<T>() where T : IModel
        {
            this.HandlePrecautions();

            TransactionManager manager = InstanceContainer.Instance.ConnectionManager();

            var result = manager.Read<T>(this.clauses);

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
            // check soft delete, then delete
            if (this is IUseTimestamps)
            {
                this.clauses.AddSoftDeleteClause();
            } else
            {
                this.clauses.AddDeleteClause();
            }

            return this.RunQuery(Transactions.Type.TYPE_DELETE);
        }

        /// <summary>
        /// Force deletes the model from the database.
        /// </summary>
        /// <returns></returns>
        public IModel ForceDelete()
        {
            this.clauses.AddDeleteClause();

            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_DELETE);

            if (result.Status != SaveStatus.Error)
            {
                return result.Data as IModel;
            }

            throw new Exception(result.Message);
        }

        /// <summary>
        /// Restores the model from the database.
        /// </summary>
        /// <returns></returns>
        public IModel Restore()
        {
            if (this is IUseTimestamps && this.Attributes.IsTrashed())
            {
                this.clauses.AddRestoreClause();
            }

            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_UPDATE);

            if (result.Status != SaveStatus.Error)
            {
                return result.Data as IModel;
            }

            throw new Exception(result.Message);
        }

        private QueryResult<SaveStatus> RunQuery(Transactions.Type type)
        {
            this.CompileAttributesToClause(type);

            var result = TransactionManager.Save(this.clauses);
            this.clauses.ClearClauses();

            if (this is IEvent)
            {
                this.HandleDispatchEvent(type);
            }

            return result;
        }

        private void HandleDispatchEvent(Transactions.Type transactionType)
        {
            string eventToDispatch = transactionType switch
            {
                Transactions.Type.TYPE_INSERT => "created",
                Transactions.Type.TYPE_UPDATE => "updated",
                Transactions.Type.TYPE_DELETE => "deleted",
            };

            Eloquent eloquent = this;
            IModel model = null;

            if (this is IUseTimestamps && eventToDispatch == "deleted")
            {
                eloquent = this.WithTrashed();
            }

            switch (transactionType)
            {
                case Transactions.Type.TYPE_INSERT:
                    model = eloquent.OrderBy("id", "desc").First<IModel>();
                    break;
                case Transactions.Type.TYPE_UPDATE:
                case Transactions.Type.TYPE_DELETE:
                    model = eloquent.Find<IModel>(eloquent.Attributes.GetValue(eloquent.Attributes.PrimaryKey));
                    break;
            }

            this.DispatchEvent(eventToDispatch, model);
            this.DispatchEvent("saved", model);
        }

        /// <summary>
        /// Compiles the attributes to a clause.
        /// </summary>
        /// <param name="operation"></param>
        private void CompileAttributesToClause(Transactions.Type operation)
        {
            bool modelExists = this.Attributes.HasPrimaryKey();
            if (this is IUseTimestamps)
            {
                this.Attributes.HandleTimestamps(operation, modelExists);
            }

            if (this is IEvent)
            {
                string eventOperation = operation switch
                {
                    Transactions.Type.TYPE_INSERT => "creating",
                    Transactions.Type.TYPE_UPDATE => "updating",
                    Transactions.Type.TYPE_DELETE => "deleting",
                };

                this.DispatchEvent(eventOperation);
                this.DispatchEvent("saving");
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
            }
        }

        private void CompileManyToClause(ParamBag[] bags, Transactions.Type operation)
        {
            List<IDictionary<string, string>> keyValues = new();

            foreach (var bag in bags)
            {
                IDictionary<string, string> keyValue = new Dictionary<string, string>();
                foreach (var item in bag.GetParameters())
                {
                    keyValue[item.Item1] = item.Item2;
                }
                keyValues.Add(keyValue);
            }

            this.clauses.AddInsertManyClause(keyValues);
        }

        /// <summary>
        /// Creates a new model in the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IModel Create<T>() where T : IModel
        {
            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_INSERT);

            if (result.Status != SaveStatus.Error)
            {
                IModel? model = GetModel<T>();

                return model;
            }

            throw new Exception(result.Message);
        }

        /// <summary>
        /// Updates the model in the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IModel Update<T>() where T : IModel
        {
            if (!this.Attributes.HasPrimaryKey())
            {
                throw new Exception("Primary key not set.");
            }

            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_UPDATE);

            if (result.Status != SaveStatus.Error)
            {
                IModel? model = GetModel<T>();

                return model;
            }

            throw new Exception(result.Message);
        }

        public List<IModel> CreateMany<T>(ParamBag[] bags) where T : IModel
        {
            this.CompileManyToClause(bags, Transactions.Type.TYPE_INSERT);

            var result = TransactionManager.Save(this.clauses);

            if (result.Status != SaveStatus.Error)
            {
                List<IModel> models = this.OrderBy("id", "desc").Limit(bags.Length).Get<T>();

                if (this is IEvent)
                {
                    foreach (var model in models)
                    {
                        this.DispatchEvent("created", model);
                        this.DispatchEvent("saved", model);
                        this.DispatchEvent("retrieved", model);
                    }
                }

                return models;
            }

            throw new Exception(result.Message);
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
            this.clauses.SetLimit(1);
            return this.Get<T>().FirstOrDefault();
        }

        /// <summary>
        /// Handles precautions making sure the query is valid and executable
        /// </summary>
        private void HandlePrecautions()
        {
            if (this.Attributes.TimestampsEnabled() &&
                this is IUseTimestamps &&
                !this.clauses.HasWhereColumn(TimestampManager.SOFT_DELETE_COLUMN))
            {
                this.WhereNull(TimestampManager.SOFT_DELETE_COLUMN);
            }

            if (this.clauses.QueryType is null)
            {
                this.clauses.AddSelectColumn("*");
            }
        }

        /// <summary>
        /// Adds an order to the query.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="order"></param>
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
                this.Attributes.GetValue(TimestampManager.DELETED_AT) is null;
        }

        private void DispatchEvent(string eventToDispatch)
        {
            Events.Dispatcher dispatcher = InstanceContainer.Instance.EventDispatcher();

            IModel model = this as IModel;
            eventToDispatch = dispatcher.GenerateEventName(this.GetTable(), eventToDispatch);

            dispatcher.Dispatch(eventToDispatch, model);
        }

        private void DispatchEvent(string eventToDispatch, IModel model)
        {
            Events.Dispatcher dispatcher = InstanceContainer.Instance.EventDispatcher();
            eventToDispatch = dispatcher.GenerateEventName(this.GetTable(), eventToDispatch);
            dispatcher.Dispatch(eventToDispatch, model);
        }

        private void RegisterEvents()
        {
            if (this is IEvent)
            {
                var eventMethods = typeof(IEvent).GetMethods();
                foreach (var method in eventMethods)
                {
                    Events.Dispatcher dispatcher = InstanceContainer.Instance.EventDispatcher();
                    string eventName = dispatcher.GenerateEventName(this.GetTable(), method.Name);

                    Action<IModel> action = (Action<IModel>)Delegate.CreateDelegate(typeof(Action<IModel>), this, method);
                    
                    InstanceContainer.Instance.EventDispatcher().AddListener(eventName, action);
                }
            }
        }
    }
}
