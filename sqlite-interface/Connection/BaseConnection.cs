using Database.Contracts;
using System.Data.Common;
using Database.Enums;
using Database.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Database.Connection
{
    public abstract class BaseConnection : IConnection
    {
        const string databaseSource = @"Data Source=database.sqlite";
        const string databaseLocation = "database.sqlite";
        const string testDatabaseSource = ":memory:";
        //private DbConnection? connection = null;
        private DbContext connection;

        public QueryResult<SaveStatus> Result { get; protected set; }

        public BaseConnection(string? location = null, string? source = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            string? defaultLocation = location ?? databaseLocation;
            string defaultSource = source ?? databaseSource;

            optionsBuilder.UseSqlite(defaultSource);
            this.connection = new DbContext(optionsBuilder.Options);

            if (!defaultSource.Contains(testDatabaseSource) && !File.Exists(defaultLocation))
            {
                this.connection.Database.EnsureCreated();
            }

            Result = new QueryResult<SaveStatus>();
        }

        public void SetConnection(string source, string? location = null)
        {
            string? defaultLocation = location;
            string defaultSource = source;

            if (!defaultSource.Contains(testDatabaseSource) && !File.Exists(defaultLocation))
            {
                this.connection.Database.EnsureCreated();
            }

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlite(defaultSource);
            this.connection.Dispose();
            this.connection = new DbContext(optionsBuilder.Options);
        }

        public DbConnection SqliteConnection()
        {
            return connection.Database.GetDbConnection() ?? throw new InvalidOperationException("Connection is not initialized.");
        }

        public string GetDatabasePath()
        {
            if (connection is null)
            {
                throw new InvalidOperationException("Connection is not initialized.");
            }

            OpenConnection();

            string path = connection.Database.GetDbConnection().DataSource;
            CloseConnection();

            return path;
        }

        public QueryResult<SaveStatus> RunSaveQuery(string query)
        {
            Result = new QueryResult<SaveStatus>();
            Result.SetStatus(SaveStatus.Pending);
            try
            {
                if (connection is null)
                {
                    throw new InvalidOperationException("Connection is not initialized.");
                }

                OpenConnection();
                var transaction = connection.Database.BeginTransaction();
                int numberAffectedRows = connection.Database.ExecuteSqlRaw(query);
                transaction.Commit();
                transaction.Dispose();
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
                CloseConnection();
            }

            return Result;
        }

        public QueryResult<SaveStatus> RunQuery(Transaction transaction)
        {
            DbDataReader reader = null;
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
            return connection.Database.GetDbConnection().State == System.Data.ConnectionState.Open;
        }

        public void OpenConnection()
        {
            if (!IsOpen())
            {
                connection.Database.OpenConnection();
            }
        }

        public void CloseConnection()
        {
            if (IsOpen())
            {
                connection.Database.CloseConnection();
            }
        }

        public async Task OpenConnectionAsync()
        {
            if (!this.IsOpen())
            {
                await this.connection.Database.OpenConnectionAsync();
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (this.IsOpen())
            {
                await this.connection.Database.CloseConnectionAsync();
            }
        }
    }
}
