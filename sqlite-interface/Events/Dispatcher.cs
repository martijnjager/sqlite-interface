using Database.Console;
using Database.Contracts;
using Database.Contracts.Cache;
using Database.Contracts.Event;
using Database.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Events
{
    public class Dispatcher : IEventHandler
    {
        private readonly Dictionary<string, List<Type>> events;

        private readonly Dictionary<string, List<Type>> defaultDispatchableEvents;

        public Dispatcher()
        {
            this.events = [];
            this.defaultDispatchableEvents = [];
        }

        internal void AddDefaultDispatchEvent<TObserver>(string eventName)
        {
            if (!this.defaultDispatchableEvents.ContainsKey(eventName))
            {
                this.defaultDispatchableEvents[eventName] = [];
            }

            var observerType = typeof(TObserver);

            if (!this.defaultDispatchableEvents[eventName].Contains(observerType))
            {
                this.defaultDispatchableEvents[eventName].Add(observerType);
            }
        }

        public IEventHandler On<TModel, TObserver>() where TObserver : IEvent where TModel : IUseEvents
        {
            var eventMethods = typeof(IEvent).GetMethods();
            var eventDispatcher = App.Instance.EventDispatcher();

            foreach (var method in eventMethods)
            {
                var modelName = typeof(TModel).Name;
                string eventName = eventDispatcher.GenerateEventName(modelName, method.Name);
                var observerType = typeof(TObserver);

                AddListener<TObserver>(eventName);
            }

            return this;
        }

        internal void AddDefaultObserving<T>(string name) where T : IEvent
        {
            var eventMethods = typeof(IEvent).GetMethods();
            var eventDispatcher = App.Instance.EventDispatcher();

            foreach (var method in eventMethods)
            {
                string eventName = eventDispatcher.GenerateEventName(name, method.Name);
                //var observer = App.Resolve<T>();
                //Action<IModel> action = (Action<IModel>)Delegate.CreateDelegate(typeof(Action<IModel>), observer, method);
                eventDispatcher.AddDefaultDispatchEvent<T>(eventName);
            }
        }

        /// <summary>
        /// Adds a listener for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        internal void AddListener<TObserver>(string eventName)
        {
            if (!this.events.ContainsKey(eventName))
            {
                this.events[eventName] = [];
            }

            var observerType = typeof(TObserver);

            if (!this.events[eventName].Contains(observerType))
            {
                this.events[eventName].Add(observerType);
            }
        }

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventName"></param>
        public void Dispatch(string eventName, IModel model)
        {
            BaseCommand.WriteLine(eventName);

            if (this.events.ContainsKey(eventName))
            {
                foreach (var observerType in this.events[eventName])
                {
                    InvokeObserver(observerType, eventName.Split('_')[1], model);
                }
            }

            foreach (var observers in this.defaultDispatchableEvents)
            {
                observers.Value.ForEach(observerType =>
                {
                    InvokeObserver(observerType, eventName.Split('_')[1], model);
                });
            }
        }

        private void InvokeObserver(Type observerType, string methodName, IModel model)
        {
            methodName = char.ToUpper(methodName[0]) + methodName.Substring(1);

            var observerInstance = App.Create(observerType);
            var method = observerType.GetMethod(methodName);
            _ = method?.Invoke(observerInstance, new object[] { model });
        }

        /// <summary>
        /// Removes a listener for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        public void RemoveListener<TObserver>(string eventName)
        {
            if (this.events.ContainsKey(eventName))
            {
                var observerType = typeof(TObserver);
                this.events[eventName].RemoveAll(listener => listener == observerType);
            }
        }

        /// <summary>
        /// Removes all listeners for a given event.
        /// </summary>z
        /// <param name="eventName"></param>
        public void RemoveAllListeners(string eventName)
        {
            if (this.events.ContainsKey(eventName))
            {
                this.events.Remove(eventName);
            }
        }

        /// <summary>
        /// Removes all listeners.
        /// </summary>
        public void RemoveAllListeners()
        {
            this.events.Clear();
        }

        /// <summary>
        /// Returns whether or not there is a listener for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public bool HasListener(string eventName)
        {
            return this.events.ContainsKey(eventName);
        }

        /// <summary>
        /// Returns whether or not there are listeners.
        /// </summary>
        /// <returns></returns>
        public bool HasListeners()
        {
            return this.events.Count > 0;
        }

        /// <summary>
        /// Returns whether or not there are listeners for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public bool HasListeners(string eventName)
        {
            return this.events.ContainsKey(eventName) && this.events[eventName].Count > 0;
        }

        /// <summary>
        /// Returns the number of listeners for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public int ListenerCount(string eventName)
        {
            return this.events.ContainsKey(eventName) ? this.events[eventName].Count : 0;
        }

        /// <summary>
        /// Returns the number of listeners.
        /// </summary>
        /// <returns></returns>
        public int ListenerCount()
        {
            return this.events.Count;
        }

        /// <summary>
        /// Returns all event names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetEventNames()
        {
            return this.events.Keys.ToList();
        }

        /// <summary>
        /// Returns all listeners for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public List<Type> GetListeners(string eventName)
        {
            return this.events.ContainsKey(eventName) ? this.events[eventName] : new List<Type>();
        }

        /// <summary>
        /// Returns all listeners.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<Type>> GetAllListeners()
        {
            return this.events;
        }

        /// <summary>
        /// Returns whether or not there are listeners for a given model.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool HasListenersByModel(string tableName)
        {
            return this.events.Keys.Any(x => x.Contains(tableName));
        }

        public string GenerateEventName(string tableName, string action)
        {
            tableName = tableName.ToLower();
            action = action.ToLower();
            return $"{tableName}_{action}";
        }
    }
}
