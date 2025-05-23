using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Exceptions
{
    class DispatchException : BaseException
    {
        public override string Message { get; }

        public DispatchException() : base()
        {
            Message = "An error occurred with dispatching a job";
        }

        public DispatchException(string customMessage) : base()
        {
            Message = customMessage;
        }
    }
}
