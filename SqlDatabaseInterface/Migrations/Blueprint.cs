using System;
using System.Collections.Generic;
using System.Data.Common.EntitySql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Migrations
{
    public class Blueprint
    {
        private List<Tuple<string, string>> _items;

        private List<Tuple<string, string>> _foreign;

        private List<string> indexes;

        public string primaryKey { get; private set; }

        public Blueprint()
        {
            this._items = new List<Tuple<string, string>>();
            this._foreign = new List<Tuple<string, string>>();
            this.indexes = new List<string>();
        }

        public Blueprint PrimaryKey(string id = null)
        {
            if (id == null)
            {
                id = "id";
            }

            this.primaryKey = id;

            this.AddItem(id, "integer primary key autoincrement");

            return this;
        }

        public Blueprint String(string value)
        {
            this.AddItem(value, "string not null");

            return this;
        }

        public Blueprint Int(string value)
        {
            this.AddItem(value, "int not null");

            return this;
        }

        public Blueprint Text(string value)
        {
            this.AddItem(value, "text not null");

            return this;
        }

        public Blueprint Table(string value)
        {
            this.AddItem(value, null);

            return this;
        }

        public Blueprint ForeignKey(string foreign)
        {
            this._foreign.Add(new Tuple<string, string>(foreign, null));

            return this;
        }

        public Blueprint On(string table, string key = null)
        {
            Tuple<string, string> item = this._foreign[this._foreign.Count - 1];

            if (key == null)
            {
                key = "id";
            }

            if (item != null)
            {
                item = new Tuple<string, string>(item.Item1, "references " + table + "(" + key + ")");
            }

            this._foreign[this._foreign.Count - 1] = item;

            return this;
        }

        public Blueprint Unique()
        {
            Tuple<string, string> item = this._items[this._items.Count - 1];

            if (item != null)
            {
                string definitions = item.Item2 + " unique";
                item = new Tuple<string, string>(item.Item1, definitions);

                this._items[this._items.Count - 1] = item;
            }

            return this;
        }

        public Blueprint Nullable()
        {
            Tuple<string, string> item = this._items[this._items.Count - 1];

            if (item != null)
            {
                string definitions = item.Item2.Replace("not null", "null");
                item = new Tuple<string, string>(item.Item1, definitions);

                this._items[this._items.Count - 1] = item;
            }

            return this;
        }

        public Blueprint Timestamps()
        {
            this.AddItem("created_at", "datetime nullable");
            this.AddItem("updated_at", "datetime nullable");

            return this;
        }

        public Blueprint SoftDeletes()
        {
            this.AddItem("deleted_at", "datetime nullable");

            return this;
        }

        public Blueprint Index(string column)
        {
            this.indexes.Add(column);

            return this;
        }

        private void AddItem(string key, string data)
        {
            _items.Add(new Tuple<string, string>(key, data));
        }

        public List<Tuple<string, string>> GetDefinitions() => this._items;

        public List<Tuple<string, string>> GetRelations() => this._foreign;

        public List<string> GetIndexes() => this.indexes;
    }
}
