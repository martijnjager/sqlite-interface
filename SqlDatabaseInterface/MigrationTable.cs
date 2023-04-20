using Database;
using Database.Contracts;
using Database.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDatabaseInterface
{
    internal class MigrationTable : Migration, IMigration
    {
        public string Down()
        {
            throw new NotImplementedException();
        }

        public Blueprint Up()
        {
            this.table.Table("migrations");
            this.table.PrimaryKey();
            this.table.String("migration");
            this.table.Timestamps();

            return table;
        }
    }
}
