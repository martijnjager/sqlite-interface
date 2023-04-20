using System.Collections.Generic;
using BaseCollection = Database.Collections.Collection;

namespace Database
{
    public class Collection : BaseCollection
    {
        public Collection(List<Model> items) : base(items)
        {
        }

        public Collection Sort(string column, string direction = "desc")
        {
            this.items.Sort((x, y) => x.GetValue(column).CompareTo(y.GetValue(column)));

            if (direction.Equals("desc"))
            {
                this.items.Reverse();
            }

            return this;
        }

        //public List<T> AllModels<T>()
        //{
        //    List<T> items = new List<T>();

        //    string typeName = Eloquent.Pluralize(typeof(T).Name);

        //    this.rows.ForEach(row =>
        //    {
        //        if (typeName == row.GetTable())
        //        {
        //            items.Add(row);
        //        }
        //        else
        //        {
        //            var relations = row.GetRelations(typeName);

        //            foreach (var relation in relations)
        //            {
        //                items.Add(relation);
        //            }
        //        }
        //    });

        //    return items;
        //}
    }
}