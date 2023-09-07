using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public abstract class Job
    {
        public abstract void Handle(object data = null);
    }
}
