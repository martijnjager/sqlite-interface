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
    public class JobDispatcher
    {
        readonly List<dynamic> pendingJobs;

        public JobDispatcher() 
        {
            this.pendingJobs = new List<dynamic>();
        }

        public static void DispatchJob<T>(object? data = null)
        {
            App.Instance.Dispatcher().Dispatch<T>(data);
        }

        public void Dispatch<T>(object? data = null)
        {
            ThreadPool.GetAvailableThreads(out _, out int availableAsyncThreads);

            if (availableAsyncThreads > 0)
            {
                try
                {
                    var job = Activator.CreateInstance<T>();
                    var method = typeof(T).GetMethod("Handle");

                    if (method == null) {
                        throw new Exception("Job must implement a Handle method");
                    }

                    pendingJobs.Add(job);
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            method.Invoke(job, new object[] { data });
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            pendingJobs.Remove(job);
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to dispatch job: {ex.Message}", ex);
                }
            }
            else
            {
                throw new Exception("No thread pool threads available for job dispatch");
            }
        }

    }
}
