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

        //public static BaseJoin JoinFrom<T>(string primaryKey, string foreignKey, RelationType type)
        //{
        //    return CreateJoin<T>(primaryKey, foreignKey, type);
        //}

        //public static BaseJoin JoinFromMany<M, R, T>(string mainTableForeignKey, string targetTableForeignKey, RelationType type)
        //{
        //    return CreateJoin<M, R, T>(mainTableForeignKey, targetTableForeignKey, type);
        //}

        //public static BaseJoin JoinTo<T>(string foreignKey, string primaryKey, RelationType type)
        //{
        //    return CreateJoin<T>(primaryKey, foreignKey, type);
        //}

        //private static BaseJoin CreateJoin<T>(string primaryKey, string foreignKey, RelationType type)
        //{
        //    if (!InstanceContainer.HasModelByType(typeof(T)))
        //    {
        //        throw new Exception("Model not found.");
        //    }

        //    return new(typeof(T), primaryKey, foreignKey, type);
        //}

        //private static BaseJoin CreateJoin<M, R, T>(string mainTableForeignKey, string targetTableForeignKey, RelationType type)
        //{
        //    if (!InstanceContainer.HasModelByType(typeof(M)) || 
        //        !InstanceContainer.HasModelByType(typeof(R)) ||
        //        !InstanceContainer.HasModelByType(typeof(T)))
        //    {
        //        throw new Exception("Model not found.");
        //    }

        //    return new(typeof(R), typeof(T), targetTableForeignKey, mainTableForeignKey, type);
        //}

        //public Eloquent Run(string key, string value)
        //{
        //    Model m = InstanceContainer.ModelByKey(this.MainTable.Name) as Model;
        //    return m.Where(key, value);
        //}

        //public Eloquent Run(string key, params object[] values)
        //{
        //    //if (this.Type == RelationType.HasMany)
        //    //{
        //    //    m = InstanceContainer.ModelByKey(this.MainTable.Name) as Model as Model;
        //    //    Model t = InstanceContainer.ModelByKey(this.TargetTable.Name) as Model as Model;
                
        //    //    return t.Join(m.GetTable(), m.Attributes.PrimaryKey, "=", t.GetTable() + "." + this.ForeignKey)
        //    //            .WhereIn(m.Attributes.PrimaryKey, values);
        //    //}

        //    Model m = InstanceContainer.ModelByKey(this.MainTable.Name) as Model;
        //    return m.WhereIn(key, values);
        //}

        /// <summary>
        /// Belongs to many relation.
        /// </summary>
        /// <param name="mainTablePrimaryKeyValue"></param>
        /// <returns></returns>
        //public Eloquent Run(string mainTablePrimaryKeyValue)
        //{
        //    Model m = InstanceContainer.ModelByKey(this.MainTable.Name) as Model as Model;
        //    Model i = InstanceContainer.ModelByKey(this.IntersectionTable.Name) as Model as Model;
        //    Model t = InstanceContainer.ModelByKey(this.TargetTable.Name) as Model as Model;

        //    return t
        //        .Join(i.GetTable(), m.Attributes.PrimaryKey, "=", i.GetTable() + "." + this.PrimaryKey)
        //        .Where(i.GetTable() + "." + this.ForeignKey, mainTablePrimaryKeyValue);
        //}
    }
}
