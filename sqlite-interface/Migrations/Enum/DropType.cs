using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Migrations.Enum
{
    public struct DropType
    {
        public const string COLUMN = "COLUMN";
        public const string INDEX = "INDEX";
        public const string TABLE = "TABLE";
        public const string TRIGGER = "TRIGGER";
        public const string VIEW = "VIEW";
        public const string FOREIGN_KEY = "FOREIGN_KEY";

        public static string[] GetValues()
        {
            return new string[] { COLUMN, INDEX, TABLE, TRIGGER, VIEW };
        }
    }
}
