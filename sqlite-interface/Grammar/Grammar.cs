using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Database.Console;
using Database.Migrations.Enum;
using Database.Relations;

namespace Database.Grammar
{
    public class GrammarDefinitions
    {
        protected string CompileInsert(string table, IDictionary<string, string> attributes, IList<string> rawAttributes)
        {
            StringBuilder query = new("insert into " + table);
            StringBuilder columns = new();
            StringBuilder values = new();

            string columnString = string.Empty;
            string valuesString = string.Empty;

            foreach (KeyValuePair<string, string> attr in attributes)
            {
                if (!string.IsNullOrEmpty(attr.Value) && !attr.Key.Equals("id"))
                {
                    columns.Append(attr.Key).Append(',');

                    if (rawAttributes.Contains(attr.Key))
                    {
                        values.Append('\'').Append(attr.Value).Append('\'').Append(',');
                    }
                    else
                    {
                        if (attr.Value.Contains('(') && attr.Value.Contains(')'))
                        {
                            values.Append(attr.Value).Append(',');
                        }
                        else
                        {
                            values.Append('\'').Append(attr.Value).Append('\'').Append(',');
                        }
                    }
                }
            }

            if (columns.Length > 0)
            {
                columnString = columns.Remove(columns.Length - 1, 1).ToString();
                valuesString = values.Remove(values.Length - 1, 1).ToString();
            }

            query.Append('(').Append(columnString).Append(')');
            query.Append(" values (").Append(valuesString).Append(')');
            query.Append(CompileEndOfString());

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
                .Append('\'')
                .Append(CompileEndOfString());

            return statement.ToString();
        }

        protected string CompileDelete(string table, IDictionary<string, string> attributes)
        {
            StringBuilder statement = new("delete from " + table + " where ");

            foreach (KeyValuePair<string, string> d in attributes)
            {
                statement.Append(d.Key + " = '" + d.Value + "' and ");
            }

            if (statement.ToString().EndsWith(" and "))
            {
                statement.Remove(statement.Length - 5, 5);
            }

            statement.Append(CompileEndOfString());

            return statement.ToString();
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
            return "create index if not exists idx_" + column + "_" + table + " on " + table + "(" + column + ")" + CompileEndOfString();
        }

        protected string CompileDropTable(string table)
        {
            return "PRAGMA foreign_keys = OFF; drop table if exists " + table +  "; PRAGMA foreign_keys = ON" + CompileEndOfString();
        }

        private string CompileDropElements(string elementType, List<string> elements)
        {
            StringBuilder query = new($"drop {elementType} if exists ");

            for (int i = 0; i < elements.Count; i++)
            {
                query.Append(elements[i]);
                if (i < elements.Count - 1)
                {
                    query.Append(", ");
                }
            }

            return query.Append(CompileEndOfString()).ToString();
        }

        protected string CompileDropColumns(string table, List<string> columns)
        {
            StringBuilder query = new("alter table ");
            query.Append(table)
                .Append(" drop ");

            for (int i = 0; i < columns.Count; i++)
            {
                query.Append(columns[i]);
                if (i < columns.Count - 1)
                {
                    query.Append(", ");
                }
            }

            return query.Append(CompileEndOfString()).ToString();
        }

        protected string CompileAddColumns(string table, List<string> columns)
        {
            StringBuilder query = new("alter table ");
            query.Append(table)
                .Append(" add ");

            for (int i = 0; i < columns.Count; i++)
            {
                query.Append(columns[i]);
                if (i < columns.Count - 1)
                {
                    query.Append(", ");
                }
            }

            return query.Append(CompileEndOfString()).ToString();
        }

        protected string CompileDropIndexes(List<string> indexes)
        {
            return CompileDropElements(DropType.INDEX, indexes);
        }

        protected string CompileDropForeignKeys(string table, List<string> foreignKeys)
        {
            StringBuilder query = new("alter table ");
            query.Append(table)
                .Append(" drop foreign key ");

            for (int i = 0; i < foreignKeys.Count; i++)
            {
                query.Append(foreignKeys[i]);
                if (i < foreignKeys.Count - 1)
                {
                    query.Append(", ");
                }
            }

            return query.Append(CompileEndOfString()).ToString();
        }

        protected string CompileDropTriggers(List<string> triggers)
        {
            return CompileDropElements(DropType.TRIGGER, triggers);
        }

        protected string CompileDropViews(List<string> views)
        {
            return CompileDropElements(DropType.VIEW, views);
        }
    }
}