using Database.Contracts;
using System.IO;
using System.Data.SQLite;
using System;
using Database;
using Database.Enums;
using System.Threading.Tasks;

namespace Database
{
    public class Connection : IConnection
    {
        const string databaseSource = @"Data Source=database.sqlite";
        const string databaseLocation = "database.sqlite";
        private SQLiteConnection connection;
        public QueryResult<SaveStatus> Result { get; private set; }

        public Connection()
        {
            if (!File.Exists(databaseLocation))
            {
                SQLiteConnection.CreateFile(databaseLocation);
            }

            connection = new SQLiteConnection(databaseSource);
        }

        public string GetDatabasePath()
        {
            if (!this.IsOpen())
            {
                this.connection.Open();
            }

            string path = this.connection.FileName;
            this.connection.Close();

            return path;
        }

        public QueryResult<SaveStatus> RunSaveQuery(string query)
        {
            this.connection.Close();
            this.connection.Open();
            Result = new QueryResult<SaveStatus>();
            Result.SetStatus(SaveStatus.Pending);
            int numberAffectedRows = 0;
            try
            {
                using (SQLiteTransaction transaction = this.connection.BeginTransaction())
                {
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = query;
                    numberAffectedRows = command.ExecuteNonQuery();
                    transaction.Commit();
                    command.Dispose();
                }
            } catch (Exception ex)
            {
                Result.SetStatus(SaveStatus.Error);
                Result.SetMessage(ex.Message);
            }
            this.connection.Close();
            Result.SetStatus(SaveStatus.Success);

            InstanceContainer.Instance.ParamBag();
            Result.SetMessage("Number affected rows:" + numberAffectedRows);

            return Result;
        }

        public SQLiteDataReader RunQuery(string query)
        {
            this.connection.Close();
            this.connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = query;
            InstanceContainer.Instance.ParamBag();
            return command.ExecuteReader();
        }

        public bool IsOpen()
        {
            return this.connection.State == System.Data.ConnectionState.Open;
        }
    }
}
