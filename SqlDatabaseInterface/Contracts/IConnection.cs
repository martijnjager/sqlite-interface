using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Database.Enums;
using Database;

namespace Database.Contracts
{
    public interface IConnection
    {
        QueryResult<SaveStatus> RunSaveQuery(string query);
        SQLiteDataReader RunQuery(string query);

        string GetDatabasePath();
        QueryResult<SaveStatus> Result { get; }
    }
}
