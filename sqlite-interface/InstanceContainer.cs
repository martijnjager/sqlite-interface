using Database.Contracts;
using Database.Extensions;
using Database.Models;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;

namespace Database
{
    public class InstanceContainer : IInstanceContainer
    {
        private static readonly object @lock = new();
        private static InstanceContainer? instance = null;
        public static InstanceContainer Instance
        {
            get
            {
                lock (@lock)
                {
                    instance ??= new InstanceContainer();
                    return instance;
                }
            }
        }

        private readonly HashSet<Tuple<string, object>> singletonItems;

        private readonly HashSet<Tuple<string, Type>> bindItems;

        private readonly List<Type> models;

        private InstanceContainer()
        {
            this.singletonItems = new HashSet<Tuple<string, object>>();
            this.bindItems = new HashSet<Tuple<string, Type>>();
            this.models = new();
        }
        public static void LoadModelTypes(Assembly executingAssembly)
        {
            Type[] typeList = executingAssembly.GetTypes();
            // Need to fetch all types that are in the Database.Models namespace

            var models = typeList.Where(t => t.FullName.Contains("Database.Models") && !t.Name.Contains('<')).ToList();

            foreach (Type model in models)
            {
                Instance.models.Add(model);
            }
            Instance.models.Add(typeof(Database.Models.Migration));
        }

        public static void LoadMigrationTypes(Assembly executingAssembly)
        {
            Type[] typeList = executingAssembly.GetTypes();
            // Need to fetch all types that are in the Database.Migrations namespace

            List<Type> migrations = typeList.Where(t => t.FullName.Contains("Backend.Database.Migrations.Tables") && !t.Name.Contains('<')).ToList();

            foreach (Type migration in migrations)
            {
                Migration.RegisterMigration(migration);
            }
        }

        /// <summary>
        /// Registers all types in the specified namespace as binded types
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <param name="namespaceName"></param>
        public static void RegisterByNamespace(Assembly executingAssembly, string namespaceName)
        {
            Type[] typeList = executingAssembly.GetTypes();
            // Need to fetch all types that are in the Database.Migrations namespace
            List<Type> migrations = typeList.Where(t => t.FullName.Contains(namespaceName) && !t.Name.Contains('<')).ToList();
            foreach (Type migration in migrations)
            {
                Instance.bindItems.Add(new Tuple<string, Type>(migration.Name, migration));
            }
        }

        public static List<object> LoadByNamespace(string classNamespace, List<string> only = null, List<string> except = null)
        {
            List<object> instances = new List<object>();

            List<Tuple<string, Type>> classesInNamespace = Instance.bindItems.Where(x => x.Item2.Namespace == classNamespace).ToList();

            if (only is not null)
            {
                classesInNamespace = classesInNamespace.Where(x => only.Contains(x.Item1)).ToList();
            }

            if (except is not null)
            {
                classesInNamespace = classesInNamespace.Where(x => !except.Contains(x.Item1)).ToList();
            }

            if (classesInNamespace.Count < 1)
            {
                return instances;
            }

            foreach (Tuple<string, Type> item in classesInNamespace)
            {
                var obj = Instance.GetInstance<object>(item.Item1);
                instances.Add(obj);
            }

            return instances;
        }

        /// <summary>
        /// Register a singleton object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        protected void Register<T>()
        {
            object? singleton = Factory.Create<T>();

            if (singleton is null)
            {
                throw new Exception("Object could not be created: " + typeof(T).Name);
            }

            string name = singleton.GetType().Name;
            this.singletonItems.Add(new Tuple<string, object>(name, singleton));
        }

        protected void BindType<T>(string key)
        {
            this.bindItems.Add(new Tuple<string, Type>(key, typeof(T)));
        }

        public static void RegisterSingleton<T>()
        { 
            Instance.Register<T>();
        }

        public static void Bind<T>(string key)
        {
            Instance.BindType<T>(key);
        }

        public T GetInstance<T>(string key)
        {
            foreach (Tuple<string, object> instance in this.singletonItems)
            {
                if (instance.Item1.Equals(key))
                {
                    return (T)instance.Item2;
                }
            }

            foreach (Tuple<string, Type> item in this.bindItems)
            {
                if (item.Item1.Equals(key))
                {
                    return (T)Factory.Create(item.Item2);
                }
            }

            throw new Exception("Object has not been registered: " + key);
        }

        public T GetInstance<T>()
        {
            Type type = typeof(T);
            foreach (Tuple<string, object> instance in this.singletonItems)
            {
                if (instance.Item2.GetType().FullName.Equals(type.FullName))
                {
                    return (T)instance.Item2;
                }
            }

            foreach (Tuple<string, Type> item in this.bindItems)
            {
                if (item.Item2.FullName.Equals(type.FullName))
                {
                    return (T)Factory.Create(item.Item2);
                }
            }

            throw new Exception("Object has not been registered: " + type.FullName);
        }

        public static object Create(Type o)
        {
            return Factory.Create(o);
        }

        /// <summary>
        /// Invoke a method of an object
        /// </summary>
        /// <param name="Object"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static dynamic Invoke(object Object, string method)
        {
            return Factory.InvokeMethod(Object, method);
        }

        /// <summary>
        /// Creates or returns a new object that has been registered
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return Instance.GetInstance<T>(key);
        }

        public static T Get<T>()
        {
            return Instance.GetInstance<T>();
        }

        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return Factory.Create<T>();
        }

        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>(params object[] data)
        {
            //if (data.Length == 0)
            //{
            //    return Resolve<T>();
            //}

            //foreach (Model t in Instance.models)
            //{
            //    if (t.GetType().Name.ToLower().Equals(data[0].ToString().Singular().ToLower()))
            //    {
            //        return t as T;
            //    }
            //}

            return Factory.Create<T>(data);
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

            throw new Exception("Model not found." + key);
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

        public void SetSingleton(object instance, string key)
        {
            if (this.singletonItems.Any(x => x.Item1 == key))
            {
                return;
            }
            this.singletonItems.Add(new Tuple<string, object>(key, instance));
        }
    }
}
