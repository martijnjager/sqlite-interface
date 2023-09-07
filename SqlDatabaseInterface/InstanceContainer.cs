using System;
using System.Collections.Generic;

namespace Database
{
    public class InstanceContainer
    {
        private static readonly object @lock = new object();
        private static InstanceContainer instance = null;
        public static InstanceContainer Instance
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

        protected void Register<T>()
        {
            object singleton = Factory.Create<T>();
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
            return Factory.Create<T>(data);
        }
    }
}
