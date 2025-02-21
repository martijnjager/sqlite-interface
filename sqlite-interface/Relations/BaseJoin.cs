using Database.Console;
using Database.Contracts.Relation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.Relations
{
    public abstract class BaseJoin : IJoin
    {
        public Type MainTable { get; private set; }
        public Type TargetTable { get; private set; }
        public Type IntersectionTable { get; private set; } // belongsToMany intersection table
        public string PrimaryKey { get; private set; }
        public string ForeignKey { get; private set; }

        public RelationType Type { get; private set; }

        public BaseJoin(Type MainTable, string primaryKey, string foreignKey, RelationType type)
        {
            this.MainTable = MainTable;
            this.PrimaryKey = primaryKey;
            this.ForeignKey = foreignKey;
            this.Type = type;
        }

        public BaseJoin(Type IntersectionTable, Type TargetTable, string primaryKey, string foreignKey, RelationType type)
        {
            this.IntersectionTable = IntersectionTable;
            this.TargetTable = TargetTable;
            this.PrimaryKey = primaryKey;
            this.ForeignKey = foreignKey;
            this.Type = type;
        }

        public abstract Eloquent BuildQuery(string key, dynamic[] values);
    }
}
