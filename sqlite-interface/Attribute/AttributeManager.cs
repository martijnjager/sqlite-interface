using Database.Contracts.Attribute;
using Database.Extensions.Model.Attribute;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;

namespace Database.Attribute
{
    public class AttributeManager : CastManager, IAttributeManager
    {
        /// <summary>
        /// Keeps track of changes.
        /// </summary>
        public IDictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Attributes loaded from the database.
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

        /// <summary>
        /// The properties defined of the model.
        /// </summary>
        public IEnumerable<Property> Properties { get; private set; }

        public AttributeManager()
        {
            this.Attributes = new Dictionary<string, string>();
            this.RawAttributes = new List<string>();
            this.Original = new Dictionary<string, string>();
            this.isLoaded = false;
            this.PrimaryKey = string.Empty;
        }

        public void SetProperties(IEnumerable<Property> properties)
        {
            this.Properties = properties;

            foreach (Property property in properties)
            {
                this.AddCasts(new System.Collections.Generic.List<System.Tuple<string, System.Type>> {
                    new(property.Name, property.Type)
                });
            }
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

                if (Casts != null && !string.IsNullOrEmpty(value))
                {
                    foreach (Tuple<string, System.Type> cast in Casts)
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

        public void SetAttributes(IDictionary<string, dynamic?> data)
        {
            System.Type type = this.GetType();

            foreach (var pair in data)
            {
                var attribute = this.Properties.FirstOrDefault(x => x.Name.Equals(pair.Key));
                bool shouldAssign = attribute is not null || !this.isLoaded;

                if (!shouldAssign)
                {
                    continue;
                }

                this.Attributes[pair.Key] = pair.Value;
                type.GetProperty(pair.Key)?.SetValue(this, pair.Value);
            }

            if (!this.isLoaded)
            {
                this.SyncOriginal();
            }
        }

        public void AssignByReader(ParamBag data)
        {
            System.Type type = this.GetType();

            foreach (Tuple<string, dynamic, bool> parameter in data.GetParameters())
            {
                bool shouldAssign = this.Original.ContainsKey(parameter.Item1) || !this.isLoaded;

                if (shouldAssign)
                {
                    this.Attributes[parameter.Item1] = parameter.Item2;

                    if (parameter.Item2 is not null)
                    {
                        Type valueType = parameter.Item2.GetType();

                        if (valueType != typeof(string))
                        {
                            this.AddCasts(new List<Tuple<string, System.Type>> {
                                new(parameter.Item1, valueType)
                            });
                        }
                    }

                    type.GetProperty(parameter.Item1)?.SetValue(this, parameter.Item2);

                    if (parameter.Item3)
                    {
                        this.RawAttributes.Add(parameter.Item1);
                    }
                }
            }

            //string[] timestampColumns = TimestampColumns();

            //if (data.HasAnyKey(timestampColumns))
            //{
            //    this.SetTimestamps(data.GetByKeys(timestampColumns), this.isLoaded);
            //}

            if (!this.isLoaded)
            {
                this.SyncOriginal();
            }
        }
        
        public void Set(string attribute, dynamic value)
        {
            this.Attributes[attribute] = value.ToString();
        }

        public void ResyncOriginal()
        {
            this.Original = new Dictionary<string, string>(this.Attributes);
            this.isLoaded = true;
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

            string[] timestampColumns = TimestampColumns();

            List<Tuple<string, System.Type>> timestampCasts = new();

            foreach (var item in columns)
            {
                string key = item.ColumnName;

                if (string.IsNullOrEmpty(key)) continue;

                if (!this.Original.ContainsKey(key))
                {
                    this.Attributes.Add(key, null);
                    this.Original.Add(key, null);
                }

                if (timestampColumns.Contains(key))
                {
                    var timecast = new Tuple<string, System.Type>(key, typeof(DateTime));
                    timestampCasts.Add(timecast);
                }
            }

            if (timestampCasts.Count > 0)
            {
                this.EnableTimestamps();
                this.AddTimestampCasts();
            }
        }

        public bool HasChanges()
        {
            return !this.Original.SequenceEqual(this.Attributes);
        }

        public bool HasChanged(string key)
        {
            if (this.Original.ContainsKey(key) && this.Attributes.ContainsKey(key))
            {
                return this.Original[key] != this.Attributes[key];
            }

            return false;
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
