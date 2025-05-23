using Database.Cache;
using Database.Contracts;
using Database.Contracts.Relation;
using Database.Extensions;
using Database.Extensions.Model;
using Database.Relations.Join;
using System.Reflection;

namespace Database.Relations
{
    /// <summary>
    /// Represents a manager for handling relations between models.
    /// </summary>
    public class RelationManager
    {
        /// <summary>
        /// The relations loaded during runtime.
        /// </summary>
        public IDictionary<string, Relation> ActiveRelations { get; private set; }

        protected List<ToLoad> EagerLoad { get; private set;}

        private readonly ICache<IModel> cache = CacheFactory.GetOrCreate<IModel>("RelationModels");

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationManager"/> class.
        /// </summary>
        public RelationManager()
        {
            this.ActiveRelations = new Dictionary<string, Relation>();
            this.EagerLoad = new List<ToLoad>();
        }

        //public static IJoin AddJoin<T>(string primaryKey, string foreignKey, RelationType type)
        //{
        //    IJoin? join = null;
        //    switch (type)
        //    {
        //        case RelationType.HasOne:
        //            join = new BelongsTo(typeof(T), foreignKey, primaryKey);
        //            break;
        //        case RelationType.HasMany:
        //            join = new HasMany(typeof(T), foreignKey, primaryKey);
        //            break;
        //        case RelationType.BelongsTo:
        //            join = new BelongsTo(typeof(T), primaryKey, foreignKey);
        //            break;
        //    }

        //    return join;
        //}

        //public static IJoin AddBelongsToManyJoin<M, R, T>(string mainTablePrimaryKey, string targetTableKey)
        //{
        //    // primary key of model querying from is the foreign key of the intersection table
        //    // intersection table is inner joined with the model querying to
        //    // select from the model querying to and connect the joined intersection table

        //    Join join = Join.JoinFromMany<M, R, T>(mainTablePrimaryKey, targetTableKey, RelationType.BelongsToMany);
        //    return join;
        //}

        /// <summary>
        /// Checks if the relation is loaded.
        /// </summary>
        /// <param name="relation">The name of the relation.</param>
        /// <returns>True if the relation is loaded, false otherwise.</returns>
        public bool IsLoaded(string relation)
        {
            return this.ActiveRelations.TryGetValue(relation, out _);
        }

        /// <summary>
        /// Adds a new relation to the loaded relations.
        /// </summary>
        /// <param name="key">The key of the relation.</param>
        /// <param name="model">The model to add to the relation.</param>
        /// <param name="type">The type of the relation.</param>
        public void AddToActiveRelation(string key, IModel model, RelationType type)
        {
            if (this.IsLoaded(key))
            {
                this.ActiveRelations[key].Models.Add(model);
            }
            else
            {
                Relation relation = new()
                {
                    Models = new List<IModel>() { model },
                    Type = type
                };
                this.ActiveRelations.Add(key, relation);
            }
        }

        /// <summary>
        /// Gets the loaded relation for the specified key.
        /// </summary>
        /// <param name="key">The key of the relation.</param>
        /// <returns>The loaded relation if it exists, null otherwise.</returns>
        public Relation? GetLoadedRelation(string key)
        {
            return this.ActiveRelations.TryGetValue(key, out var relation) ? relation : null;
        }

        /// <summary>
        /// Gets all the loaded relations.
        /// </summary>
        /// <returns>A dictionary containing all the loaded relations.</returns>
        public IDictionary<string, Relation> GetLoadedRelations()
        {
            return this.ActiveRelations;
        }

        /// <summary>
        /// Resets the manager by clearing the registered joins.
        /// </summary>
        //public void Reset()
        //{
        //    this.RegisteredJoins.Clear();
        //}

        //public void LoadRelations(IDictionary<string>)

        /// <summary>
        /// Loads the specified relations for the given model.
        /// </summary>
        /// <param name="relations">The list of relation types to load.</param>
        /// <param name="model">The model to load the relations for.</param>
        //public void LoadRelations(List<Type> relations, Model model)
        //{
        //    Model currentModel = model;

        //    foreach (Type relation in relations)
        //    {
        //        MethodInfo method = currentModel.GetType().GetMethod(relation.Name.Plural());

        //        if (method is null)
        //        {
        //            method = currentModel.GetType().GetMethod(relation.Name.Singular());
        //        }

        //        if (method is null)
        //        {
        //            method = currentModel.GetType().GetMethod(relation.Name);
        //        }

        //        if (method is not null)
        //        {
        //            Join join = (Join)method.Invoke(currentModel, null);

        //            if (join is null)
        //            {
        //                continue;
        //            }

        //            Eloquent query = null;

        //            switch (join.Type)
        //            {
        //                case RelationType.BelongsTo:
        //                    query = join.Run(join.PrimaryKey, model.attributeManager.Attributes[join.ForeignKey]);
        //                    break;
        //                case RelationType.HasMany:
        //                case RelationType.HasOne:
        //                    query = join.Run(join.ForeignKey, model.attributeManager.Attributes[join.PrimaryKey]);
        //                    break;
        //            }

        //            List<IModel> loadedModels = query.Get<Model>();

        //            foreach (Model m in loadedModels)
        //            {
        //                currentModel.relations.AddToLoadedRelation(relation.Name, m, join.Type);

        //                var relationsLeft = relations.SkipWhile(r => r != relation).Skip(1).ToList();
        //                if (relationsLeft.Count > 0)
        //                {
        //                    //LoadRelations(relationsLeft, m);
        //                }
        //            }
        //        }
        //    }
        //}

        public static void LoadRelations(ToLoad relation, IEnumerable<IModel> models)
        {
            IModel currentModel = models.FirstOrDefault();

            if (currentModel is null)
            {
                return;
            }

            MethodInfo method = currentModel.GetRelationMethod(relation.Key);

            if (method?.Invoke(currentModel, null) is not IJoin join)
            {
                return;
            }

            Eloquent query = null;

            // TODO: connect BelongsToMany
            dynamic[] ids = null;
            string primarykey = null;
            string foreignkey = null;
            switch (join.Type)
            {
                case RelationType.BelongsTo:
                case RelationType.BelongsToMany:
                    primarykey = join.ForeignKey;
                    foreignkey = join.PrimaryKey;
                    break;
                case RelationType.HasMany:
                case RelationType.HasOne:
                    primarykey = join.PrimaryKey;
                    foreignkey = join.ForeignKey;
                    break;
            }

            ids = models.Select(m => m.GetValue(primarykey)).Distinct().ToArray();

            ICache<IModel> cache = CacheFactory.GetOrCreate<IModel>("RelationModels");

            string cacheKeyName = join.TargetTable?.Name ?? join.MainTable.Name;

            var uncachedIds = ids.Where(id =>
            {
                string cacheKey = CacheFactory.GenerateCacheKey(cacheKeyName, id);
                return cache.Get(cacheKey) is null;
            });
            var cachedIds = ids.Except(uncachedIds);

            if (uncachedIds.Any())
            {
                query = join.BuildQuery(foreignkey, uncachedIds.ToArray());
                Collection loadedModels = query.Get();

                // Cache newly loaded models
                loadedModels.ForEach(m =>
                {
                    string id = m.GetValue(m.Attributes.PrimaryKey).ToString();
                    string key = CacheFactory.GenerateCacheKey(cacheKeyName, id);
                    cache.Store(key, m);
                });

                foreach (IModel model in models)
                {
                    var modelsToConnect = loadedModels.Where(m => 
                        m.GetValue(foreignkey) == model.GetValue(primarykey)
                    );

                    foreach (Model m in modelsToConnect.Cast<Model>())
                    {
                        model.Relations.AddToActiveRelation(relation.Key, m, join.Type);
                    }
                }
            }

            foreach (IModel model in models) 
            {
                string lookupId = model.GetValue(primarykey).ToString();
                string cacheKey = CacheFactory.GenerateCacheKey(cacheKeyName, lookupId);

                if (cache.Get(cacheKey) is IModel cachedModel)
                {
                    model.Relations.AddToActiveRelation(relation.Key, cachedModel, join.Type);
                }
            }

            if (relation.Children.Count >0)
            {
                var allModels = models.SelectMany(m => m.Relations.GetLoadedRelation(relation.Key)?.Models ?? new List<IModel>());

                relation.Children.ForEach(relation =>
                {
                    LoadRelations(relation, allModels);
                });
            }
        }

        /// <summary>
        /// Recursively loads relations
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="models"></param>
        public static void LoadRelations(List<ToLoad> relations, IEnumerable<IModel> models)
        {
            foreach (ToLoad relation in relations)
            {
                LoadRelations(relation, models);
            }
        }

        public static void LoadRelations(Collection items, Transactions.Transaction transaction)
        {
            if (items.Count > 0 && transaction.GetRelationsToLoad().Count > 0)
            {
                foreach (ToLoad relation in items.First().Relations.ToEagerLoad())
                {
                    LoadRelations(relation, items);
                }
            }
        }

        /// <summary>
        /// Sets relations to eager load
        /// </summary>
        /// <param name="relations"></param>
        public void LoadEager(List<ToLoad> relations)
        {
            this.EagerLoad = relations;
        }

        public bool CanEagerLoad()
        {
            return this.EagerLoad.Count > 0;
        }

        public List<ToLoad> ToEagerLoad()
        {
            return this.EagerLoad;
        }
    }

    /// <summary>
    /// Represents a relation between models.
    /// </summary>
    public interface IRelation
    {
        RelationManager Relations { get; }
    }
}
