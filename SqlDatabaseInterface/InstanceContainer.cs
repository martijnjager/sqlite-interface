using System;
using System.Collections.Generic;

namespace Database
{
    public class InstanceContainer
    {
        private static readonly object @lock = new object();
        private static InstanceContainer instance = null;
        private static InstanceContainer Instance
        {
            get
            {
                lock (@lock)
                {
                    if (instance == null)
                    {
                        instance = new InstanceContainer();
                    }
                    return instance;
                }
            }
        }

        private HashSet<Tuple<string, object>> singletonItems;

        private HashSet<Tuple<string, Type>> bindItems;

        private InstanceContainer()
        {
            this.singletonItems = new HashSet<Tuple<string, object>>();
            this.bindItems = new HashSet<Tuple<string, Type>>();
        }

        protected void Register<T>(string key)
        {
            this.singletonItems.Add(new Tuple<string, object>(key, Factory.Create<T>()));
        }

        protected void BindType<T>(string key)
        {
            this.bindItems.Add(new Tuple<string, Type>(key, typeof(T)));
        }

        public static void RegisterSingleton<T>(string key)
        { 
            Instance.Register<T>(key);
        }

        public static void Bind<T>(string key)
        {
            Instance.BindType<T>(key);
        }

        public dynamic GetInstance(string key)
        {
            foreach (Tuple<string, object> instance in this.singletonItems)
            {
                if (instance.Item1.Equals(key))
                {
                    return instance.Item2;
                }
            }

            foreach (Tuple<string, Type> item in this.bindItems)
            {
                if (item.Item1.Equals(key))
                {
                    return Factory.Create(item.Item2);
                }
            }

            throw new Exception("Object has not been registered: " + key);
        }

        public static dynamic Invoke(object Object, string method)
        {
            return Factory.InvokeMethod(Object, method);
        }

        /// <summary>
        /// Creates or returns a new object that has been registered
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static dynamic Get(string key)
        {
            return Instance.GetInstance(key);
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
    }
}
