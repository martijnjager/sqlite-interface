using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using Database.Contracts;
using Database.Relations;
using Database.Grammar;
using BaseCollection = Database.Collections.Collection;

namespace Database
{
    public delegate void DatabaseCallback(Eloquent query);
    public class Eloquent
    {
        protected IDictionary<string, string> clauses;

        private readonly IDictionary<string, string> parameters;

        internal IConnection connection;

        protected IDictionary<string, string> Attributes { get; private set; }

        protected IDictionary<string, string> DefaultAttributes { get; set; }

        protected bool timestamps;

        protected string PrimaryKey;

        protected string table;

        protected bool useSoftDeletes;

        private readonly List<string> relations;

        private readonly IDictionary<string, Collection> relationModels;

        private readonly List<Join> joins;

        public Eloquent(string table)
        {
            this.joins = new List<Join>();
            this.clauses = new Dictionary<string, string>();
            this.parameters = new Dictionary<string, string>();
            this.connection = new Connection();
            this.Attributes = new Dictionary<string, string>();
            this.DefaultAttributes= new Dictionary<string, string>();
            this.relations = new List<string>();
            this.relationModels = new Dictionary<string, Collection>();
            this.timestamps = false;
            this.table = table;
            this.SetTable();
            this.SetPrimaryKey();
            this.FillDefaultColumns();
        }

        private void SetPrimaryKey()
        {
            if (string.IsNullOrEmpty(this.PrimaryKey))
            {
                this.PrimaryKey = "id";
            }
        }

        private void SetTable()
        {
            if (this.table.Equals(string.Empty))
            {
                this.table = Pluralize(this.GetType().Name);
            }
        }

        public string GetTable()
        {
            return this.table;
        }

        public IConnection GetConnection() => this.connection;

        internal static string Pluralize(string value)
        {
            PluralizationService pluralizationService = PluralizationService.CreateService(CultureInfo.InstalledUICulture);
            return pluralizationService.Pluralize(value);
        }

        internal static string Singularize(string value)
        {
            PluralizationService pluralizationService = PluralizationService.CreateService(CultureInfo.InstalledUICulture);
            return pluralizationService.Singularize(value);
        }

        /// <summary>
        /// Returns the columns of a model
        /// </summary>
        /// <returns>ICollection columns of model</returns>
        public ICollection<string> GetKeys()
        {
            return this.Attributes.Keys;
        }

        internal IDictionary<string, string> GetAttributes()
        {
            return this.Attributes;
        }

        public void AddRelation(Model relation)
        {
            string key = relation.GetTable();
            if (this.relationModels.ContainsKey(key))
            {
                this.relationModels[key].Add(relation);
            } else
            {
                this.relationModels.Add(key, new Collection(new List<Model> { relation }));
            }
        }

        public BaseCollection GetRelations(string name)
        {
            if (this.relationModels.ContainsKey(name))
            {
                return this.relationModels[name];
            }
            return new Collection(new List<Model>());
        }

        public IDictionary<string, Collection> GetRelations()
        {
            return this.relationModels;
        }

        public Model Find(string key)
        {
            return this.Where("id", key).Get().First();
        }

        public Eloquent Select(string columns = "*")
        {
            this.clauses.Add("select", columns);

            return this;
        }

        public Eloquent Select(string[] columns)
        {
            this.clauses.Add("select", string.Join(", ", columns));

            return this;
        }

        public Eloquent From(string table)
        {
            this.clauses.Add("from", table);

            return this;
        }

        public Eloquent Has(string table, string foreignId = null)
        {
            if (foreignId == null)
            {
                foreignId = Singularize(this.GetTable()) + "_id";
            }

            //Join relation = new Join(table, this.GetTable(), foreignId, "id", "to");
            //this.joins.Add(relation);
            this.From(table);
            this.Where(foreignId, this.Attributes.First().Value);
            return this;
        }

        public Eloquent BelongsTo(string table, string id = null, string foreignId = null)
        {
            if (foreignId == null)
            {
                foreignId = table + "_id";
            }

            id = id ?? "id";

            //Join relation = new Join(this.GetTable(), table, id, foreignId, "from");
            //this.joins.Add(relation);
            this.From(table);
            this.Where(id, this.Attributes.Where(v => v.Key == foreignId).First().Value);
            return this;
        }

        public Eloquent Where(string where, string op, string value = null)
        {
            if (value == null)
            {
                value = op;

                this.clauses.Add("where", where + " = '" + value + "'");
            }
            else
            {
                this.clauses.Add("where", where + op + " " + value + "'");
            }

            return this;
        }

        public Eloquent Where(DatabaseCallback callback)
        {
            callback(this);
            return this;
        }

        public Eloquent GroupBy(string column)
        {
            this.clauses.Add("groupBy", column);
            return this;
        }

        public Eloquent Having(string have, string op, string value = null)
        {
            if (value == null)
            {
                value = op;

                this.clauses.Add("having", have + " = '" + value + "'");
            }
            else
            {
                this.clauses.Add("having", have + op + " '" + value + "'");
            }

            return this;
        }

        public Eloquent OrderBy(string column, string order = "asc")
        {
            this.AddOrder(column, order);

            return this;
        }

        public Eloquent Limit(int limit)
        {
            this.clauses.Add("limit", limit.ToString());

            return this;
        }

        public void Load(string relations)
        {
            foreach (string relation in relations.Split(','))
            {
                string[] r = relation.Split('.');
                int index = 0;

                if (relations.Length > 0)
                {
                    this.LoadRelation(r, index);
                }
            }
        }

        public bool IsThrashed()
        {
            string deletedValue = this.First().GetValue("deleted_at");

            return string.IsNullOrEmpty(deletedValue);
        }

        public Eloquent WithTrashed()
        {
            if (this.useSoftDeletes)
            {
                this.Where("deleted_at", "!=", null);
            }

            return this;
        }

        private void LoadRelation(string[] relations, int index)
        {
            if (index >= relations.Length)
            {
                return;
            }

            string currentRelation = relations[index];

            Eloquent relationQuery = InstanceContainer.Invoke(this, currentRelation);

            if (this.relationModels.ContainsKey(currentRelation))
            {
                foreach (Model model in relationQuery.Get().All())
                {
                    model.LoadRelation(relations, index + 1);
                    this.relationModels[currentRelation].Add(model);
                }
            }
            else
            {
                Collection collection = new Collection(relationQuery.Get().All());

                foreach (Model model in collection.All())
                {
                    model.LoadRelation(relations, index + 1);
                }

                this.relationModels.Add(currentRelation, collection);
            }
        }

        public Collection Get()
        {
            string sql = this.CompileQuery();
            SQLiteDataReader reader = this.connection.RunQuery(sql);

            this.clauses.Clear();
            this.joins.Clear();

            List<Model> items = new List<Model>();
            ParamBag paramBag = InstanceContainer.Instance.ParamBag();

            while (reader.Read())
            {
                string table = reader.GetTableName(0);

                Model mainModel = (Model)InstanceContainer.Resolve<Model>(table);
                List<dynamic> relationModels = new List<dynamic>();

                foreach (string relation in relations)
                {
                    relationModels.Add(InstanceContainer.Resolve<Model>(relation));
                }

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string key = reader.GetName(i);
                    string value = reader[i].ToString();

                    paramBag.Add(key, value);

                    //if (mainModel.GetValue(key) != null)
                    //{
                    //    this.AssignInRelation(relationModels, key, value);
                    //}
                    //else
                    //{
                        //mainModel.Assign(paramBag);
                    //}
                }
                mainModel.Assign(paramBag);

                foreach (dynamic relation in relationModels)
                {
                    mainModel.AddRelation(relation);
                }

                items.Add(mainModel);
            }

            reader.Close();

            return new Collection(items);
        }

        private void AssignInRelation(List<dynamic> relations, string key, string value)
        {
            foreach (Model model in relations)
            {
                if (model.GetKeys().Contains(key) && model.GetValue(key) == null)
                {
                    model.Assign(new ParamBag().Add(key, value));
                    return;
                }
            }
        }

        public Model First()
        {
            return this.Get().First();
        }

        private string CompileQuery()
        {
            this.HandlePrecautions();

            return GrammarCompiler.Compile(this.clauses, this.joins, this.parameters);
        }

        private void HandlePrecautions()
        {
            if (!this.clauses.ContainsKey("select"))
            {
                this.Select();
            }

            if (!this.clauses.ContainsKey("from"))
            {
                this.From(this.GetTable().ToLower());
            }
        }

        private void AddOrder(string column, string order)
        {
            this.parameters.Add(":o" + column, order);
        }

        private void FillDefaultColumns()
        {
            SQLiteDataReader reader = this.GetConnection().RunQuery("pragma table_info(" + this.GetTable() + ")");

            while (reader.Read())
            {
                string key = reader.GetValue(1).ToString();

                if (!this.Attributes.ContainsKey(key))
                {
                    this.Attributes.Add(key, null);
                    this.DefaultAttributes.Add(key, null);
                }
            }
        }
    }
}
