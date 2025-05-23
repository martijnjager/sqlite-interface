using Database.Extensions.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Attribute
{
    public interface IAttributeManager : ICastManager
    {
        ///<summary>
        /// The attributes that are mass assignable.
        /// </summary>
        IDictionary<string, string> Attributes { get; }

        /// <summary>
        /// The attributes that are custom set by user.
        /// </summary>
        IDictionary<string, dynamic> CustomAttributes { get; }

        /// <summary>
        /// The attributes without modification.
        /// </summary>
        IDictionary<string, string> Original { get; }

        /// <summary>
        /// The attributes that will be saved as raw.
        /// </summary>
        IList<string> RawAttributes { get; }

        /// <summary>
        /// The properties defined of the model.
        /// </summary>
        IEnumerable<Property> Properties { get; }

        /// <summary>
        /// The primary key for the model.
        /// </summary>
        string PrimaryKey { get; }

        /// <summary>
        /// Sets the properties for the model.
        /// </summary>
        /// <param name="properties"></param>
        void SetProperties(IEnumerable<Property> properties);

        /// <summary>
        /// Sets the primary key for the model.
        /// </summary>
        /// <param name="primaryKey">The primary key.</param>
        void SetPrimaryKey(string primaryKey);

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        dynamic GetValue(string column);

        /// <summary>
        /// Sets the attributes for the model.
        /// </summary>
        /// <param name="attributes"></param>
        void SetAttributes(IDictionary<string, dynamic?> attributes);

        /// <summary>
        /// Sets attributes for the model from the reader.
        /// </summary>
        /// <param name="data"></param>
        void AssignByReader(ParamBag data);

        /// <summary>
        /// Gets the attributes by the specified keys
        /// Gets all keys if no keys are specified.
        /// </summary>
        IDictionary<string, string> AttributesByKeys(string[] keys = null);

        /// <summary>
        /// Sets the attributes that are mass assignable.
        /// </summary>
        /// <param name="columns"></param>
        void FillDefaultColumns(ReadOnlyCollection<DbColumn> columns);

        /// <summary>
        /// Indicates whether any mass assignable values have been changed.
        /// </summary>
        /// <returns></returns>
        bool HasChanges();


        /// <summary>
        /// Indicates whether the model has a primary key.
        /// </summary>
        /// <returns></returns>
        bool HasPrimaryKey();

        /// <summary>
        /// Gets the changes that have been made to the model.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetChanges();

        /// <summary>
        /// Resyncs the original attributes.
        /// </summary>
        void ResyncOriginal();

        /// <summary>
        /// Sets the value of the specified attribute.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        void Set(string attribute, dynamic value);

        /// <summary>
        /// Indicates whether the specified attribute has been changed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if attribute has changed since the last read from database</returns>
        bool HasChanged(string key);
    }
}
