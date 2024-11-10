using Database.Contracts.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Attribute
{
    /// <summary>
    /// Manages the timestamps for a model.
    /// </summary>
    public class TimestampManager : ITimestampManager
    {
        /// <summary>
        /// Indicates if the model should be timestamped.
        /// </summary>
        protected bool timestamps;

        protected bool softDeletes;

        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        public const string CREATED_AT = "created_at";
        public const string UPDATED_AT = "updated_at";
        public const string DELETED_AT = "deleted_at";

        public const string SOFT_DELETE_COLUMN = "deleted_at";

        private string? deletedAt { get; set; }
        private string? createdAt { get; set; }
        private string? updatedAt;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampManager"/> class.
        /// </summary>
        public TimestampManager()
        {
            this.timestamps = false;
        }

        /// <summary>
        /// Enable timestamps for the model.
        /// </summary>
        public void EnableTimestamps(bool disableSoftDeletes = false)
        {
            this.softDeletes = !disableSoftDeletes;
            this.timestamps = true;
        }

        /// <summary>
        /// Check if timestamps are enabled.
        /// </summary>
        /// <returns>True if timestamps are enabled; otherwise, false.</returns>
        public bool TimestampsEnabled()
        {
            return this.timestamps;
        }

        /// <summary>
        /// Set the deleted_at timestamp.
        /// </summary>
        public void SetDeletedAt()
        {
            if (this.timestamps && this.softDeletes)
            {
                this.deletedAt = DateTime.Now.ToString(DATE_FORMAT);
            }
        }

        /// <summary>
        /// Checks if the model is trashed.
        /// </summary>
        /// <returns>True if the model is trashed; otherwise, false.</returns>
        public bool IsTrashed() => this.deletedAt != null;

        /// <summary>
        /// Handles the timestamps based on the transaction type and model existence.
        /// </summary>
        /// <param name="type">The transaction type.</param>
        /// <param name="modelExists">Indicates if the model exists.</param>
        public void HandleTimestamps(Transactions.Type type, bool modelExists)
        {
            if (this.TimestampsEnabled())
            {
                this.SetTimestamps(type, modelExists);
            }
        }

        /// <summary>
        /// Sets the timestamps based on the transaction type and model existence.
        /// </summary>
        /// <param name="transactionType">The transaction type.</param>
        /// <param name="modelExists">Indicates if the model exists.</param>
        public void SetTimestamps(Transactions.Type transactionType, bool modelExists)
        {
            if (this.TimestampsEnabled() && !modelExists && transactionType == Transactions.Type.TYPE_INSERT)
            {
                this.createdAt = DateTime.Now.ToString(DATE_FORMAT);
            }
            else if (this.TimestampsEnabled() && modelExists && transactionType == Transactions.Type.TYPE_UPDATE)
            {
                this.updatedAt = DateTime.Now.ToString(DATE_FORMAT);
            }

            if (transactionType == Transactions.Type.TYPE_DELETE)
            {
                this.SetDeletedAt();
            }
        }

        /// <summary>
        /// Gets the timestamps as key-value pairs.
        /// </summary>
        /// <returns>The timestamps as key-value pairs.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetTimestamps()
        {
            IEnumerable<KeyValuePair<string, string>> timestamps = new List<KeyValuePair<string, string>>()
                {
                    new(key: CREATED_AT, value: createdAt),
                    new(key: UPDATED_AT, value: updatedAt),
                    new(key: DELETED_AT, value: deletedAt)
                };

            return timestamps;
        }

        /// <summary>
        /// Checks if the model can be soft deleted.
        /// </summary>
        /// <returns>True if model can be soft deleted, false if not</returns>
        public bool CanBeSoftDeleted() => this.softDeletes;
    }
}
