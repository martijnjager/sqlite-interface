using Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Event
{
    public interface IEventHandler
    {
        /// <summary>
        /// Adds an event.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        IEventHandler On<TModel, TObserver>() where TObserver : IEvent where TModel : IUseEvents;

        /// <summary>
        /// Triggers the event.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="model"></param>
        void Dispatch(string eventName, IModel model);
    }
}
