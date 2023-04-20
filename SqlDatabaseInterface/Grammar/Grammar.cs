using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database.Relations;

namespace Database.Grammar
{
    public class GrammarDefinitions
    {
        protected string CompileSelect(string columns)
        {
            return "select " + columns;
        }

        protected string CompileFrom(string table)
        {
            return " from " + table;
        }

        protected string CompileInsert(string table, IDictionary<string, string> attributes)
        {
            StringBuilder query = new StringBuilder("insert into " + table);
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();

            string columnString = string.Empty;
            string valuesString = string.Empty;

            foreach (KeyValuePair<string, string> attr in attributes)
            {
                if (!string.IsNullOrEmpty(attr.Value) && !attr.Key.Equals("id"))
                {
                    columns.Append(attr.Key).Append(",");
                    if (attr.Value.Contains("(") && attr.Value.Contains(")"))
                    {
                        values.Append(attr.Value).Append(",");
                    } else
                    {
                        values.Append("'").Append(attr.Value).Append("'").Append(",");
                    }
                }
            }

            if (columns.Length > 0)
            {
                columnString = columns.Remove(columns.Length - 1, 1).ToString();
                valuesString = values.Remove(values.Length - 1, 1).ToString();
            }

            query.Append("(").Append(columnString).Append(")");
            query.Append(" values (").Append(valuesString).Append(")");

            return query.ToString();
        }

        protected string CompileUpdate(string table, IDictionary<string, string> attributes)
        {
            StringBuilder statement = new StringBuilder("update ").Append(table).Append(" set ");

            foreach (KeyValuePair<string, string> pair in attributes)
            {
                statement.Append(pair.Key + " = '" + pair.Value + "',");
            }

            if (statement.ToString().EndsWith(","))
            {
                statement.Remove(statement.Length - 1, 1);
            }

            statement.Append(" where ")
                .Append(attributes.First().Key)
                .Append(" = '")
                .Append(attributes.First().Value)
                .Append("'")
                .Append(this.CompileEndOfString());

            return statement.ToString();
        }

        protected string CompileDelete(string table, IDictionary<string, string> attributes)
        {
            StringBuilder statement = new StringBuilder("delete from " + table + " where ");

            foreach (KeyValuePair<string, string> d in attributes)
            {
                statement.Append(d.Key + " = '" + d.Value + "',");
            }

            if (statement.ToString().EndsWith(","))
            {
                statement.Remove(statement.Length - 1, 1);
            }

            statement.Append(this.CompileEndOfString());

            return statement.ToString();
        }

        /// <summary>
        /// Foreign id in foreign table
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        protected string CompileJoinTo(Join relation)
        {
            return " " + relation.Type + " join " + relation.Table + " on " + relation.MainTable + "." + relation.Id + " = " + relation.Table + "." + relation.ForeignId;
        }

        /// <summary>
        /// foreign Id in own table
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        protected string CompileJoinFrom(Join relation)
        {
            return " " + relation.Type + " join " + relation.MainTable + " on " + relation.Table + "." + relation.Id + " = " + relation.MainTable + "." + relation.ForeignId;
        }

        protected string CompileWhere(string wheres)
        {
            return " where " + wheres;
        }

        protected string CompileJoin(string join)
        {
            return " " + join;
        }

        protected string CompileEndOfString()
        {
            return ";";
        }

        protected string CompileCreateTable(string table)
        {
            return @"create table if not exists " + table + "(";
        }

        protected string CompileAlterTable(string table)
        {
            return @"alter table " + table + " add ";
        }

        protected string CompileColumn(string column, string definitions)
        {
            return column + " " + definitions;
        }

        protected string CompileForeignKey(string foreign, string table)
        {
            return "foreign key(" + foreign + ") " + table;
        }

        protected string CompileIndex(string column, string table)
        {
            return "create unique index if not exists idx_" + column + "_" + table + " on " + table + "(" + column + ")" + this.CompileEndOfString();
        }
    }
}