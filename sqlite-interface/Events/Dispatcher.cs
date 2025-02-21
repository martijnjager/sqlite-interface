using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Events
{
    internal class Dispatcher
    {
        private readonly Dictionary<string, List<Action<IModel>>> events;

        public Dispatcher()
        {
            this.events = new Dictionary<string, List<Action<IModel>>>();
        }

        /// <summary>
        /// Adds a listener for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void AddListener(string eventName, Action<IModel> action)
        {
            if (!this.events.ContainsKey(eventName))
            {
                this.events[eventName] = new List<Action<IModel>>();
            }

            this.events[eventName].Add(action);
        }

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventName"></param>
        public void Dispatch(string eventName, IModel model)
        {
            if (this.events.ContainsKey(eventName))
            {
                foreach (var action in this.events[eventName])
                {
                    action(model);
                }
            }
        }

        /// <summary>
        /// Removes a listener for a given event.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        public void RemoveListener(string eventName, Action<IModel> action)
        {
            if (this.events.ContainsKey(eventName))
            {
                this.events[eventName].Remove(action);
            }
        }

        /// <summary>
        /// Removes all listeners for a given event.
        /// </summary>
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
        /// Returns whether or not there is a listener for a given event and action.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool HasListener(string eventName, Action<IModel> action)
        {
            return this.events.ContainsKey(eventName) && this.events[eventName].Contains(action);
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
        public List<Action<IModel>> GetListeners(string eventName)
        {
            return this.events.ContainsKey(eventName) ? this.events[eventName] : new List<Action<IModel>>();
        }

        /// <summary>
        /// Returns all listeners.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<Action<IModel>>> GetAllListeners()
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
