using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Relations.Join
{
    public class BelongsToMany : BaseJoin
    {
        public BelongsToMany(IModel model, Type IntersectionTable, Type TargetTable, string primaryKey, string foreignKey) 
            : base(model, IntersectionTable, TargetTable, primaryKey, foreignKey, RelationType.BelongsToMany)
        {
        }

        public override Eloquent BuildQuery(string key, dynamic[] value)
        {
            if (value.Length < 1)
            {
                throw new Exception("No data provided for the relation.");
            }

            Model m = ModelRegistry.ModelByKey(this.TargetTable.Name) as Model;
            return m.Join(this.IntersectionTable.Name, this.PrimaryKey, "=", this.ForeignKey)
                .Where(key, value[0]);
        }
    }
}
