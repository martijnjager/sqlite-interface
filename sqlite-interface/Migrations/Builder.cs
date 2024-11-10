using Database.Grammar;
using Database.Migrations.Enum;
using System.Text;

namespace Database.Migrations
{
    internal class Builder
    {
        public static string Handle(BlueprintBase blueprint)
        {
            StringBuilder query = new();

            if (!blueprint.HasDropData() && !blueprint.HasAlterData())
            {
                ProcessCreate(blueprint, query);
            }

            if (blueprint.HasDropData())
            {
                ProcessDrop(blueprint, query);
            }

            if (blueprint.HasAlterData())
            {
                ProcessAlter(blueprint, query);
            }

            return query.ToString();
        }

        private static void ProcessCreate(BlueprintBase blueprint, StringBuilder query)
        {
            AddTableNameIntoQuery(blueprint, query);

            AppendDefinitionsAndRelations(blueprint, query);

            AppendIndexes(blueprint, query);
        }
        
        private static void ProcessDrop(BlueprintBase blueprint, StringBuilder query)
        {
            IDictionary<string, List<string>> droppable = blueprint.GetDroppable();

            // Drop table
            if (droppable.Count == 1 && blueprint.GetTable() is null)
            {
                query.Append(GrammarCompiler.CompileDropTableStatement(droppable[DropType.TABLE][0]));
            } else
            {
                string table = blueprint.GetTable() ?? throw new Exception("Table name cannot be missing for dropping columns");
                foreach (string key in droppable.Keys)
                {
                    switch (key)
                    {
                        case DropType.INDEX:
                            query.Append(GrammarCompiler.CompileDropIndexesStatement(droppable[DropType.INDEX]));
                            break;
                        case DropType.TRIGGER:
                            query.Append(GrammarCompiler.CompileDropTriggersStatement(droppable[DropType.TRIGGER]));
                            break;
                        case DropType.VIEW:
                            query.Append(GrammarCompiler.CompileDropViewsStatement(droppable[DropType.VIEW]));
                            break;
                        case DropType.TABLE:
                            query.Append(GrammarCompiler.CompileDropTableStatement(droppable[DropType.TABLE][0]));
                            break;
                        case DropType.COLUMN:
                            query.Append(GrammarCompiler.CompileDropColumnsStatement(table, droppable[DropType.COLUMN]));
                            break;
                        case DropType.FOREIGN_KEY:
                            query.Append(GrammarCompiler.CompileDropForeignKeysStatement(table, droppable[DropType.COLUMN]));
                            break;

                    }
                }
            }
        }

        private static void ProcessAlter(BlueprintBase blueprint, StringBuilder query)
        {
            AddTableNameIntoQuery(blueprint, query, true);
            AppendDefinitionsAndRelations(blueprint, query);
            AppendIndexes(blueprint, query);
        }

        private static void AppendDefinitionsAndRelations(BlueprintBase blueprint, StringBuilder query)
        {
            foreach (KeyValuePair<string, string> definition in blueprint.GetDefinitions())
            {
                query.Append($"{GrammarCompiler.CompileColumnStatement(definition.Key, definition.Value)},");
            }

            foreach (KeyValuePair<string, string> relation in blueprint.GetRelations())
            {
                query.Append($"{GrammarCompiler.CompileForeignKeyStatement(relation.Key, relation.Value)},");
            }

            if (query[^1] == ',')
            {
                query.Remove(query.Length - 1, 1);
                query.Append(");");
            }
        }

        private static void AppendIndexes(BlueprintBase blueprint, StringBuilder query)
        {
            foreach (string index in blueprint.GetIndexes())
            {
                query.Append(GrammarCompiler.CompileIndexStatement(index, blueprint.GetTable() ?? throw new Exception("Table name cannot be missing for indexes")));
            }
        }

        private static void AddTableNameIntoQuery(BlueprintBase blueprint, StringBuilder query, bool alterQuery = false)
        {
            string table = blueprint.GetDefinitions()[0].Key;

            if (alterQuery)
            {
                query.Append(GrammarCompiler.CompileAlterTableStatement(table));
            } else
            {
                query.Append(GrammarCompiler.CompileCreateTableStatement(table));
            }
            blueprint.GetDefinitions().RemoveAt(0);
        }
    }
}
