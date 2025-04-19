using Database.Contracts;
using Database.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Database
{
    public class App : Singleton<App>, IApp
    {
        private readonly ConcurrentDictionary<string, object> singletonItems;
        private readonly ConcurrentDictionary<string, Type> bindItems;

        private App()
        {
            this.singletonItems = new ConcurrentDictionary<string, object>();
            this.bindItems = new ConcurrentDictionary<string, Type>();
        }

        //protected void Register<T>()
        //{
        //    object? singleton = Factory.Create<T>();

        //    if (singleton is null)
        //    {
        //        throw new Exception("Object could not be created: " + typeof(T).Name);
        //    }

        //    string name = singleton.GetType().Name;

        //    this.singletonItems.TryAdd(name, singleton);
        //}

        public static void RegisterByNamespace(Assembly assembly, string namespaceName)
        {
            Type[] typeList = assembly.GetTypes();
            List<Type> types = typeList.Where(t => t.FullName.Contains(namespaceName) && !t.Name.Contains('<')).ToList();

            foreach (Type type in types)
            {
                Instance.bindItems.TryAdd(type.Name, type);
            }
        }

        public static List<object> RegisterByNamespace(string namespaceName, IEnumerable<string> includeOnly = null, IEnumerable<string> exclude = null)
        {
            List<object> instances = new();
            var classesInNamespace = Instance.bindItems.Where(x => x.Value.Namespace == namespaceName).ToList();

            if (includeOnly is not null)
            {
                classesInNamespace = classesInNamespace.Where(x => includeOnly.Contains(x.Key)).ToList();
            }

            if (exclude is not null)
            {
                classesInNamespace = classesInNamespace.Where(x => !exclude.Contains(x.Key)).ToList();
            }

            if (classesInNamespace.Count < 1)
            {
                return instances;
            }

            foreach (var item in classesInNamespace)
            {
                instances.Add(Get<object>(item.Key));
            }

            return instances;
        }

        public static void RegisterSingleton<T>(string key = null)
        {
            if (key is null)
            {
                key = typeof(T).Name;
            }

            Instance.singletonItems.TryAdd(key, Factory.Create<T>());
        }

        public static void Bind<T>(string key)
        {
            Instance.bindItems.TryAdd(key, typeof(T));
        }

        public static T Get<T>(string key)
        {
            if (Instance.singletonItems.TryGetValue(key, out object instance))
            {
                return (T)instance;
            }
            if (Instance.bindItems.TryGetValue(key, out Type type))
            {
                return (T)Factory.Create(type);
            }
            throw new Exception($"No object registered with key '{key}'. Use RegisterSingleton or Bind to add it.");
        }

        public static T Get<T>()
        {
            Type type = typeof(T);
            var singleton = Instance.singletonItems.FirstOrDefault(x => x.Value.GetType().FullName.Equals(type.FullName));
            if (singleton.Value != null)
            {
                return (T)singleton.Value;
            }
            var bind = Instance.bindItems.FirstOrDefault(x => x.Value.FullName.Equals(type.FullName));
            if (bind.Value != null)
            {
                return (T)Factory.Create(bind.Value);
            }
            throw new Exception($"No object registered for {type.FullName}. Use RegisterSingleton or Bind.");
        }

        public static T Resolve<T>()
        {
            return Factory.Create<T>();
        }

        public static T Resolve<T>(params object[] data)
        {
            return Factory.Create<T>(data);
        }

        public static object Create(Type o)
        {
            return Factory.Create(o);
        }

        public static void SetSingleton<T>(T instance, string key, bool overwrite = false)
        {
            if (overwrite)
            {
                Instance.singletonItems[key] = instance; // Thread-safe overwrite
            }
            else
            {
                Instance.singletonItems.TryAdd(key, instance); // Thread-safe add, ignores if exists
            }
        }

        // IApp interface methods
        T IApp.GetInstance<T>(string key) => Get<T>(key);
        T IApp.GetInstance<T>() => Get<T>();
        void IApp.SetSingleton(object instance, string key) => SetSingleton(instance, key);
    }
}