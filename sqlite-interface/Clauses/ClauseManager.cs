using Database.Attribute;
using Database.Contracts.Clause;
using Database.Relations;
using Database.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    /// <summary>
    /// Manages the clauses for constructing SQL queries.
    /// </summary>
    public class ClauseManager
    {
        protected List<string> groups;
        protected IDictionary<string, string> orderBys;

        protected IWhereClause wheres;

        protected IHavingClause havings;

        protected ISelectClause selects;

        protected IJoinClause joins;

        protected IDeleteClause delete;

        protected IUpdateClause update;

        protected IInsertClause insert;

        protected IInsertManyClause insertMany;

        protected int Limit;

        protected readonly IDictionary<string, string> parameters;

        protected string table;

        public readonly List<string> ExcludeColumns;

        public List<ToLoad> Relations { get; private set; }

        public Transactions.Type? QueryType { get; private set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClauseManager"/> class.
        /// </summary>
        /// <param name="table">The table name.</param>
        public ClauseManager(string table)
        {
            groups = new List<string>();
            parameters = new Dictionary<string, string>();
            wheres = new WhereClause();
            selects = new SelectClause();
            orderBys = new Dictionary<string, string>();
            havings = new HavingClause();
            joins = new JoinClause();
            delete = new DeleteClause();
            update = new UpdateClause();
            insert = new InsertClause();
            insertMany = new InsertManyClause();

            this.table = table;
            this.Limit = -1;
            this.ExcludeColumns = new List<string>();
        }

        public void SetRelationsToLoad(List<ToLoad> relations)
        {
            this.Relations = relations;
        }

        /// <summary>
        /// Clears all the clauses.
        /// </summary>
        public void ClearClauses()
        {
            groups.Clear();
            parameters.Clear();
            wheres.Clear();
            selects.Clear();
            orderBys.Clear();
            havings.Clear();
            joins.Clear();
            delete.Clear();
            update.Clear();
            insert.Clear();
            insertMany.Clear();
            QueryType = null;
            Relations?.Clear();
        }

        private void SetType(Transactions.Type type)
        {
            //if (this.QueryType is not null && this.QueryType != type)
            //{
            //    throw new Exception("Cannot change type of query");
            //}

            this.QueryType = type;
        }

        /// <summary>
        /// Adds a WHERE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddWhereClause(string key, string op, string value)
        {
            wheres.AddWhereClause(key, op, value);
        }

        /// <summary>
        /// Adds a WHERE IN clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="values">The values.</param>
        public void AddWhereInClause(string key, params object[] values)
        {
            wheres.AddWhereInClause(key, values);
        }

        /// <summary>
        /// Adds an OR WHERE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddOrWhereClause(string key, string op, string value)
        {
            wheres.AddOrWhereClause(key, op, value);
        }

        /// <summary>
        /// Adds a nested WHERE clause to the query.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model)
        {
            wheres.AddNestedWhereClause(callback, model);
        }

        /// <summary>
        /// Adds a HAVING clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddHavingClause(string key, string op, string value)
        {
            havings.AddHavingClause(key, op, value);
        }

        /// <summary>
        /// Adds an OR HAVING clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddOrHavingClause(string key, string op, string value)
        {
            havings.AddOrHavingClause(key, op, value);
        }

        /// <summary>
        /// Adds a nested HAVING clause to the query.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddNestedHavingClause(Action<Eloquent> callback, Eloquent model)
        {
            havings.AddNestedHavingClause(callback, model);
        }

        /// <summary>
        /// Adds a column to the SELECT clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="alias">The column alias.</param>
        public void AddSelectColumn(string column, string alias = "")
        {
            this.SetType(Transactions.Type.TYPE_SELECT);
            selects.AddColumn(column, alias);
        }

        public void DontSelectColumn(string column)
        {
            ExcludeColumns.Add(column);
        }

        public void RemoveDontSelect(string column)
        {
            ExcludeColumns.Remove(column);
        }

        /// <summary>
        /// Adds a column to the GROUP BY clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        public void AddGroupBy(string column)
        {
            groups.Add(column);
        }

        /// <summary>
        /// Adds a column to the ORDER BY clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="direction">The sort direction.</param>
        public void AddOrderBy(string column, string direction = "asc")
        {
            orderBys.Add(column, direction);
        }

        /// <summary>
        /// Checks if the WHERE clause contains a specific column.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>True if the column exists in the WHERE clause, otherwise false.</returns>
        public bool HasWhereColumn(string column)
        {
            return wheres.HasWhereColumn(column);
        }

        /// <summary>
        /// Adds a JOIN clause to the query.
        /// </summary>
        /// <param name="table">The table name.</param>
        /// <param name="first">The first column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="second">The second column name.</param>
        public void AddJoinClause(string table, string first, string op, string second)
        {
            joins.AddJoinClause(table, first, op, second);
        }

        /// <summary>
        /// Adds a WHERE HAS clause to the query.
        /// </summary>
        /// <param name="relation">The relation name.</param>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            wheres.AddWhereHasClause(relation, callback, model);
        }

        /// <summary>
        /// Adds a WHERE HAS NOT clause to the query.
        /// </summary>
        /// <param name="relation">The relation name.</param>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            wheres.AddWhereHasNotClause(relation, callback, model);
        }

        /// <summary>
        /// Adds an UPDATE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="value">The value.</param>
        public void AddUpdateClause(string key, string value)
        {
            this.SetType(Transactions.Type.TYPE_UPDATE);
            update.AddUpdateClause(key, value);
        }

        /// <summary>
        /// Adds a DELETE clause to the query.
        /// </summary>
        public void AddDeleteClause()
        {
            this.SetType(Transactions.Type.TYPE_DELETE);
            delete.AddDeleteClause();
        }

        /// <summary>
        /// Adds a soft delete clause to the query.
        /// </summary>
        public void AddSoftDeleteClause()
        {
            this.SetType(Transactions.Type.TYPE_DELETE);
            delete.AddSoftDeleteClause();
        }

        public void AddRestoreClause()
        {
            this.SetType(Transactions.Type.TYPE_UPDATE);
            update.AddUpdateClause(TimestampManager.DELETED_AT, "null");
        }

        /// <summary>
        /// Adds an INSERT clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="value">The value.</param>
        public void AddInsertClause(string key, string value)
        {
            this.SetType(Transactions.Type.TYPE_INSERT);
            insert.AddInsertClause(key, value);
        }

        public void AddInsertManyClause(List<IDictionary<string, string>> keyValues)
        {
            this.SetType(Transactions.Type.TYPE_INSERT);
            insertMany.AddInsertManyClause(keyValues);
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        /// <returns>The table name.</returns>
        public string GetTable()
        {
            return table;
        }

        /// <summary>
        /// Sets the limit for the query.
        /// </summary>
        /// <param name="limit">The limit value.</param>
        public void SetLimit(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// Compiles the query into a transaction.
        /// </summary>
        /// <returns>The compiled transaction.</returns>
        public Transactions.Transaction Compile()
        {
            StringBuilder query = new("");

            if (update.HasConditions() && !delete.HasConditions())
            {
                query = CompileUpdateQuery(query);
            }

            if (delete.HasConditions())
            {
                query = CompileDeleteQuery(query);
            }

            if (insert.HasConditions() || insertMany.HasConditions())
            {
                query = CompileInsertQuery(query);
            }

            if (!update.HasConditions() && !delete.HasConditions() &&
                !insert.HasConditions() && !insertMany.HasConditions())
            {
                query = CompileSelectQuery(query);
            }

            var conn = InstanceContainer.Instance.ConnectionManager().GetConnection().SqliteConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }
            var transaction = conn.BeginTransaction();

            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = query.ToString();
            command.CommandType = System.Data.CommandType.Text;
            this.Bind(command);

            return new Transactions.Transaction(conn, command, transaction);
        }

        protected SQLiteCommand Bind(SQLiteCommand command)
        {
            if (wheres.HasConditions())
            {
                wheres.Bind(command);
            }

            if (havings.HasConditions())
            {
                havings.Bind(command);
            }

            if (insert.HasConditions())
            {
                insert.Bind(command);
            }

            if (insertMany.HasConditions())
            {
                insertMany.Bind(command);
            }

            if (update.HasConditions())
            {
                update.Bind(command);
            }

            if (delete.HasConditions())
            {
                delete.Bind(command);
            }

            return command;
        }

        private StringBuilder CompileInsertQuery(StringBuilder query)
        {
            query.Append("INSERT INTO ").Append(table);

            if (insert.HasConditions())
            {
                query.Append(insert.Compile());
            } else if (insertMany.HasConditions())
            {
                query.Append(insertMany.Compile());
            }
            query.Append(';');

            return query;
        }

        private StringBuilder CompileUpdateQuery(StringBuilder query)
        {
            query.Append("UPDATE ").Append(table);

            query.Append(update.Compile());

            if (wheres.HasConditions())
            {
                query.Append(" WHERE ").Append(wheres.Compile());
            }

            query.Append(';');

            return query;
        }

        private StringBuilder CompileSelectQuery(StringBuilder query)
        {
            query.Append(selects.Compile());
            query.Append(" FROM ").Append(table);
            query.Append(joins.Compile());

            if (wheres.HasConditions())
            {
                query.Append(" WHERE ").Append(wheres.Compile());
            }

            string groupBy = string.Join(", ", groups);

            if (groupBy.Length > 0)
            {
                query.Append(" GROUP BY ").Append(groupBy);
            }

            if (havings.HasConditions())
            {
                query.Append(" HAVING ").Append(havings.Compile());
            }

            string orderBy = string.Join(", ", orderBys.Select(x => x.Key + " " + x.Value));

            if (orderBy.Length > 0)
            {
                query.Append(" ORDER BY ").Append(orderBy);
            }

            if (Limit > 0)
            {
                query.Append(" LIMIT ").Append(Limit);
            }

            query.Append(';');

            return query;
        }

        private StringBuilder CompileDeleteQuery(StringBuilder query)
        {
            bool softDelete = delete.GetConditions<bool>()[0];

            if (softDelete)
            {
                query.Append("UPDATE ")
                    .Append(table)
                    .Append(update.Compile());

                if (wheres.HasConditions())
                {
                    query.Append(" WHERE ").Append(wheres.Compile());
                }
            }
            else
            {
                query.Append("DELETE FROM ").Append(table);

                if (wheres.HasConditions())
                {
                    query.Append(" WHERE ").Append(wheres.Compile());
                }
                string orderBy = string.Join(", ", orderBys.Select(x => x.Key + " " + x.Value));

                if (orderBy.Length > 0)
                {
                    query.Append(" ORDER BY ").Append(orderBy);
                }

                if (Limit > 0)
                {
                    query.Append(" LIMIT ").Append(Limit);
                }
            }

            query.Append(';');
            return query;
        }
    }
}
