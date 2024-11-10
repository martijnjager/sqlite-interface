using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions.Model
{
    public static class Async
    {
        public static void HandleWait(this Task<List<IModel>> result)
        {
            while (result.Status != TaskStatus.RanToCompletion && 
                result.Status != TaskStatus.Faulted)
            {

            }

            if (result.Status == TaskStatus.Faulted)
            {
                throw result.Exception;
            }
        }
        public static void HandleWait(this Task<IModel> result)
        {
            if (result.Status != TaskStatus.Faulted && result.Status != TaskStatus.Running)
            {
                result.Start();
            }

            while (result.Status != TaskStatus.RanToCompletion &&
                result.Status != TaskStatus.Faulted)
            {

            }

            if (result.Status == TaskStatus.Faulted)
            {
                throw result.Exception;
            }
        }
    }
}
