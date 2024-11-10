using Database.Attribute;
using Database.Migrations.Enum;
using System.Text;

namespace Database.Migrations
{
    /// <summary>
    /// Represents a blueprint for creating database tables and defining their structure.
    /// </summary>
    public class BlueprintBase
    {
        private readonly List<KeyValuePair<string, string>> _items;
        private readonly List<KeyValuePair<string, string>> _foreign;
        private readonly List<string> indexes;
        private readonly IDictionary<string, List<string>> droppable;
        private bool alterTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlueprintBase"/> class.
        /// </summary>
        public BlueprintBase()
        {
            this._items = new List<KeyValuePair<string, string>>();
            this._foreign = new List<KeyValuePair<string, string>>();
            this.indexes = new List<string>();
            this.droppable = new Dictionary<string, List<string>>();
            this.alterTable = false;
        }

        /// <summary>
        /// Sets the primary key of the table.
        /// </summary>
        /// <param name="id">The name of the primary key column. If not provided, the default name is "id".</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase PrimaryKey(string? id = null)
        {
            this.AddItem(id ?? "id", "integer primary key autoincrement");

            return this;
        }

        /// <summary>
        /// Adds a string column to the table.
        /// </summary>
        /// <param name="value">The name of the column.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase String(string value)
        {
            this.AddItem(value, "string not null");

            return this;
        }

        /// <summary>
        /// Adds an integer column to the table.
        /// </summary>
        /// <param name="value">The name of the column.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Int(string value)
        {
            this.AddItem(value, "int not null");

            return this;
        }

        /// <summary>
        /// Adds a text column to the table.
        /// </summary>
        /// <param name="value">The name of the column.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Text(string value)
        {
            this.AddItem(value, "text not null");

            return this;
        }

        /// <summary>
        /// Sets the name of the table.
        /// </summary>
        /// <param name="value">The name of the table.</param>
        /// <param name="alterTable">Indicates whether the table is being altered.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Table(string value, bool alterTable = false)
        {
            this.AddItem(value, null);
            this.alterTable = alterTable;

            return this;
        }

        /// <summary>
        /// Adds a foreign key constraint to the table.
        /// </summary>
        /// <param name="foreign">The name of the foreign key column.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase ForeignKey(string foreign)
        {
            _foreign.Add(new KeyValuePair<string, string>(foreign, null));

            return this;
        }

        /// <summary>
        /// Specifies the referenced table and column for a foreign key constraint.
        /// </summary>
        /// <param name="table">The name of the referenced table.</param>
        /// <param name="key">The name of the referenced column. If not provided, the default name is "id".</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase On(string table, string? key = null, string? action = null, string? option = null)
        {
            KeyValuePair<string, string> item = this._foreign[_foreign.Count - 1];

            key ??= "id";

            if (!item.Equals(default(KeyValuePair<string, string>)))
            {
                var sb = new StringBuilder();
                sb.Append(" references ");
                sb.Append(table);
                sb.Append('(');
                sb.Append(key);
                sb.Append(')');

                if (action != null && option != null && CascadeActions.GetValues().Contains(action) && CascadeOptions.GetValues().Contains(option))
                {
                    sb.Append(' ');
                    sb.Append(action);
                    sb.Append(' ');
                    sb.Append(option);
                }

                item = new KeyValuePair<string, string>(item.Key, sb.ToString());
            }

            this._foreign[_foreign.Count - 1] = item;

            return this;
        }

        /// <summary>
        /// Adds a unique constraint to the last added column.
        /// </summary>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Unique()
        {
            KeyValuePair<string, string> item = this._items[_items.Count - 1];

            if (!item.Equals(default(KeyValuePair<string, string>)))
            {
                string definitions = item.Value + " unique";
                item = new KeyValuePair<string, string>(item.Key, definitions);

                this._items[_items.Count - 1] = item;
            }

            return this;
        }

        /// <summary>
        /// Makes the last added column nullable.
        /// </summary>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Nullable()
        {
            KeyValuePair<string, string> item = this._items[_items.Count - 1];

            if (!item.Equals(default(KeyValuePair<string, string>)))
            {
                string definitions = item.Value.Replace("not null", "null");
                item = new KeyValuePair<string, string>(item.Key, definitions);

                this._items[_items.Count - 1] = item;
            }

            return this;
        }

        /// <summary>
        /// Adds the "created_at" and "updated_at" columns to the table.
        /// </summary>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Timestamps()
        {
            this.AddItem(TimestampManager.CREATED_AT, "timestamp");
            this.AddItem(TimestampManager.UPDATED_AT, "timestamp nullable");

            return this;
        }

        /// <summary>
        /// Adds the "deleted_at" column to the table for soft deletes.
        /// </summary>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase SoftDeletes()
        {
            this.AddItem(TimestampManager.SOFT_DELETE_COLUMN, "timestamp nullable");

            return this;
        }

        /// <summary>
        /// Adds a datetime column to the table.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Datetime(string column)
        {
            this.AddItem(column, "timestamp not null");

            return this;
        }

        /// <summary>
        /// Drops the "deleted_at" column from the table.
        /// </summary>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase DropSoftDeletes()
        {
            this.AddDropItem(DropType.COLUMN, TimestampManager.SOFT_DELETE_COLUMN);

            return this;
        }

        /// <summary>
        /// Adds an index to the table.
        /// </summary>
        /// <param name="column">The name of the column to index.</param>
        /// <returns>The <see cref="BlueprintBase"/> instance.</returns>
        public BlueprintBase Index(string column)
        {
            indexes.Add(column);

            return this;
        }

        /// <summary>
        /// Drops a column from the table.
        /// </summary>
        /// <param name="column">The name of the column to drop.</param>
        public void DropColumn(string column)
        {
            this.AddDropItem(DropType.COLUMN, column);
        }

        /// <summary>
        /// Drops the table from the database.
        /// </summary>
        /// <param name="table">The name of the table to drop.</param>
        public void DropTable(string table)
        {
            this.AddDropItem(DropType.TABLE, table);
        }

        public void DropIndex(string column)
        {
            this.AddDropItem(DropType.INDEX, column);
        }

        public void DropForeignKey(string foreignKey)
        {
            this.AddDropItem(DropType.FOREIGN_KEY, foreignKey);
        }

        private void AddDropItem(string type, string column)
        {
            if (droppable.TryGetValue(type, out List<string>? columns))
            {
                columns.Add(column);
            }
            else
            {
                droppable.Add(type, new List<string> { column });
            }
        }

        private void AddItem(string key, string data)
        {
            _items.Add(new KeyValuePair<string, string>(key, data));
        }

        /// <summary>
        /// Gets the column definitions of the table.
        /// </summary>
        /// <returns>A list of key-value pairs representing the column definitions.</returns>
        public List<KeyValuePair<string, string>> GetDefinitions() => this._items;

        /// <summary>
        /// Gets the foreign key relations of the table.
        /// </summary>
        /// <returns>A list of key-value pairs representing the foreign key relations.</returns>
        public List<KeyValuePair<string, string>> GetRelations() => this._foreign;

        /// <summary>
        /// Gets the indexes of the table.
        /// </summary>
        /// <returns>A list of column names representing the indexes.</returns>
        public List<string> GetIndexes() => this.indexes;

        /// <summary>
        /// Gets the columns to be dropped from the table.
        /// </summary>
        /// <returns>A list of column names to be dropped.</returns>
        public IDictionary<string, List<string>> GetDroppable() => this.droppable;

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <returns>The name of the table.</returns>
        public string? GetTable() => _items.Find((item) => item.Value is null).Key;

        /// <summary>
        /// Determines whether there are columns to be dropped from the table.
        /// </summary>
        /// <returns><c>true</c> if there are columns to be dropped; otherwise, <c>false</c>.</returns>
        public bool HasDropData() => droppable.Count > 0;

        /// <summary>
        /// Determines whether the table is being altered.
        /// </summary>
        /// <returns><c>true</c> if the table is being altered; otherwise, <c>false</c>.</returns>
        public bool HasAlterData() => this.alterTable;
    }
}