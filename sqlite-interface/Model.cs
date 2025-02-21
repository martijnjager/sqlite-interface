using Database.Enums;
using Database.Transactions;
using Database.Contracts;
using Database.Extensions;
using Database.Relations;
using Database.Extensions.Model;
using System.Collections.ObjectModel;
using System.Data.Common;
using Database.Contracts.Event;

namespace Database
{
    /// <summary>
    /// Represents a model in the database.
    /// </summary>
    public class Model : Eloquent, IModel
    {
        protected bool disableSoftDeletes = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        /// <param name="table">The name of the database table.</param>
        public Model(ReadOnlyCollection<DbColumn> columns = null) : base()
        {
            if (this is IUseTimestamps)
            {
                this.Attributes.EnableTimestamps(this.disableSoftDeletes);
            }

            this.Attributes.FillDefaultColumns(columns);
        }

        /// <summary>
        /// Gets the path of the database.
        /// </summary>
        /// <returns>The path of the database.</returns>
        public static string GetDatabasePath()
        {
            return InstanceContainer.Instance.ConnectionManager().GetConnection().GetDatabasePath();
        }

        public static void SetConnection(string location, string source)
        {
            InstanceContainer.Instance.ConnectionManager().GetConnection().SetConnection(location, source);
        }

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <returns>The value of the column.</returns>
        public dynamic GetValue(string column)
        {
            return base.Attributes.GetValue(column);
        }

        /// <summary>
        /// Assigns the specified data to the model.
        /// </summary>
        /// <param name="data">The data to assign.</param>
        public void Assign(ParamBag data)
        {
            base.Attributes.Assign(data);
        }

        /// <summary>
        /// Adds casts to the model.
        /// </summary>
        /// <param name="casts"></param>
        public void AddCasts(List<Tuple<string, System.Type>> casts)
        {
            this.Attributes.AddCasts(casts);
        }

        /// <summary>
        /// Gets the attributes of the model by the specified keys.
        /// </summary>
        /// <param name="keys">The keys of the attributes to retrieve.</param>
        /// <returns>A dictionary containing the attributes.</returns>
        public IDictionary<string, string> AttributesByKeys(string[] keys = null)
        {
            return this.Attributes.AttributesByKeys(keys);
        }

        /// <summary>
        /// Loads the specified relations.
        /// </summary>
        /// <param name="relations">The relations to load.</param>
        public void Load(string relations)
        {
            List<ToLoad> relationsToLoad = this.BuildRelationTree(relations);

            foreach (var r in relationsToLoad)
            {
                RelationManager.LoadRelations(r, new[] { this });
            }
        }

        public bool HasChanges()
        {
            return this.Attributes.HasChanges();
        }

        public new QueryResult<SaveStatus> Delete()
        {
            if (!this.Attributes.HasPrimaryKey())
            {
                throw new Exception("Primary key not set.");
            }

            this.Where(this.Attributes.PrimaryKey, this.Attributes.GetValue(this.Attributes.PrimaryKey));

            return base.Delete();
        }

        public new IModel Restore()
        {
            if (!this.Attributes.HasPrimaryKey())
            {
                throw new Exception("Primary key not set.");
            }

            this.Where(this.Attributes.PrimaryKey, this.Attributes.GetValue(this.Attributes.PrimaryKey));
            return base.Restore();
        }

        public new IModel Update<T>() where T : IModel
        {
            if (!this.Attributes.HasPrimaryKey())
            {
                throw new Exception("Primary key not set.");
            }

            this.Where(this.Attributes.PrimaryKey, this.Attributes.GetValue(this.Attributes.PrimaryKey));

            return base.Update<T>();
        }

        public new IModel Create<T>() where T : IModel
        {
            if (this.Attributes.HasPrimaryKey())
            {
                return this.Update<T>();
            }

            this.Where(this.Attributes.PrimaryKey, this.Attributes.GetValue(this.Attributes.PrimaryKey));

            return base.Create<T>();
        }

        public static List<IModel> CreateMany<T>(ParamBag[] bags) where T : IModel
        {
            IModel model = InstanceContainer.ModelByKey(typeof(T).Name);

            return model.CreateMany<T>(bags);
        }

        /// <summary>
        /// Checks if the model is thrashed.
        /// </summary>
        /// <returns>True if the model is thrashed, otherwise false.</returns>
        public bool IsTrashed()
        {
            return this.Attributes.IsTrashed();
        }

        /// <summary>
        /// Sets the relations to eager load
        /// </summary>
        /// <param name="relations"></param>
        /// <returns></returns>
        public Eloquent With(string relations)
        {
            if (string.IsNullOrEmpty(relations))
            {
                return this;
            }

            var relationsToLoad = this.BuildRelationTree(relations);

            this.Relations.LoadEager(relationsToLoad);
            this.clauses.SetRelationsToLoad(relationsToLoad);

            return this;
        }

        public string PrimaryKey()
        {
            return this.Attributes.PrimaryKey;
        }

        public void Set(string attribute, dynamic value)
        {
            this.Attributes.Set(attribute, value);
        }
    }
}
