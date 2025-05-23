using Database.Attribute;
using Database.Clauses;
using Database.Contracts;
using Database.Contracts.Relation;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Database.QueryBuilder
{
    public class Builder
    {
        internal readonly ClauseManager clauses;
        private readonly SqlCompiler compiler;

        public readonly List<string> ExcludeColumns = [];
        public List<ToLoad> Relations { get; private set; } = [];
        public Transactions.Type? QueryType { get; private set; } = null;

        public Builder(string table)
        {
            clauses = new ClauseManager(table);
            compiler = new SqlCompiler();
        }

        public string GetTable() => clauses.table;

        public IDictionary<string, string> GetParameters()
        {
            var combinedParameters = new Dictionary<string, string>();

            foreach (var clause in clauses.GetAllClauses())
            {
                if (clause.HasConditions())
                {
                    var parameters = clause.Parameters;
                    foreach (var parameter in parameters)
                    {
                        Console.BaseCommand.WriteLine($"Parameter: {parameter.Key} = {parameter.Value}");
                        combinedParameters[parameter.Key] = parameter.Value;
                    }
                }
            }

            return combinedParameters;
        }

        public string Compile()
        {
            string query = compiler.Compile(this);

            if (string.IsNullOrEmpty(query))
            {
                throw new InvalidOperationException("Query is empty.");
            }

            return query;
        }

        private void SetType(Transactions.Type type)
        {
            this.QueryType = type;
        }

        /// <summary>
        /// Sets the limit for the query.
        /// </summary>
        /// <param name="limit">The limit value.</param>
        public void SetLimit(int limit)
        {
            this.clauses.Limit = limit;
        }

        /// <summary>
        /// Adds a WHERE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddWhereClause(string key, string op, object value)
        {
            clauses.wheres.AddWhereClause(key, op, value);
        }

        /// <summary>
        /// Adds a WHERE IN clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="values">The values.</param>
        public void AddWhereInClause(string key, params object[] values)
        {
            clauses.wheres.AddWhereInClause(key, values);
        }

        /// <summary>
        /// Adds an OR WHERE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddOrWhereClause(string key, string op, object value)
        {
            clauses.wheres.AddOrWhereClause(key, op, value);
        }

        /// <summary>
        /// Adds a nested WHERE clause to the query.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model)
        {
            clauses.wheres.AddNestedWhereClause(callback, model);
        }

        /// <summary>
        /// Adds a HAVING clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddHavingClause(string key, string op, object value)
        {
            clauses.havings.AddHavingClause(key, op, value);
        }

        /// <summary>
        /// Adds an OR HAVING clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="op">The operator.</param>
        /// <param name="value">The value.</param>
        public void AddOrHavingClause(string key, string op, object value)
        {
            clauses.havings.AddOrHavingClause(key, op, value);
        }

        /// <summary>
        /// Adds a nested HAVING clause to the query.
        /// </summary>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddNestedHavingClause(Action<Eloquent> callback, Eloquent model)
        {
            clauses.havings.AddNestedHavingClause(callback, model);
        }

        /// <summary>
        /// Adds a column to the SELECT clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="alias">The column alias.</param>
        public void AddSelectColumn(string column, string alias = "")
        {
            this.SetType(Transactions.Type.TYPE_SELECT);
            clauses.AddSelectColumn(column, alias);
        }

        public void DontSelectColumn(string column)
        {
            ExcludeColumns.Add(column);
        }

        public void RemoveDontSelect(string column)
        {
            ExcludeColumns.Remove(column);
        }

        public void SetRelationsToLoad(List<ToLoad> relations)
        {
            this.Relations = relations;
        }

        /// <summary>
        /// Adds a column to the GROUP BY clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        public void AddGroupBy(string column)
        {
            clauses.AddGroupBy(column);
        }

        /// <summary>
        /// Adds a column to the ORDER BY clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="direction">The sort direction.</param>
        public void AddOrderBy(string column, string direction = "asc")
        {
            clauses.AddOrderBy(column, direction);
        }

        /// <summary>
        /// Checks if the WHERE clause contains a specific column.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <returns>True if the column exists in the WHERE clause, otherwise false.</returns>
        public bool HasWhereColumn(string column)
        {
            return clauses.wheres.HasWhereColumn(column);
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
            clauses.joins.AddJoinClause(table, first, op, second);
        }

        /// <summary>
        /// Adds a WHERE HAS clause to the query.
        /// </summary>
        /// <param name="relation">The relation name.</param>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model, IJoin join)
        {
            clauses.wheres.AddWhereHasClause(relation, callback, model, join);
        }

        /// <summary>
        /// Adds a WHERE HAS clause to the query.
        /// </summary>
        /// <param name="relation"></param>
        /// <param name="model"></param>
        /// <param name="join"></param>
        public void AddWhereHasClause(string relation, Eloquent model, IJoin join)
        {
            clauses.wheres.AddWhereHasClause(relation, model, join);
        }

        /// <summary>
        /// Adds a WHERE HAS NOT clause to the query.
        /// </summary>
        /// <param name="relation">The relation name.</param>
        /// <param name="callback">The callback function.</param>
        /// <param name="model">The model.</param>
        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model, IJoin join)
        {
            clauses.wheres.AddWhereHasNotClause(relation, callback, model, join);
        }

        /// <summary>
        /// Adds a WHERE HAS NOT clause to the query.
        /// </summary>
        /// <param name="relation"></param>
        /// <param name="model"></param>
        /// <param name="join"></param>
        public void AddWhereHasNotClause(string relation, Eloquent model, IJoin join)
        {
            clauses.wheres.AddWhereHasNotClause(relation, model, join);
        }

        /// <summary>
        /// Adds an UPDATE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="value">The value.</param>
        public void AddUpdateClause(string key, string value)
        {
            this.SetType(Transactions.Type.TYPE_UPDATE);
            clauses.AddUpdateClause(key, value);
        }

        /// <summary>
        /// Adds a DELETE clause to the query.
        /// </summary>
        public void AddDeleteClause()
        {
            this.SetType(Transactions.Type.TYPE_DELETE);
            clauses.AddDeleteClause();
        }

        /// <summary>
        /// Adds a soft delete clause to the query.
        /// </summary>
        public void AddSoftDeleteClause()
        {
            this.SetType(Transactions.Type.TYPE_DELETE);
            clauses.AddSoftDeleteClause();
        }

        public void AddRestoreClause()
        {
            this.SetType(Transactions.Type.TYPE_UPDATE);
            clauses.AddUpdateClause(TimestampManager.DELETED_AT, "null");
        }

        /// <summary>
        /// Adds an INSERT clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="value">The value.</param>
        public void AddInsertClause(string key, string value)
        {
            this.SetType(Transactions.Type.TYPE_INSERT);
            clauses.AddInsertClause(key, value);
        }

        public void AddInsertManyClause(List<IDictionary<string, string>> keyValues)
        {
            this.SetType(Transactions.Type.TYPE_INSERT);
            clauses.AddInsertManyClause(keyValues);
        }
    }
}
