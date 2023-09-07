using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDatabaseInterface.Contracts
{
    public interface IProvider
    {
        void Boot(InstanceContainer instance);
    }
}
