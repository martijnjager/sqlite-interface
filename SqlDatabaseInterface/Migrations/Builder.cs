using Database.Grammar;
using Database.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDatabaseInterface.Migrations
{
    internal class Builder
    {
        public static string Handle(Blueprint blueprint)
        {
            StringBuilder query = new StringBuilder();

            List<Tuple<string, string>> definitions = blueprint.GetDefinitions();

            string table = definitions[0].Item1;

            query.Append(GrammarCompiler.CompileCreateTableStatement(table));
            definitions.RemoveAt(0);

            foreach (Tuple<string, string> definition in definitions)
            {
                query.Append(GrammarCompiler.CompileColumnStatement(definition.Item1, definition.Item2));
                query.Append(",");
            }

            foreach (Tuple<string, string> relation in blueprint.GetRelations())
            {
                query.Append(GrammarCompiler.CompileForeignKeyStatement(relation.Item1, relation.Item2));
                query.Append(",");
            }

            if (query.ToString().EndsWith(","))
            {
                query.Remove(query.Length - 1, 1);
                query.Append(");");
            }

            foreach (string index in blueprint.GetIndexes())
            {
                query.Append(GrammarCompiler.CompileIndexStatement(index, table));
            }

            return query.ToString();
        }
    }
}
