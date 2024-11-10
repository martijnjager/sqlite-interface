using Database.Contracts.Attribute;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace Database.Attribute
{
    public class AttributeManager : CastManager, IAttributeManager
    {
        /// <summary>
        /// The attributes that are mass assignable.
        /// </summary>
        public IDictionary<string, string> Attributes { get; private set; }
        
        /// <summary>
        /// The attributes without modification.
        /// </summary>
        public IDictionary<string, string> Original { get; private set; }

        /// <summary>
        /// The attributes that will be saved as raw.
        /// </summary>
        public IList<string> RawAttributes { get; private set; }

        /// <summary>
        /// The primary key for the model.
        /// </summary>
        public string PrimaryKey { get; internal set; }

        private bool isLoaded;

        public AttributeManager()
        {
            this.Attributes = new Dictionary<string, string>();
            this.RawAttributes = new List<string>();
            this.Original = new Dictionary<string, string>();
            this.isLoaded = false;
            this.PrimaryKey = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryKey"></param>
        public void SetPrimaryKey(string primaryKey)
        {
            if (string.IsNullOrEmpty(this.PrimaryKey))
            {
                this.PrimaryKey = "id";

                return;
            }

            this.PrimaryKey = primaryKey;
        }

        /// <summary>
        /// Returns the value of an attribute
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public dynamic GetValue(string column)
        {
            if (Attributes.TryGetValue(column, out string? attributeValue))
            {
                string value = attributeValue;

                if (casts != null && !string.IsNullOrEmpty(value))
                {
                    foreach (Tuple<string, System.Type> cast in casts)
                    {
                        if (cast.Item1 == column)
                        {
                            Convert.ChangeType(value, cast.Item2);
                            return value;
                        }
                    }
                }

                return value;
            }

            return null;
        }

        public void Assign(ParamBag data)
        {
            foreach (Tuple<string, dynamic, bool> parameter in data.GetParameters())
            {
                bool shouldAssign = this.Original.ContainsKey(parameter.Item1) || !this.isLoaded;

                if (shouldAssign)
                {
                    this.Attributes[parameter.Item1] = parameter.Item2;
                    System.Type type = this.GetType();
                    type.GetProperty(parameter.Item1)?.SetValue(this, parameter.Item2);

                    if (parameter.Item3)
                    {
                        this.RawAttributes.Add(parameter.Item1);
                    }
                }
            }

            if (!this.isLoaded)
            {
                this.SyncOriginal();
            }
        }

        protected void SyncOriginal()
        {
            if (this.HasPrimaryKey() && !this.isLoaded)
            {
                this.Original = new Dictionary<string, string>(this.Attributes);
                this.isLoaded = true;
            }
        }

        public IDictionary<string, string> AttributesByKeys(string[]? keys = null)
        {
            if (keys is null || keys.Length < 1)
            {
                return this.Attributes;
            }

            IDictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (string key in keys)
            {
                if (Attributes.TryGetValue(key, out string? value))
                {
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }

        public void FillDefaultColumns(ReadOnlyCollection<DbColumn> columns)
        {
            if (columns is null || columns.Count < 1)
            {
                return;
            }

            foreach (var item in columns)
            {
                string key = item.ColumnName;

                if (string.IsNullOrEmpty(key)) continue;

                if (!this.Original.ContainsKey(key))
                {
                    this.Attributes.Add(key, null);
                    this.Original.Add(key, null);
                }

                if (key.Equals(TimestampManager.SOFT_DELETE_COLUMN))
                {
                    this.EnableTimestamps();
                }
            }
        }

        public bool HasChanges()
        {
            return !this.Original.SequenceEqual(this.Attributes);
        }

        public bool HasPrimaryKey()
        {
            return this.Attributes.ContainsKey(this.PrimaryKey) &&
                !string.IsNullOrEmpty(this.Attributes[this.PrimaryKey]);
        }

        public IDictionary<string, string> GetChanges()
        {
            var attributesNotInOriginal = this.Attributes.Where(x => !this.Original.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            var attributesInOriginal = this.Attributes.Where(x => this.Original.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            var changedAttributes = this.Attributes.Where(a => attributesInOriginal.ContainsKey(a.Key))
                .Where(
                    x => this.Original[x.Key] != x.Value && 
                    x.Key != TimestampManager.CREATED_AT && 
                    x.Key != TimestampManager.UPDATED_AT &&
                    x.Key != TimestampManager.DELETED_AT &&
                    x.Key != PrimaryKey
                )
            .ToDictionary(x => x.Key, x => x.Value);

            return attributesNotInOriginal.Concat(changedAttributes)
                .Concat(this.GetTimestamps())
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }

    /// <summary>
    /// Allows access to the AttributeManager
    /// </summary>
    public interface IAttribute
    {
        IAttributeManager Attributes { get; }
    }
}
