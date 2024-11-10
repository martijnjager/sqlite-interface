using Newtonsoft.Json.Linq;

namespace Database.Relations.Join
{
    public class HasMany : BaseJoin
    {
        public HasMany(Type MainTable, string primaryKey, string foreignKey) 
            : base(MainTable, primaryKey, foreignKey, RelationType.HasMany)
        {
        }

        public override Eloquent BuildQuery(string key, dynamic[] values)
        {
            if (values.Length < 1)
            {
                throw new Exception("No data provided for the relation.");
            }

            Model m = InstanceContainer.ModelByKey(this.MainTable.Name) as Model;
            return m.WhereIn(key, values);
        }
    }
}
