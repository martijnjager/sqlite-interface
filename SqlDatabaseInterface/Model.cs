using Database.Collections;
using Database.Grammar;
using Microsoft.Win32;
using Database;
using Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Database
{
    public class Model : Eloquent
    {
        protected delegate void creating();
        protected delegate void updating();
        protected delegate void saving();
        protected delegate void deleting();
        protected event creating create;
        protected event saving save;
        protected event updating update;
        protected event deleting delete;

        protected List<Tuple<string, Type>> casts;

        public Model(string table) : base(table)
        {
            if (this.timestamps)
            {
                this.RegisterTimestampEvents();
            }
        }

        public dynamic GetValue(string column)
        {
            if (this.DefaultAttributes.ContainsKey(column))
            {
                string value = this.Attributes[column];

                if (this.casts != null && !string.IsNullOrEmpty(value))
                {
                    foreach (Tuple<string, Type> cast in this.casts)
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

            Collections.Collection relations = this.GetRelations(column);

            if (relations.HasItems())
            {
                return relations;
            }

            throw new Exception("Attribute " + column + " not found for " + this.table + ", attributes available: " + this.Attributes.Keys.ToString());
        }

        public string GetDatabasePath()
        {
            return this.connection.GetDatabasePath();
        }

        public Model Assign(ParamBag data)
        {
            foreach (Tuple<string, string> parameter in data.GetParameters())
            {
                if (this.DefaultAttributes.ContainsKey(parameter.Item1))
                {
                    this.Attributes[parameter.Item1] = parameter.Item2;
                }
            }

            return this;
        }

        public QueryResult<SaveStatus> Delete()
        {
            try
            {
                this.delete?.Invoke();

                string sqlStatement = GrammarCompiler.CompileDeleteStatement(this, this.AttributesByKeys(new[] { this.PrimaryKey, "deleted_at" }));

                this.clauses.Clear();

                this.connection.RunSaveQuery(sqlStatement);
            } catch (Exception ex) {
                System.Console.WriteLine(ex.Message);
            }

            return this.connection.Result;
        }

        public QueryResult<SaveStatus> Save<T>()
        {
            string sqlStatement = string.Empty;
            bool isNew = false;
            Model model = null;

            string primaryKey = this.GetValue(this.PrimaryKey);
            this.HandleTimestamp();

            if (string.IsNullOrEmpty(primaryKey))
            {
                this.create?.Invoke();
                sqlStatement = GrammarCompiler.CompileInsertStatement(this.GetTable(), this.AttributesByKeys());
                isNew = true;
            }
            else
            {
                this.update?.Invoke();
                sqlStatement = GrammarCompiler.CompileUpdateStatement(this.GetTable(), this.AttributesByKeys());
            }

            this.clauses.Clear();

            this.connection.RunSaveQuery(sqlStatement);

            QueryResult<SaveStatus> result = this.connection.Result;

            var tempModel = InstanceContainer.Resolve<Model>(this.GetTable());

            model = isNew ? tempModel.Get().Last() : tempModel.Find(this.Attributes[this.PrimaryKey]);

            result.SetData(model);

            return result;
        }

        private IDictionary<string, string> AttributesByKeys(string[] keys = null)
        {
            IDictionary<string,string> dictionary = new Dictionary<string, string>();

            if (keys != null)
            {
                foreach (string key in keys)
                {
                    if (this.Attributes.ContainsKey(key))
                    {
                        dictionary.Add(key, this.Attributes[key]);
                    }
                }
            } else
            {
                dictionary = this.Attributes;
            }

            return dictionary;
        }

        private void HandleTimestamp()
        {
            string primaryKey = this.GetValue(this.PrimaryKey);
            if (this.timestamps && string.IsNullOrEmpty(primaryKey))
            {
                this.Attributes["created_at"] = "datetime()";
            }
            else if (this.timestamps && primaryKey != string.Empty)
            {
                this.Attributes["updated_at"] = "datetime()";
            }
        }

        private void HandleDelete()
        {
            if (this.useSoftDeletes)
            {
                this.Attributes["deleted_at"] = "datetime()";
            }
        }

        private void RegisterTimestampEvents()
        {
            this.save += this.HandleTimestamp;
            this.delete += this.HandleDelete;
        }
    }
}
