using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Attribute
{
    /// <summary>
    /// Represents a timestamp manager interface.
    /// </summary>
    public interface ITimestampManager
    {
        /// <summary>
        /// Enables timestamps.
        /// </summary>
        void EnableTimestamps(bool disableSoftDeletes = false);

        /// <summary>
        /// Checks if timestamps are enabled.
        /// </summary>
        /// <returns>True if timestamps are enabled, false otherwise.</returns>
        bool TimestampsEnabled();

        /// <summary>
        /// Sets the deleted timestamp.
        /// </summary>
        void SetDeletedAt();

        /// <summary>
        /// Sets the timestamps for the model.
        /// </summary>
        /// <param name="transactionType">The type of transaction.</param>
        /// <param name="modelExists">Indicates if the model exists.</param>
        void SetTimestamps(Transactions.Type transactionType, bool modelExists);

        /// <summary>
        /// Gets the timestamps.
        /// </summary>
        /// <returns>A collection of key-value pairs representing the timestamps.</returns>
        IEnumerable<KeyValuePair<string, string>> GetTimestamps();

        /// <summary>
        /// Checks if the model is trashed.
        /// </summary>
        /// <returns>True if the model is trashed, false otherwise.</returns>
        bool IsTrashed();

        /// <summary>
        /// Handles the timestamps for the model.
        /// </summary>
        /// <param name="type">The type of transaction.</param>
        /// <param name="modelExists">Indicates if the model exists.</param>
        void HandleTimestamps(Transactions.Type type, bool modelExists);

        /// <summary>
        /// Checks if the model can be soft deleted.
        /// </summary>
        /// <returns>True if model can be soft deleted, else false</returns>
        bool CanBeSoftDeleted();
    }
}
