using Database.Grammar;
using Database.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Database.Migrations
{
    internal class Builder
    {
        public static string Handle(Blueprint blueprint)
        {
            StringBuilder query = new StringBuilder();

            if (blueprint.GetDefinitions().Count > 0 )
            {
                ProcessCreate(blueprint, query);
            } else
            {
                ProcessDrop(blueprint, query);
            }

            return query.ToString();
        }

        private static void ProcessCreate(Blueprint blueprint, StringBuilder query)
        {
            string table = blueprint.GetDefinitions()[0].Item1;

            query.Append(GrammarCompiler.CompileCreateTableStatement(table));
            blueprint.GetDefinitions().RemoveAt(0);

            foreach (Tuple<string, string> definition in blueprint.GetDefinitions())
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
        }
        
        private static void ProcessDrop(Blueprint blueprint, StringBuilder query)
        {
            List<string> droppable = blueprint.GetDroppable();

            // Drop table
            if (droppable.Count == 1)
            {
                query.Append(GrammarCompiler.CompileDropTableStatement(droppable[0]));
            } else
            {

            }
        }
    }
}
