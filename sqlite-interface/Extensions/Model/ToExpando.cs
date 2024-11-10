using Database.Contracts;
using Database.Extensions;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions.Model
{
    public static class ToExpandoCast
    {
        public static ExpandoObject ToExpando(this IModel model)
        {
            var obj = new ExpandoObject() as IDictionary<string, object>;

            foreach (KeyValuePair<string, string> attribute in model.GetAttributes())
            {
                obj.Add(attribute.Key, attribute.Value);
            }

            foreach (KeyValuePair<string, Database.Relations.Relation> relation in model.GetRelations())
            {
                List<ExpandoObject> list = new();

                foreach (Database.Model m in relation.Value.Models.Cast<Database.Model>())
                {
                    list.Add(m.ToExpando());
                }

                switch (relation.Value.Type)
                {
                    case RelationType.HasOne:
                    case RelationType.BelongsTo:
                        obj[relation.Key.ToLower()] = list.FirstOrDefault();
                        break;
                    case RelationType.HasMany:
                    case RelationType.BelongsToMany:
                        obj[relation.Key.Plural().ToLower()] = list;
                        break;
                }
            }

            return (ExpandoObject)obj;
        }

        public static List<ExpandoObject> ToExpandos(this List<Database.Model> items)
        {
            List<ExpandoObject> list = new();
            foreach (Database.Model model in items)
            {
                list.Add(model.ToExpando());
            }

            return list;
        }

        public static List<ExpandoObject> ToExpandos(this List<IModel> items)
        {
            List<ExpandoObject> list = new();
            foreach (Database.Model model in items.Cast<Database.Model>())
            {
                list.Add(model.ToExpando());
            }

            return list;
        }
    }
}
