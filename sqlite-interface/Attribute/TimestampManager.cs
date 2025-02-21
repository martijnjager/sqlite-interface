using Database.Contracts.Attribute;

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

        private string? DeletedAt { get; set; } = null;
        private string? CreatedAt { get; set; }
        private string? UpdatedAt;

        private List<string> changed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampManager"/> class.
        /// </summary>
        public TimestampManager()
        {
            this.timestamps = false;
            this.changed = new List<string>();
        }

        public static string[] TimestampColumns()
        {
            return new string[] { CREATED_AT, UPDATED_AT, DELETED_AT };
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
                this.DeletedAt = DateTime.Now.ToString(DATE_FORMAT);
            }
        }

        /// <summary>
        /// Checks if the model is trashed.
        /// </summary>
        /// <returns>True if the model is trashed; otherwise, false.</returns>
        public bool IsTrashed() => this.DeletedAt != null;

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
                this.CreatedAt = DateTime.Now.ToString(DATE_FORMAT);
                this.changed.Add(CREATED_AT);
            }
            else if (this.TimestampsEnabled() && modelExists && transactionType == Transactions.Type.TYPE_UPDATE)
            {
                this.UpdatedAt = DateTime.Now.ToString(DATE_FORMAT);
                this.changed.Add(UPDATED_AT);
            }

            if (transactionType == Transactions.Type.TYPE_DELETE)
            {
                this.SetDeletedAt();
                this.changed.Add(DELETED_AT);
            }
        }

        //public void SetTimestamps(IDictionary<string, string> keys, bool isLoaded)
        //{
        //    foreach (KeyValuePair<string, string> key in keys)
        //    {
        //        this.SetTimestamp(key.Value, key.Key, isLoaded);
        //    }
        //}

        //public void SetTimestamp(string value, string key, bool isLoaded)
        //{
        //    switch (key)
        //    {
        //        case CREATED_AT:
        //            this.CreatedAt = value;
        //            break;
        //        case UPDATED_AT:
        //            this.UpdatedAt = value;
        //            break;
        //        case DELETED_AT:
        //            this.DeletedAt = value;
        //            this.softDeletes = true;
        //            break;
        //    }

        //    if (this.DeletedAt != null)
        //    {
        //        this.EnableTimestamps(true);
        //    }
        //    else
        //    {
        //        this.EnableTimestamps(false);
        //    }
        //}

        /// <summary>
        /// Gets the timestamps as key-value pairs.
        /// </summary>
        /// <returns>The timestamps as key-value pairs.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetTimestamps()
        {
            List<KeyValuePair<string, string?>> timestamps = new List<KeyValuePair<string, string?>>();

            foreach (string key in this.changed)
            {
                switch (key)
                {
                    case CREATED_AT:
                        timestamps.Add(new KeyValuePair<string, string?>(CREATED_AT, CreatedAt));
                        break;
                    case UPDATED_AT:
                        timestamps.Add(new KeyValuePair<string, string?>(UPDATED_AT, UpdatedAt));
                        break;
                    case DELETED_AT:
                        timestamps.Add(new KeyValuePair<string, string?>(DELETED_AT, DeletedAt));
                        break;
                }
            }

            return timestamps;
        }

        /// <summary>
        /// Checks if the model can be soft deleted.
        /// </summary>
        /// <returns>True if model can be soft deleted, false if not</returns>
        public bool CanBeSoftDeleted() => this.softDeletes;
    }
}
