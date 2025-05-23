using Database.Clauses;
using Database.Contracts;
using Database.Enums;
using Database.Contracts.Connection;
using Database.Connection;
using Database.Relations;
using System.Net.WebSockets;
using System.Data.Common;

namespace Database.Transactions
{
    /// <summary>
    /// Represents a manager for handling transactions in the database.
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        private Transaction? activeTransaction;

        //private readonly List<DbCommand> pendingQueries;

        //private readonly IConnection connection;

        //public bool IsTransactionInProgress => this.transaction is not null;

        private readonly IReader reader;

        private IWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        public TransactionManager()
        {
            this.reader = new Reader();
            //this.pendingQueries = new List<DbCommand>();
        }

        public void SetConnection(string location, string source)
        {
            this.reader?.SetConnection(source, location);
            this.writer?.SetConnection(source, location);
        }

        public static Transaction CreateTransaction(QueryBuilder.Builder queryBuilder, string query)
        {
            // dont use same transaction, database connection does not like it
            return Transaction.CreateTransaction(queryBuilder, query);
        }

        /// <summary>
        /// Runs a transaction to create/update/delete one or more model.
        /// </summary>
        /// <param name="model">The model to run the transaction on.</param>
        /// <param name="operation">The operation to perform on the model.</param>
        /// <returns>The result of the transaction.</returns>
        public static QueryResult<SaveStatus> Save(QueryBuilder.Builder queryBuilder)
        {
            if (Instance.writer is not null)
            {
                throw new Exception("A transaction is already in progress.");
            }

            if (queryBuilder.QueryType == Type.TYPE_SELECT)
            {
                throw new Exception("Cannot save a select query.");
            }

            Instance.writer = new Writer();

            var query = queryBuilder.Compile();
            var transaction = CreateTransaction(queryBuilder, query);

            QueryResult<SaveStatus> result = Instance.writer.Save(transaction);

            Instance.writer.Dispose();
            Instance.writer = null;
            transaction?.Close();
            transaction = null;

            return result;
        }

        /// <summary>
        /// Runs the transactions and returns a list of models.
        /// </summary>
        /// <typeparam name="T">The type of the models.</typeparam>
        /// <returns>A list of models.</returns>
        public Collection Read(QueryBuilder.Builder queryBuilder)
        {
            if (queryBuilder.QueryType != Type.TYPE_SELECT)
            {
                throw new Exception("Transaction must be a select query with this function.");
            }

            var query = queryBuilder.Compile();
            var transaction = CreateTransaction(queryBuilder, query);

            Collection models = this.reader.Read(transaction);
            transaction?.Close();
            transaction = null;

            return models;
        }

        /// <summary>
        /// Gets the connection used by the manager.
        /// </summary>
        /// <returns>The connection used by the manager.</returns>
        public IConnection GetConnection() => Instance.reader;

        protected static TransactionManager Instance => App.Instance.ConnectionManager();
    }
}
