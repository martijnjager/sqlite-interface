using Database.Contracts.Migration;
using Database.Migrations.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Migrations
{
    public class Blueprint
    {
        private readonly BlueprintBase _table;

        public Blueprint()
        {
            this._table = new BlueprintBase();
        }

        public Blueprint PrimaryKey(string? id = null)
        {
            this._table.PrimaryKey(id);
            return this;
        }

        public Blueprint String(string column)
        {
            this._table.String(column);
            return this;
        }

        public Blueprint Text(string column)
        {
            this._table.Text(column);
            return this;
        }

        public Blueprint Int(string column)
        {
            this._table.Int(column);
            return this;
        }

        public Blueprint Timestamps()
        {
            this._table.Timestamps();
            return this;
        }


        public Blueprint SoftDeletes()
        {
            this._table.SoftDeletes();
            return this;
        }

        public Blueprint Table(string table)
        {
            this._table.Table(table);
            return this;
        }

        public Blueprint DropTable(string table)
        {
            this._table.DropTable(table);
            return this;
        }

        public Blueprint ForeignKey(string foreign)
        {
            this._table.ForeignKey(foreign);
            return this;
        }

        public Blueprint On(string table, string? key = null, string? action = null, string? option = null)
        {
            this._table.On(table, key, action, option);
            return this;
        }

        public Blueprint Unique()
        {
            this._table.Unique();
            return this;
        }

        public Blueprint Nullable()
        {
            this._table.Nullable();
            return this;
        }

        public Blueprint DropSoftDeletes()
        {
            this._table.DropSoftDeletes();
            return this;
        }

        public Blueprint Index(string column)
        {
            this._table.Index(column);
            return this;
        }

        public Blueprint DropColumn(string column)
        {
            this._table.DropColumn(column);
            return this;
        }

        public Blueprint Datetime(string column)
        {
            this._table.Datetime(column);
            return this;
        }

        public BlueprintBase GetBase()
        {
            return this._table;
        }
    }
}
