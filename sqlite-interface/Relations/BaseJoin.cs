using Database.Console;
using Database.Contracts;
using Database.Contracts.Relation;
using Database.QueryBuilder;
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

        protected IModel mainModel { get; private set; }

        public BaseJoin(IModel mainModel, Type TargetTable, string primaryKey, string foreignKey, RelationType type)
        {
            this.TargetTable = TargetTable;
            this.PrimaryKey = primaryKey;
            this.ForeignKey = foreignKey;
            this.Type = type;
            this.mainModel = mainModel;
        }

        public BaseJoin(IModel mainModel, Type IntersectionTable, Type TargetTable, string primaryKey, string foreignKey, RelationType type)
        {
            this.IntersectionTable = IntersectionTable;
            this.TargetTable = TargetTable;
            this.PrimaryKey = primaryKey;
            this.ForeignKey = foreignKey;
            this.Type = type;
            this.mainModel = mainModel;
        }

        public abstract Eloquent BuildQuery(string key, dynamic[] values);

        public Eloquent Query()
        {
            dynamic[] ids = null;
            string primarykey = null;
            string foreignkey = null;

            switch (this.Type)
            {
                case RelationType.BelongsTo:
                case RelationType.BelongsToMany:
                    PrimaryKey = ForeignKey;
                    ForeignKey = PrimaryKey;
                    break;
                case RelationType.HasMany:
                case RelationType.HasOne:
                    primarykey = PrimaryKey;
                    foreignkey = ForeignKey;
                    break;
            }

            Model m = ModelRegistry.ModelByKey(this.TargetTable.Name) as Model;
            string foreignKey = m.GetTable() + "." + this.ForeignKey;
            string mainModelKey = this.mainModel.GetTable() + "." + this.PrimaryKey;

            return m.Where(foreignKey, new RawSql(mainModelKey));
        }
    }
}
