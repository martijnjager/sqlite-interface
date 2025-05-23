
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
using Database.Console;
using Database.Contracts.Relation;

namespace Database
{
    /// <summary>
    /// Represents a fluid interface query builder class for interacting with the database.
    /// </summary>
    public class Eloquent : IEloquent
    {
        public RelationManager Relations { get; }

        public IAttributeManager Attributes { get; }

        internal QueryBuilder.Builder queryBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Eloquent"/> class.
        /// </summary>
        /// <param name="table">The table name.</param>
        public Eloquent()
        {
            this.Relations = new RelationManager();
            this.Attributes = new AttributeManager();
            this.Attributes.SetPrimaryKey(string.Empty);
            this.queryBuilder = new QueryBuilder.Builder(this.SetTable());


            //if (this is IEvent)
            //{
            //    this.RegisterEvents();
            //}
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
            return this.queryBuilder.GetTable();
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

        public IDictionary<string, dynamic> GetCustomAttributes()
        {
            return this.Attributes.CustomAttributes;
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
        public IModel? Find(string value)
        {
            string primaryKey = this.Attributes.PrimaryKey;
            Collection result = this.Where(primaryKey, value).Get();

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
                this.queryBuilder.AddSelectColumn(column);
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
                this.queryBuilder.DontSelectColumn(column);
            }

            return this;
        }

        public Eloquent RemoveDontSelect(params string[] columns)
        {
            foreach (var column in columns)
            {
                this.queryBuilder.RemoveDontSelect(column);
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
                this.queryBuilder.AddSelectColumn(column.Key, column.Value);
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
            this.queryBuilder.AddJoinClause(table, first, op, second);

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
        public Eloquent Where(string where, object op, object? value = null)
        {
            if (value is null)
            {
                value = op;

                op = Operator.Equals;
            }

            this.queryBuilder.AddWhereClause(where, op as string, value);

            return this;
        }

        public Eloquent WhereIn(string key, params object[] values)
        {
            this.queryBuilder.AddWhereInClause(key, values);

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

        public Eloquent OrWhere(string where, object op, object? value = null)
        {
            if (value is null)
            {
                value = op;

                op = Operator.Equals;
            }

            this.queryBuilder.AddOrWhereClause(where, op.ToString(), value);

            return this;
        }

        /// <summary>
        /// Adds a "where" condition to the query using a callback.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent Where(Action<Eloquent> callback)
        {
            this.queryBuilder.AddNestedWhereClause(callback, this);
            return this;
        }

        public Eloquent WhereHas(string relation, Action<Eloquent> callback)
        {
            try
            {
                var methodData = this.GetType().GetMethod(relation);

                if (methodData is not null)
                {
                    var join = methodData.Invoke(this, null) as IJoin;

                    var subquery = join.Query();

                    this.queryBuilder.AddWhereHasClause(relation, callback, subquery, join);

                    return this;
                }

                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Eloquent WhereHas(string relation)
        {
            try
            {
                var methodData = this.GetType().GetMethod(relation);
                if (methodData is not null)
                {
                    var join = methodData.Invoke(this, null) as IJoin;
                    var subquery = join.Query();
                    this.queryBuilder.AddWhereHasClause(relation, null, subquery, join);
                    return this;
                }
                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Eloquent WhereDoesntHave(string relation, Action<Eloquent> callback)
        {
            try
            {
                var methodData = this.GetType().GetMethod(relation, BindingFlags.IgnoreCase);

                if (methodData is not null)
                {
                    var join = methodData.Invoke(this, null) as IJoin;

                    var subquery = join.Query();

                    this.queryBuilder.AddWhereHasNotClause(relation, callback, subquery, join);

                    return this;
                }

                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Eloquent WhereDoesntHave(string relation)
        {
            try
            {
                var methodData = this.GetType().GetMethod(relation, BindingFlags.IgnoreCase);
                if (methodData is not null)
                {
                    var join = methodData.Invoke(this, null) as IJoin;
                    var subquery = join.Query();
                    this.queryBuilder.AddWhereHasNotClause(relation, null, subquery, join);
                    return this;
                }
                throw new ArgumentException();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// Sets the column to group by.
        /// </summary>
        /// <param name="column">The column to group by.</param>
        /// <returns>The <see cref="Eloquent"/> instance.</returns>
        public Eloquent GroupBy(string column)
        {
            this.queryBuilder.AddGroupBy(column);
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
        public Eloquent Having(string have, object op, object? value = null)
        {
            if (value is null)
            {
                value = op;
                op = Operator.Equals;
            }

            this.queryBuilder.AddHavingClause(have, op as string, value);

            return this;
        }

        public Eloquent OrHaving(string have, object op, object? value = null)
        {
            if (value is null)
            {
                value = op;
                op = Operator.Equals;
            }

            this.queryBuilder.AddOrHavingClause(have, op as string, value);

            return this;
        }

        public Eloquent Having(Action<Eloquent> callback)
        {
            this.queryBuilder.AddNestedHavingClause(callback, this);

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
            this.queryBuilder.SetLimit(limit);

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
        public Collection Get()
        {
            this.HandlePrecautions();

            TransactionManager manager = App.Instance.ConnectionManager();

            Collection result = manager.Read(this.queryBuilder);

            //this.Relations.Reset();

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
                this.queryBuilder.AddSoftDeleteClause();
            } else
            {
                this.queryBuilder.AddDeleteClause();
            }

            return this.RunQuery(Transactions.Type.TYPE_DELETE);
        }

        /// <summary>
        /// Force deletes the model from the database.
        /// </summary>
        /// <returns></returns>
        public IModel? ForceDelete()
        {
            this.queryBuilder.AddDeleteClause();

            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_DELETE);

            if (result.Status != SaveStatus.Error && result.Data is IModel)
            {
                return result.Data as IModel;
            }

            throw new Exception(result.Message);
        }

        /// <summary>
        /// Restores the model from the database.
        /// </summary>
        /// <returns></returns>
        public IModel? Restore()
        {
            if (this is IUseTimestamps && this.Attributes.IsTrashed())
            {
                this.queryBuilder.AddRestoreClause();
            }

            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_UPDATE);

            if (result.Status != SaveStatus.Error && result.Data is IModel)
            {
                return result.Data as IModel;
            }

            throw new Exception(result.Message);
        }

        private QueryResult<SaveStatus> RunQuery(Transactions.Type type)
        {
            this.CompileAttributesToClause(type);

            var result = TransactionManager.Save(this.queryBuilder);
            //this.queryBuilder.Clea();

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
                Transactions.Type.TYPE_INSERT => Events.EventType.CREATED,
                Transactions.Type.TYPE_UPDATE => Events.EventType.UPDATED,
                Transactions.Type.TYPE_DELETE => Events.EventType.DELETED,
                Transactions.Type.TYPE_SELECT => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            Eloquent eloquent = this;
            IModel model = null;

            if (this is IUseTimestamps && eventToDispatch == Events.EventType.DELETED)
            {
                eloquent = this.WithTrashed();
            }

            switch (transactionType)
            {
                case Transactions.Type.TYPE_INSERT:
                    model = eloquent.OrderBy("id", "desc").First();
                    break;
                case Transactions.Type.TYPE_UPDATE:
                case Transactions.Type.TYPE_DELETE:
                    model = eloquent.Find(eloquent.Attributes.GetValue(eloquent.Attributes.PrimaryKey));
                    break;
            }

            this.DispatchEvent(eventToDispatch, model);
            this.DispatchEvent(Events.EventType.SAVED, model);
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

            if (this is IUseEvents)
            {
                string eventOperation = operation switch
                {
                    Transactions.Type.TYPE_INSERT => Events.EventType.CREATING,
                    Transactions.Type.TYPE_UPDATE => Events.EventType.UPDATING,
                    Transactions.Type.TYPE_DELETE => Events.EventType.DELETING,
                    _ => throw new NotImplementedException(),
                };

                this.DispatchEvent(eventOperation);
                this.DispatchEvent(Events.EventType.SAVING);
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
                        this.queryBuilder.AddInsertClause(item.Key, item.Value);
                        break;
                    case Transactions.Type.TYPE_UPDATE:
                        this.queryBuilder.AddUpdateClause(item.Key, item.Value);
                        break;
                    case Transactions.Type.TYPE_DELETE:
                        if (item.Key == TimestampManager.DELETED_AT && this.CanBeSoftDeleted())
                        {
                            this.queryBuilder.AddUpdateClause(item.Key, item.Value);
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

            this.queryBuilder.AddInsertManyClause(keyValues);
        }

        /// <summary>
        /// Creates a new model in the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IModel? Create()
        {
            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_INSERT);

            if (result.Status != SaveStatus.Error)
            {
                IModel? model = GetModel();

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
        public IModel? Update()
        {
            if (!this.Attributes.HasPrimaryKey())
            {
                throw new Exception("Primary key not set.");
            }

            string modelId = this.Attributes.GetValue(this.Attributes.PrimaryKey);

            QueryResult<SaveStatus> result = this.RunQuery(Transactions.Type.TYPE_UPDATE);

            if (result.Status != SaveStatus.Error)
            {
                IModel? model = GetModel(modelId);

                return model;
            }

            throw new Exception(result.Message);
        }

        public Collection CreateMany(ParamBag[] bags)
        {
            this.CompileManyToClause(bags, Transactions.Type.TYPE_INSERT);

            var result = TransactionManager.Save(this.queryBuilder);

            if (result.Status != SaveStatus.Error)
            {
                Collection models = this.OrderBy("id", "desc").Limit(bags.Length).Get();

                if (this is IUseEvents)
                {
                    foreach (var model in models)
                    {
                        this.DispatchEvent(Events.EventType.CREATED, model);
                        this.DispatchEvent(Events.EventType.SAVED, model);
                        this.DispatchEvent(Events.EventType.READ, model);
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
        private IModel? GetModel(string? id = null)
        {
            var tempModel = ModelRegistry.ModelByKey(this.GetTable()) as Model;

            if (string.IsNullOrEmpty(id))
            {
                return tempModel?.OrderBy("id", "desc")?.First();
            }

            return tempModel?.Find(id);
        }

        /// <summary>
        /// Executes the query and returns the first model.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <returns>The first model.</returns>
        public IModel? First()
        {
            this.queryBuilder.SetLimit(1);
            return this.Get().FirstOrDefault();
        }

        /// <summary>
        /// Handles precautions making sure the query is valid and executable
        /// </summary>
        private void HandlePrecautions()
        {
            if (this.Attributes.TimestampsEnabled() &&
                this is IUseTimestamps &&
                !this.queryBuilder.HasWhereColumn(TimestampManager.SOFT_DELETE_COLUMN))
            {
                this.WhereNull(TimestampManager.SOFT_DELETE_COLUMN);
            }

            if (this.queryBuilder.QueryType is null)
            {
                this.queryBuilder.AddSelectColumn("*");
            }
        }

        /// <summary>
        /// Adds an order to the query.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="order"></param>
        private void AddOrder(string column, string order)
        {
            this.queryBuilder.AddOrderBy(column, order);
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
            Events.Dispatcher dispatcher = App.Instance.EventDispatcher();

            IModel model = this as IModel;
            eventToDispatch = dispatcher.GenerateEventName(this.GetType().Name, eventToDispatch);

            dispatcher.Dispatch(eventToDispatch, model);
        }

        private void DispatchEvent(string eventToDispatch, IModel model)
        {
            Events.Dispatcher dispatcher = App.Instance.EventDispatcher();
            string id = model.Attributes.GetValue(model.Attributes.PrimaryKey);
            eventToDispatch = dispatcher.GenerateEventName(this.GetType().Name, eventToDispatch);
            dispatcher.Dispatch(eventToDispatch, model);
        }

        //private void RegisterEvents()
        //{
        //    if (this is IUseEvents)
        //    {
        //        var eventMethods = typeof(IEvent).GetMethods();
        //        foreach (var method in eventMethods)
        //        {
        //            Events.Dispatcher dispatcher = App.Instance.EventDispatcher();
        //            string eventName = dispatcher.GenerateEventName(this.GetTable(), method.Name, this.GetHashCode().ToString());

        //            Action<IModel> action = (Action<IModel>)Delegate.CreateDelegate(typeof(Action<IModel>), this, method);
                    
        //            App.Instance.EventDispatcher().AddListener(eventName, action);
        //        }
        //    }
        //}
    }
}
