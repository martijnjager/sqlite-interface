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
                    method.Invoke(job, new object[] { data });
                    //ThreadPool.QueueUserWorkItem(job.Handle, data);
                } catch(NotSupportedException)
                {
                    _ = new DispatchException();
                }
            }
        }
    }
}
