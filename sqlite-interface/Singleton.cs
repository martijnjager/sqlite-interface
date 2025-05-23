using System.Reflection;
using System.Runtime.InteropServices.Marshalling;

namespace Database
{
    public class Singleton<T> where T: class
    {
        private static readonly Lazy<T> lazyInstance = new Lazy<T>(() => CreateInstance(), isThreadSafe: true);

        public static T Instance => lazyInstance.Value;
        
        protected Singleton()
        {
        }

        private static T CreateInstance()
        {
            var constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

            if (constructor is not null)
            {
                return (T)constructor.Invoke(null);
            }

            return (T)Activator.CreateInstance(typeof(T));
        }
    }
}
