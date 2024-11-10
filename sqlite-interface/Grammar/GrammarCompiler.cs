using Database.Attribute;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Database.Grammar
{
    public class GrammarCompiler : GrammarDefinitions
    {
        public GrammarCompiler()
        {
        }

        public static string CompileInsertStatement(string table, IDictionary<string, string> attributes, IList<string> rawAttributes)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileInsert(table, attributes, rawAttributes);
        }

        public static string CompileUpdateStatement(string table, IDictionary<string, string> attributes)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileUpdate(table, attributes);
        }

        public static string CompileDeleteStatement(Model model, IDictionary<string, string> attributes)
        {
            string table = model.GetTable();

            if (attributes.ContainsKey(TimestampManager.DELETED_AT))
            {
                return CompileUpdateStatement(table, attributes);
            }

            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDelete(table, attributes);
        }

        public static string CompileCreateTableStatement(string table)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileCreateTable(table);
        }

        public static string CompileAlterTableStatement(string table)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileAlterTable(table);
        }

        public static string CompileColumnStatement(string column, string definitions)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileColumn(column, definitions);
        }

        public static string CompileForeignKeyStatement(string column, string definitions)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileForeignKey(column, definitions);
        }

        public static string CompileIndexStatement(string column, string table)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileIndex(column, table);
        }

        public static string CompileDropTableStatement(string table)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDropTable(table);
        }

        public static string CompileDropColumnsStatement(string table, List<string> columns)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDropColumns(table, columns);
        }

        public static string CompileDropIndexesStatement(List<string> indexes)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDropIndexes(indexes);
        }

        public static string CompileDropForeignKeysStatement(string table, List<string> foreignKeys)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDropForeignKeys(table, foreignKeys);
        }

        public static string CompileDropTriggersStatement(List<string> triggers)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDropTriggers(triggers);
        }

        public static string CompileDropViewsStatement(List<string> triggers)
        {
            return InstanceContainer.Get<GrammarCompiler>("GrammarCompiler").CompileDropViews(triggers);
        }
    }
}