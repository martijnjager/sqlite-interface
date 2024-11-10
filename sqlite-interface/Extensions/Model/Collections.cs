using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions.Model
{
    public static class Collections
    {
        public static List<IModel> Search(this List<IModel> models, string key, string value)
        {
            List<IModel> results = new();

            foreach (IModel model in models)
            {
                if (model.GetKeys().Contains(key) && model.GetValue(key) == value)
                {
                    results.Add(model);
                }
            }

            return results;
        }
    }
}
