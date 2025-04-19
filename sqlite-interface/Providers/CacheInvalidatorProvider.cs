using Database.Contracts;
using Database.Contracts.Event;
using Database.Events;

namespace Database.Providers
{
    class CacheInvalidatorProvider : IProvider
    {
        public void Boot(App instance)
        {
            App.Instance.EventDispatcher().AddDefaultObserving<CacheInvalidator>("CacheInvalidator");
        }
    }
}
