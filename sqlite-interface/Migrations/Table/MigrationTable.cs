using Database;
using Database.Contracts;
using Database.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Migrations.Table
{
    internal class MigrationTable : Migration, IMigration
    {
        public void Down(Blueprint table)
        {
            table.DropTable("migrations");
        }

        public void Up(Blueprint table)
        {
            table.Table("migrations");
            table.PrimaryKey();
            table.String("migration");
            table.Text("up");
            table.Text("down");
            table.Int("batch");
            table.Timestamps();
            table.SoftDeletes();
        }
    }
}
