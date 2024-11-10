using Database.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Exceptions
{
    abstract class BaseException
    {
        public virtual string Message { get; }

        public BaseException()
        { 
            BaseCommand.WriteLine(this.Message);
        }
    }
}
