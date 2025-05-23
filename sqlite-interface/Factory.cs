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
            Type type = typeof(T);
            ConstructorInfo constructor = GetConstructor(type);

            if (constructor != null)
            {
                return (T)constructor.Invoke(Array.Empty<object>());
            }

            return Activator.CreateInstance<T>();
        }

        public static T Create<T>(params object[] data)
        {
            Type type = typeof(T);
            ConstructorInfo constructor = GetConstructor(type, data);

            if (constructor != null)
            {
                return (T)constructor.Invoke(data);
            }

            return (T)Activator.CreateInstance(typeof(T), data);
        }

        public static dynamic Create(Type type)
        {
            ConstructorInfo constructor = GetConstructor(type);

            if (constructor != null)
            {
                return constructor.Invoke(Array.Empty<object>());
            }

            return Activator.CreateInstance(type);
        }

        public static dynamic Create(Type type, params object[] data)
        {
            ConstructorInfo constructor = GetConstructor(type, data);

            if (constructor != null)
            {
                return constructor.Invoke(data);
            }

            return Activator.CreateInstance(type, data);
        }

        public static dynamic InvokeMethod(object Object, string method)
        {
            Type x = Object.GetType();
            MethodInfo methodInfo = x.GetMethod(method);

            return methodInfo.Invoke(Object, null);
        }

        /// <summary>
        /// Creates an instance of the specified type using the constructor that matches the provided parameters.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static ConstructorInfo GetConstructor(Type type, params object[] parameters)
        {
            Type currentType = type;

            while (currentType != null)
            {
                // Get all constructors from the current type
                var constructors = currentType.GetConstructors(BindingFlags.Public |
                                                            BindingFlags.NonPublic |
                                                            BindingFlags.Instance);

                // Find a constructor that matches the parameter types
                foreach (var constructor in constructors)
                {
                    var constructorParams = constructor.GetParameters();

                    // If no parameters were provided, look for parameterless constructor
                    if (parameters == null || parameters.Length == 0)
                    {
                        if (constructorParams.Length == 0)
                        {
                            return constructor;
                        }
                        continue;
                    }

                    // Check if parameter count matches
                    if (constructorParams.Length != parameters.Length)
                    {
                        continue;
                    }

                    // Check if parameter types are compatible
                    bool isMatch = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i] != null &&
                            !constructorParams[i].ParameterType.IsAssignableFrom(parameters[i].GetType()))
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        return constructor;
                    }
                }

                // Move up to the parent class
                currentType = currentType.BaseType;
            }

            return null;
        }
    }
}