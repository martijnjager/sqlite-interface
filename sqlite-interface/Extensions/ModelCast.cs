using Database.Console;
using Database.Contracts;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions
{
    public static class ModelCast
    {
        public static object ToType(this Database.Model originModel, Type conversionType)
        {
            var model = InstanceContainer.Create(conversionType);

            string[] fields = new[] {
                "Attributes",
                "RawAttributes",
                //"DefaultAttributes",
                "timestamps",
                "PrimaryKey",
            };

            foreach (string field in fields)
            {
                try
                {
                    FieldInfo f = model.GetType().BaseType.GetField(field, BindingFlags.NonPublic);
                    f.SetValue(model, originModel.GetType().BaseType.GetField(field, BindingFlags.NonPublic));
                }
                catch (Exception ex)
                {
                    // Handle the exception here
                    // continue
                    BaseCommand.WriteLine(field + " not found to cast");
                    BaseCommand.WriteLine(ex.Message);
                }
            }

            return model;
        }

        public static object ToJson(this Database.Model model)
        {
            return model.GetAttributes();
        }

        public static List<object> ToJson(this List<Database.Model> items)
        {
            List<object> list = new();
            foreach (Database.Model item in items)
            {
                list.Add(item.GetAttributes());
            }

            return list;
        }

        public static Dictionary<string, string>[] ToDictionaryArray(this List<IModel> items)
        {
            List<Dictionary<string, string>> data = new();

            foreach (Database.Model model in items.Cast<Database.Model>())
            {
                Dictionary<string, string> key = new();
                foreach (var x in model.GetKeys())
                {
                    key.Add(x, model.GetValue(x));
                }
                data.Add(key);
            }

            return data.ToArray();
        }
    }
}
