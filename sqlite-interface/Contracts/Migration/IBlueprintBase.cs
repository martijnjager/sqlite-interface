using Database.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Migration
{
    internal interface IBlueprintBase
    {
        public BlueprintBase GetTable();

        public List<KeyValuePair<string, string>> GetDefinitions();

        public List<KeyValuePair<int, string>> GetRelations();

        public List<string> GetIndexes();

        public List<string> GetDroppable();

        public bool HasDropData();

        public bool HasAlterData();
    }
}
