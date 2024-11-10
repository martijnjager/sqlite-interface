using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Migrations.Enum
{
    public struct CascadeOptions
    {
        public const string CASCADE = "CASCADE";
        public const string SET_NULL = "SET NULL";
        public const string SET_DEFAULT = "SET DEFAULT";
        public const string RESTRICT = "RESTRICT";
        public const string NO_ACTION = "NO ACTION";

        public static string[] GetValues()
        {
            return new string[] { CASCADE, SET_NULL, SET_DEFAULT, RESTRICT, NO_ACTION };
        }
    }

    public struct CascadeActions
    {
        public const string ON_DELETE = "ON DELETE";
        public const string ON_UPDATE = "ON UPDATE";

        public static string[] GetValues()
        {
            return new string[] { ON_DELETE, ON_UPDATE };
        }
    }
}
