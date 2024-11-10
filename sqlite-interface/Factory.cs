using Database.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    internal class Factory
    {
        public static T Create<T>()
        {
            return Activator.CreateInstance<T>();
        }

        public static T Create<T>(params object[] data)
        {
            return (T)Activator.CreateInstance(typeof(T), data);
        }

        public static dynamic Create(Type type)
        {
            return Activator.CreateInstance(type);
        }
        public static dynamic Create(Type type, params object[] data)
        {
            return Activator.CreateInstance(type, data);
        }

        public static dynamic InvokeMethod(object Object, string method)
        {
            Type x = Object.GetType();
            MethodInfo methodInfo = x.GetMethod(method);

            return methodInfo.Invoke(Object, null);
        }
    }
}