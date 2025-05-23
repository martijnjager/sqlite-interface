using System.Data.Common;
using Database.Enums;
using Database.Transactions;

namespace Database.Contracts
{
    public interface IConnection
    {
        //Task<QueryResult<SaveStatus>> RunSaveQueryAsync(string query);

        QueryResult<SaveStatus> RunQuery(Transaction query);

        //Task<DBDataReader> RunQueryAsync(string query);

        string GetDatabasePath();

        QueryResult<SaveStatus> Result { get; }

        DbConnection SqliteConnection();

        void SetConnection(string source, string? location = null);

        bool IsOpen();

        void OpenConnection();

        void CloseConnection();
    }
}
