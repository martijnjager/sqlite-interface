using Database;
using Database.Contracts;
using Database.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Database.Collections
{
    public static class Collection
    {
        public static List<string> Pluck(this Dictionary<string,string>[] models, string key)
        {
            List<string> foundData = new ();

            foreach (Dictionary<string, string> model in models)
            {
                if (model.TryGetValue(key, out string? value))
                {
                    foundData.Add(value);
                }
            }

            return foundData;
        }

        //private static void AddModelToJson(dynamic converted, Model model)
        //{
        //    foreach (KeyValuePair<string, string> data in model.GetAttributes())
        //    {
        //        string name = data.Key;
        //        converted[name] = data.Value;
        //    }

        //    ICollection<string> keys = model.GetRelations().Keys;

        //    if (keys.Count > 0)
        //    {
        //        foreach (string key in keys)
        //        {

        //            foreach (List<Model> relation in model.GetRelations()[key])
        //            {
        //                    converted[relationName] = relation[relationName].ToList();
        //                AddModelToJson(relation, model);
        //            }
        //        }

        //    }
        //}
    }
}
