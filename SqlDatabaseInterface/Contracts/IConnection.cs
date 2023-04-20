using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Database.Contracts
{
    public interface IConnection
    {
        int RunNonQuery(string query);
        SQLiteDataReader RunQuery(string query);

        string GetDatabasePath();
    }
}
