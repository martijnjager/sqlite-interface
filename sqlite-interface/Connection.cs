using Database.Contracts;
using System.Data.SQLite;
using Database.Enums;
using Database.Console;
using System.Data.Common;
using Database.Transactions;

namespace Database
{
    public class Connection : IConnection
    {
        const string databaseSource = @"Data Source=database.sqlite";
        const string databaseLocation = "database.sqlite";
        const string testDatabaseSource = ":memory:";
        private SQLiteConnection? connection = null;

        public QueryResult<SaveStatus> Result { get; private set; }

        public Connection(string? location = null, string? source = null)
        {
            this.SetConnection(source ?? databaseSource, location ?? databaseLocation);
            this.Result = new QueryResult<SaveStatus>();
        }

        public void SetConnection(string source, string? location = null)
        {
            string? defaultLocation = location;
            string defaultSource = source;
            if (!string.IsNullOrEmpty(defaultLocation) && !defaultSource.Contains(testDatabaseSource) && !File.Exists(defaultLocation))
            {
                SQLiteConnection.CreateFile(defaultLocation);
            }

            connection = new SQLiteConnection(defaultSource);
        }

        public SQLiteConnection SqliteConnection()
        {
            return connection ?? throw new InvalidOperationException("Connection is not initialized.");
        }

        public string GetDatabasePath()
        {
            if (this.connection is null)
            {
                throw new InvalidOperationException("Connection is not initialized.");
            }

            this.OpenConnection();

            string path = this.connection.FileName;
            this.connection.Close();

            return path;
        }

        public QueryResult<SaveStatus> RunSaveQuery(string query)
        {
            Result = new QueryResult<SaveStatus>();
            Result.SetStatus(SaveStatus.Pending);
            try
            {
                if (this.connection is null)
                {
                    throw new InvalidOperationException("Connection is not initialized.");
                }

                this.OpenConnection();
                SQLiteTransaction transaction = this.connection.BeginTransaction();
                SQLiteCommand command = this.connection.CreateCommand();
                command.CommandText = query;
                int numberAffectedRows = command.ExecuteNonQuery();
                transaction.Commit();
                transaction.Dispose();
                command.Dispose();
                Result.SetStatus(SaveStatus.Success);
                Result.SetMessage("Number affected rows:" + numberAffectedRows);
            }
            catch (Exception ex)
            {
                Result.SetStatus(SaveStatus.Error);
                Result.SetMessage(ex.Message);
            }
            finally
            {
                this.CloseConnection();
            }

            return Result;
        }

        public QueryResult<SaveStatus> RunQuery(Transaction transaction)
        {
            SQLiteDataReader? reader = null;
            int numberAffectedRows = 0;
            Result = new QueryResult<SaveStatus>();
            Result.SetStatus(SaveStatus.Pending);

            try
            {
                switch (transaction.Type)
                {
                    case Transactions.Type.TYPE_SELECT:
                        reader = transaction.Query.ExecuteReader();
                        break;
                    case Transactions.Type.TYPE_INSERT:
                    case Transactions.Type.TYPE_UPDATE:
                    case Transactions.Type.TYPE_DELETE:
                        numberAffectedRows = transaction.Query.ExecuteNonQuery();
                        break;
                }
                
                Result.SetStatus(SaveStatus.Success);
                Result.SetReader(reader);
                Result.SetMessage("Number affected rows:" + numberAffectedRows);
            }
            catch (Exception e)
            {
                Result.SetStatus(SaveStatus.Error);
                Result.SetMessage(e.Message);
            }

            return Result;
        }

        //public SQLiteDataReader RunQuery(string query)
        //{
        //    SQLiteTransaction transaction = null;
        //    SQLiteCommand command = null;
        //    SQLiteDataReader reader = null;

        //    try
        //    {
        //        this.OpenConnection();

        //        transaction = this.connection.BeginTransaction();
        //        command = this.connection.CreateCommand();
        //        command.CommandText = query;
        //        reader = command.ExecuteReader();
        //    }
        //    catch (Exception e)
        //    {
        //        BaseCommand.WriteLine(e.Message);
        //    }
        //    finally
        //    {
        //        transaction?.Dispose();
        //        command?.Dispose();
        //    }

        //    return reader;
        //}

        public bool IsOpen()
        {
            return this.connection?.State == System.Data.ConnectionState.Open;
        }

        public void OpenConnection()
        {
            if (!this.IsOpen())
            {
                this.connection?.Open();
            }
        }

        public void CloseConnection()
        {
            if (this.IsOpen())
            {
                this.connection?.Close();
            }
        }
    }
}
