using System.Data.SQLite;
using Database.Enums;
using Database.Transactions;

namespace Database.Contracts
{
    public interface IConnection
    {
        //Task<QueryResult<SaveStatus>> RunSaveQueryAsync(string query);

        QueryResult<SaveStatus> RunQuery(Transaction query);

        //Task<SQLiteDataReader> RunQueryAsync(string query);

        string GetDatabasePath();

        QueryResult<SaveStatus> Result { get; }

        SQLiteConnection SqliteConnection();

        void SetConnection(string source, string location);

        bool IsOpen();

        void OpenConnection();

        void CloseConnection();
    }
}
