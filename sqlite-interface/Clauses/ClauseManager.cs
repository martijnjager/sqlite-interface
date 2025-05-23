using Database.Attribute;
using Database.Contracts;
using Database.Contracts.Clause;
using Database.Relations;
using Database.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
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
        public IWhereClause wheres;
        public IHavingClause havings;
        public ISelectClause selects;
        public IJoinClause joins;
        public IDeleteClause delete;
        public IUpdateClause update;
        public IInsertClause insert;
        public IInsertManyClause insertMany;

        public List<string> groups = [];
        public IDictionary<string, string> orderBys;

        public string table;
        
        public int Limit = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClauseManager"/> class.
        /// </summary>
        /// <param name="table">The table name.</param>
        public ClauseManager(string table)
        {
            wheres = new WhereClause();
            selects = new SelectClause();
            havings = new HavingClause();
            joins = new JoinClause();
            delete = new DeleteClause();
            update = new UpdateClause();
            insert = new InsertClause();
            insertMany = new InsertManyClause();

            orderBys = new Dictionary<string, string>();
            //parameters = new Dictionary<string, string>();

            this.table = table;
        }

        /// <summary>
        /// Clears all the clauses.
        /// </summary>
        public void ClearClauses()
        {
            groups.Clear();
            //parameters.Clear();
            wheres.Clear();
            selects.Clear();
            orderBys.Clear();
            havings.Clear();
            joins.Clear();
            delete.Clear();
            update.Clear();
            insert.Clear();
            insertMany.Clear();
        }

        /// <summary>
        /// Adds a column to the SELECT clause of the query.
        /// </summary>
        /// <param name="column">The column name.</param>
        /// <param name="alias">The column alias.</param>
        public void AddSelectColumn(string column, string alias = "")
        {
            selects.AddColumn(column, alias);
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
        /// Adds an UPDATE clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="value">The value.</param>
        public void AddUpdateClause(string key, string value)
        {
            update.AddUpdateClause(key, value);
        }

        /// <summary>
        /// Adds a DELETE clause to the query.
        /// </summary>
        public void AddDeleteClause()
        {
            delete.AddDeleteClause();
        }

        /// <summary>
        /// Adds a soft delete clause to the query.
        /// </summary>
        public void AddSoftDeleteClause()
        {
            delete.AddSoftDeleteClause();
        }

        public void AddRestoreClause()
        {
            update.AddUpdateClause(TimestampManager.DELETED_AT, "null");
        }

        /// <summary>
        /// Adds an INSERT clause to the query.
        /// </summary>
        /// <param name="key">The column name.</param>
        /// <param name="value">The value.</param>
        public void AddInsertClause(string key, string value)
        {
            insert.AddInsertClause(key, value);
        }

        public void AddInsertManyClause(List<IDictionary<string, string>> keyValues)
        {
            insertMany.AddInsertManyClause(keyValues);
        }

        /// <summary>
        /// Gets all clause objects managed by this ClauseManager.
        /// </summary>
        /// <returns>A list of all clause objects.</returns>
        public List<IClause> GetAllClauses()
        {
            return [
                wheres,
                havings,
                selects,
                joins,
                delete,
                update,
                insert,
                insertMany
            ];
        }
    }
}
