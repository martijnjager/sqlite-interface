using Database.Contracts;
using Database.Extensions;
using Database.Models;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;

namespace Database
{
    public class ModelRegistry : Singleton<ModelRegistry>
    {
        private readonly List<Type> models;

        private ModelRegistry() : base()
        {
            models = new List<Type>();
        }

        public static void LoadModelTypes(Assembly executingAssembly, string inputNamespace = null)
        {
            if (string.IsNullOrEmpty(inputNamespace))
            {
                inputNamespace = "Database.Models";
            }

            Type[] typeList = executingAssembly.GetTypes();
            // Need to fetch all types that are in the Database.Models namespace

            var models = typeList.Where(t => t.FullName.Contains(inputNamespace) && !t.Name.Contains('<')).ToList();

            foreach (Type model in models)
            {
                Instance.models.Add(model);
            }

            Instance.models.Add(typeof(Database.Models.Migration));
        }

        public static IModel? ModelByKey(string key, ReadOnlyCollection<DbColumn>? columns = null)
        {
            key = key.ToString().Singular().ToLower().RemoveSpecialCharacters();

            foreach (Type t in Instance.models)
            {
                if (t.Name.ToLower().Equals(key))
                {
                    return Factory.Create(t, columns ?? null) as Model;
                }
            }

            throw new Exception("Model not found " + key);
        }

        /// <summary>
        /// Checks if the specified type is a registered model
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a registered model; otherwise, false.</returns>
        public static bool HasModelByType(Type type)
        {
            foreach (Type t in Instance.models)
            {
                if (t.Name.ToLower().Equals(type.Name.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
