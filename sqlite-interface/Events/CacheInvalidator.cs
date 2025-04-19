using Database.Cache;
using Database.Contracts;
using Database.Contracts.Event;
using System.Timers;

namespace Database.Events
{
    class CacheInvalidator : IEvent, IDisposable
    {
        private readonly System.Timers.Timer invalidationTimer;

        private readonly TimeSpan invalidationInterval = TimeSpan.FromMinutes(1);

        public CacheInvalidator()
        {
            invalidationTimer = new System.Timers.Timer(invalidationInterval.TotalMilliseconds);
            invalidationTimer.Elapsed += InvalidateCache;
            invalidationTimer.AutoReset = true;
            invalidationTimer.Start();
        }

        public void Dispose()
        {
            invalidationTimer.Dispose();
        }

        public void Created(IModel model)
        {
            Invalidate(model);
        }

        public void Creating(IModel model)
        {
            return;
        }

        public void Deleted(IModel model)
        {
            Invalidate(model);
        }

        public void Deleting(IModel model)
        {
            return;
        }

        public void Restored(IModel model)
        {
            return;
        }

        public void Restoring(IModel model)
        {
            return;
        }

        public void Retrieved(IModel model)
        {
            return;
        }

        public void Saved(IModel model)
        {
            Invalidate(model);
        }

        public void Saving(IModel model)
        {
            return;
        }

        public void Updated(IModel model)
        {
            Invalidate(model);
        }

        public void Updating(IModel model)
        {
            return;
        }

        private void Invalidate(IModel model)
        {
            // Invalidate cache
            string tableName = model.GetTable();
            string id = model.GetValue(model.Attributes.PrimaryKey);
            string cacheKey = CacheFactory.GenerateCacheKey(tableName, id);
            var cache = CacheFactory.GetOrCreate<IModel>("RelationModels");
            cache.Remove(cacheKey);
        }

        private void InvalidateCache(object? state, ElapsedEventArgs e)
        {
            // Logic to invalidate the entire cache periodically
            CacheFactory.ClearExpired();
        }
    }
}
