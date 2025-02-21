using Database.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Database
{
    public class Dispatcher
    {
        readonly List<dynamic> pendingJobs;

        public Dispatcher() 
        {
            this.pendingJobs = new List<dynamic>();
        }

        public static void DispatchJob<T>(object? data = null)
        {
            InstanceContainer.Instance.Dispatcher().Dispatch<T>(data);
        }

        public void Dispatch<T>(object? data = null)
        {
            ThreadPool.GetAvailableThreads(out _, out int availableAsyncThreads);

            if (availableAsyncThreads > 0)
            {
                try
                {
                    var job = Activator.CreateInstance<T>();
                    pendingJobs.Add(job);
                    MethodInfo method = typeof(T).GetMethod("Handle");
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        method.Invoke(job, new object[] { data });
                    });
                } catch(NotSupportedException)
                {
                    _ = new DispatchException();
                }
            }
        }
    }
}
