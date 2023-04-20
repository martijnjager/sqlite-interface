using System.Collections.Generic;
using System.Linq;

namespace Database.Grammar
{
    public class GrammarCompiler : GrammarDefinitions
    {
        private string[] options;

        private string[] order;

        public GrammarCompiler()
        {
            this.options = new string[]
            {
                "select", "from", "join", "where", "groupBy", "having", "orderBy", "limit"
            };

            this.order = new string[]
            {
                "select", "from", "join", "where", "groupBy", "having", "orderBy", "limit"
            };
        }

        public static string Compile(IDictionary<string, string> clauses, List<Relations.Join> joins)
        {
            return InstanceContainer.Get("grammarCompiler").GetValues(clauses, joins);
        }

        public static string CompileInsertStatement(string table, IDictionary<string, string> attributes)
        {
            return InstanceContainer.Get("grammarCompiler").CompileInsert(table, attributes);
        }

        public static string CompileUpdateStatement(string table, IDictionary<string, string> attributes)
        {
            return InstanceContainer.Get("grammarCompiler").CompileUpdate(table, attributes);
        }

        public static string CompileDeleteStatement(Model model, IDictionary<string, string> attributes)
        {
            string table = model.GetTable();

            if (attributes.ContainsKey("deleted_at"))
            {
                return CompileUpdateStatement(table, attributes);
            }

            return InstanceContainer.Get("grammarCompiler").CompileDelete(table, attributes);
        }

        public static string CompileCreateTableStatement(string table)
        {
            return InstanceContainer.Get("grammarCompiler").CompileCreateTable(table);
        }

        public static string CompileAlterTableStatement(string table)
        {
            return InstanceContainer.Get("grammarCompiler").CompileAlterTable(table);
        }

        public static string CompileColumnStatement(string column, string definitions)
        {
            return InstanceContainer.Get("grammarCompiler").CompileColumn(column, definitions);
        }

        public static string CompileForeignKeyStatement(string column, string definitions)
        {
            return InstanceContainer.Get("grammarCompiler").CompileForeignKey(column, definitions);
        }

        public static string CompileIndexStatement(string column, string table)
        {
            return InstanceContainer.Get("grammarCompiler").CompileIndex(column, table);
        }

        private string GetValues(IDictionary<string, string> clauses, List<Relations.Join> joins)
        {
            string query = "";

            foreach (string item in order)
            {
                if (item == "join")
                {
                    foreach (Relations.Join join in joins)
                    {
                        if (join.Direction == "to")
                        {
                            query += this.CompileJoinTo(join);
                        }
                        if (join.Direction == "from")
                        {
                            query += this.CompileJoinFrom(join);
                        }
                    }
                }

                if (clauses.ContainsKey(item))
                {
                    query += this.PrepareClause(item, clauses[item]);
                }
            }

            return query + this.CompileEndOfString();
        }

        private string PrepareClause(string property, dynamic value)
        {
            string sql = "";

            if (this.options.Contains(property) && !string.IsNullOrEmpty(property))
            {
                switch (property)
                {
                    case "select":
                        sql = this.CompileSelect(value);
                        break;
                    case "from":
                        sql = this.CompileFrom(value);
                        break;
                    case "where":
                        sql = this.CompileWhere(value);
                        break;
                    case "join":
                        sql = this.CompileJoin(value);
                        break;
                }
            }

            return sql;
        }
    }
}