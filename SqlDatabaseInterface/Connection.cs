using Database.Contracts;
using System.IO;
using System.Data.SQLite;
using System.Data;

namespace Database
{
    public class Connection : IConnection
    {
        const string databaseSource = @"Data Source=database.sqlite";
        const string databaseLocation = "database.sqlite";
        private SQLiteConnection connection;

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
            this.connection.Open();
            string path = this.connection.FileName;
            this.connection.Close();

            return path;
        }

        public int RunNonQuery(string query)
        {
            this.connection.Close();
            this.connection.Open();
            int numberAffectedRows = 0;
            using (SQLiteTransaction transaction = this.connection.BeginTransaction())
            {
                SQLiteCommand command = connection.CreateCommand();
                command.CommandText = query;
                numberAffectedRows = command.ExecuteNonQuery();
                transaction.Commit();
            }

            this.connection.Close();

            InstanceContainer.Get("paramBag").Clear();

            return numberAffectedRows;
        }

        public SQLiteDataReader RunQuery(string query)
        {
            this.connection.Close();
            this.connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = query;
            InstanceContainer.Get("paramBag").Clear();
            return command.ExecuteReader();
        }
    }
}
