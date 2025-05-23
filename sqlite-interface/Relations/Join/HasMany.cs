using Database.Contracts;
using Newtonsoft.Json.Linq;

namespace Database.Relations.Join
{
    public class HasMany : BaseJoin
    {
        public HasMany(IModel model, Type TargetTable, string primaryKey, string foreignKey) 
            : base(model, TargetTable, primaryKey, foreignKey, RelationType.HasMany)
        {
        }

        public override Eloquent BuildQuery(string key, dynamic[] values)
        {
            if (values.Length < 1)
            {
                throw new Exception("No data provided for the relation.");
            }

            Model m = ModelRegistry.ModelByKey(this.TargetTable.Name) as Model;
            return m.WhereIn(key, values);
        }
    }
}
