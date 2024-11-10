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
        public BelongsTo(Type MainTable, string primaryKey, string foreignKey) 
            : base(MainTable, primaryKey, foreignKey, RelationType.BelongsTo)
        {
        }

        public override Eloquent BuildQuery(string key, dynamic[] value)
        {
            if (value.Length < 1)
            {
                throw new Exception("No data provided for the relation.");
            }

            Model m = InstanceContainer.ModelByKey(this.MainTable.Name) as Model;
            return m.Where(key, value[0]);
        }
    }
}
