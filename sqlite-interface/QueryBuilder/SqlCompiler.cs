using Database.Clauses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Database.QueryBuilder
{
    internal class SqlCompiler
    {
        public ClauseManager clauses;

        public string table;

        /// <summary>
        /// Compiles the query into a transaction.
        /// </summary>
        /// <returns>The compiled transaction.</returns>
        public string Compile(QueryBuilder.Builder builder)
        {
            this.clauses = builder.clauses;
            this.table = clauses.table;
            StringBuilder query = new("");

            if (clauses.update.HasConditions() && !clauses.delete.HasConditions())
            {
                query = CompileUpdateQuery(query);
            }

            if (clauses.delete.HasConditions())
            {
                query = CompileDeleteQuery(query);
            }

            if (clauses.insert.HasConditions() || clauses.insertMany.HasConditions())
            {
                query = CompileInsertQuery(query);
            }

            if (!clauses.update.HasConditions() && !clauses.delete.HasConditions() &&
                !clauses.insert.HasConditions() && !clauses.insertMany.HasConditions())
            {
                query = CompileSelectQuery(query);
            }

            return query.ToString();
        }

        private StringBuilder CompileInsertQuery(StringBuilder query)
        {
            query.Append("INSERT INTO ").Append(this.table);

            if (this.clauses.insert.HasConditions())
            {
                query.Append(this.clauses.insert.Compile());
            }
            else if (this.clauses.insertMany.HasConditions())
            {
                query.Append(this.clauses.insertMany.Compile());
            }
            query.Append(';');
            return query;
        }

        private StringBuilder CompileUpdateQuery(StringBuilder query)
        {
            query.Append("UPDATE ").Append(this.clauses.table);

            query.Append(this.clauses.update.Compile());

            if (this.clauses.wheres.HasConditions())
            {
                query.Append(" WHERE ").Append(this.clauses.wheres.Compile());
            }

            query.Append(';');

            return query;
        }

        private StringBuilder CompileSelectQuery(StringBuilder query)
        {
            query.Append(this.clauses.selects.Compile());
            query.Append(" FROM ").Append(this.table);
            query.Append(this.clauses.joins.Compile());

            if (this.clauses.wheres.HasConditions())
            {
                query.Append(" WHERE ").Append(this.clauses.wheres.Compile());
            }

            string groupBy = string.Join(", ", this.clauses.groups);

            if (groupBy.Length > 0)
            {
                query.Append(" GROUP BY ").Append(groupBy);
            }

            if (this.clauses.havings.HasConditions())
            {
                query.Append(" HAVING ").Append(this.clauses.havings.Compile());
            }

            string orderBy = string.Join(", ", this.clauses.orderBys.Select(x => x.Key + " " + x.Value));

            if (orderBy.Length > 0)
            {
                query.Append(" ORDER BY ").Append(orderBy);
            }

            if (this.clauses.Limit > 0)
            {
                query.Append(" LIMIT ").Append(this.clauses.Limit);
            }

            query.Append(';');

            return query;
        }

        private StringBuilder CompileDeleteQuery(StringBuilder query)
        {
            bool softDelete = this.clauses.delete.GetConditions<bool>()[0];

            if (softDelete)
            {
                query.Append("UPDATE ")
                    .Append(this.table)
                    .Append(this.clauses.update.Compile());

                if (this.clauses.wheres.HasConditions())
                {
                    query.Append(" WHERE ").Append(this.clauses.wheres.Compile());
                }
            }
            else
            {
                query.Append("DELETE FROM ").Append(this.table);

                if (this.clauses.wheres.HasConditions())
                {
                    query.Append(" WHERE ").Append(this.clauses.wheres.Compile());
                }
                string orderBy = string.Join(", ", this.clauses.orderBys.Select(x => x.Key + " " + x.Value));

                if (orderBy.Length > 0)
                {
                    query.Append(" ORDER BY ").Append(orderBy);
                }

                if (this.clauses.Limit > 0)
                {
                    query.Append(" LIMIT ").Append(this.clauses.Limit);
                }
            }

            query.Append(';');
            return query;
        }
    }
}
