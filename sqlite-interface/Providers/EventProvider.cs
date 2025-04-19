using Database.Contracts;
using Database.Contracts.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Providers
{
    class EventProvider : IProvider
    {
        public void Boot(App instance)
        {
            //instance.EventDispatcher().
        }
    }
}
