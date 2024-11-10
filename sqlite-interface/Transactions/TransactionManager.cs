using Database.Clauses;
using Database.Contracts;
using Database.Enums;
using System.Data.SQLite;
using System.Text;
using System.Data;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System.Collections.ObjectModel;

namespace Database.Transactions
{
    /// <summary>
    /// Represents a manager for handling transactions in the database.
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private Transaction? transaction;

        private readonly IConnection connection;

        public bool IsTransactionInProgress => this.transaction is not null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        public TransactionManager()
        {
            this.connection = new Connection();
        }

        public void SetConnection(string location, string source)
        {
            this.connection?.SetConnection(source, location);
        }

        /// <summary>
        /// Runs a transaction to create/update/delete one or more model.
        /// </summary>
        /// <param name="model">The model to run the transaction on.</param>
        /// <param name="operation">The operation to perform on the model.</param>
        /// <returns>The result of the transaction.</returns>
        public static QueryResult<SaveStatus> Run(ClauseManager clauseManager)
        {
            if (Instance.IsTransactionInProgress)
            {
                throw new Exception("A transaction is already in progress.");
            }

            Instance.connection.OpenConnection();
            var sqliteTransaction = Instance.connection.SqliteConnection().BeginTransaction();

            Instance.transaction = clauseManager.Compile();

            if (Instance.transaction.Type == Type.TYPE_SELECT)
            {
                throw new Exception("Cannot run a select query with this function.");
            }

            QueryResult<SaveStatus> result = Instance.connection.RunQuery(Instance.transaction);

            if (result.Status == SaveStatus.Error)
            {
                throw new Exception(result.Message);
            }

            sqliteTransaction.Commit();
            sqliteTransaction.Dispose();
            Instance.transaction.Query.Dispose();
            Instance.connection.CloseConnection();

            return result;
        }

        /// <summary>
        /// Runs the transactions and returns a list of models.
        /// </summary>
        /// <typeparam name="T">The type of the models.</typeparam>
        /// <returns>A list of models.</returns>
        public List<IModel> Run<T>(ClauseManager clauseManager) where T : IModel
        {
            if (Instance.IsTransactionInProgress)
            {
                throw new Exception("A transaction is already in progress.");
            }

            Instance.transaction = clauseManager.Compile();

            Instance.connection.OpenConnection();
            var sqliteTransaction = Instance.connection.SqliteConnection().BeginTransaction();

            if (Instance.transaction.Type != Type.TYPE_SELECT)
            {
                throw new Exception("Transaction must be a select query with this function.");
            }

            QueryResult<SaveStatus> result = Instance.connection.RunQuery(Instance.transaction);

            if (result.Reader is null)
            {
                return new List<IModel>();
            }

            List<IModel> models = Process<T>(result.Reader);

            Instance.transaction = null;

            return models;
        }

        /// <summary>
        /// Gets the connection used by the manager.
        /// </summary>
        /// <returns>The connection used by the manager.</returns>
        public IConnection GetConnection() => Instance.connection;

        protected static TransactionManager Instance => InstanceContainer.Instance.ConnectionManager();

        /// <summary>
        /// Processes the query and returns a list of models.
        /// </summary>
        /// <typeparam name="T">The type of the models.</typeparam>
        /// <param name="query">The query to process.</param>
        /// <returns>A list of models.</returns>
        private static List<IModel> Process<T>(SQLiteDataReader reader) where T : IModel
        {
            List<IModel> items = new();

            if (reader is null || !reader.HasRows)
            {
                return items;
            }

            var defaultColumns = reader.GetColumnSchema();
            string table = reader.GetTableName(0);

            while (reader.Read())
            {
                IModel? mainModel = CreateFromReader<T>(reader, table, defaultColumns);

                items.Add(mainModel);
            }

            return items;
        }

        private static IModel? CreateFromReader<T>(SQLiteDataReader reader, string table, ReadOnlyCollection<DbColumn> defaultColumns) where T : IModel
        {
            ParamBag paramBag = new();
            IModel? mainModel = InstanceContainer.ModelByKey(table, defaultColumns);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string key = reader.GetName(i);
                string? value = reader[i].ToString();

                paramBag.Add(key, value);
            }

            mainModel?.Assign(paramBag);

            return mainModel;
        }
    }
}
