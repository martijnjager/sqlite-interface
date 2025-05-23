using Database.Contracts;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Relations.Join
{
    public class BelongsTo : BaseJoin
    {
        public BelongsTo(IModel model, Type TargetTable, string primaryKey, string foreignKey) 
            : base(model, TargetTable, primaryKey, foreignKey, RelationType.BelongsTo)
        {
        }

        public override Eloquent BuildQuery(string key, dynamic[] value)
        {
            if (value.Length < 1)
            {
                throw new Exception("No data provided for the relation.");
            }

            Model m = ModelRegistry.ModelByKey(this.TargetTable.Name) as Model;

            return m.WhereIn(key, value);
        }
    }
}
