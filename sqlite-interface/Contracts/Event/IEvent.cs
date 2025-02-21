using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Event
{
    public interface IEvent
    {
        /// <summary>
        /// Called when the model is saved.
        /// </summary>
        void Saved(IModel model);

        /// <summary>
        /// Called when the model is deleted.
        /// </summary>
        void Deleted(IModel model);

        /// <summary>
        /// Called when the model is updated.
        /// </summary>
        void Updated(IModel model);

        /// <summary>
        /// Called when the model is created.
        /// </summary>
        void Created(IModel model);

        /// <summary>
        /// Called when the model is restored.
        /// </summary>
        void Restored(IModel model);

        /// <summary>
        /// Called when the model is retrieved.
        /// </summary>
        void Retrieved(IModel model);

        /// <summary>
        /// Called when the model is saving.
        /// </summary>
        void Saving(IModel model);

        /// <summary>
        /// Called when the model is deleting.
        /// </summary>
        void Deleting(IModel model);

        /// <summary>
        /// Called when the model is updating.
        /// </summary>
        void Updating(IModel model);

        /// <summary>
        /// Called when the model is creating.
        /// </summary>
        void Creating(IModel model);

        /// <summary>
        /// Called when the model is restoring.
        /// </summary>
        void Restoring(IModel model);
    }
}
