using Database.Clauses;
using Database.Contracts;
using Database.Enums;
using System.Data.SQLite;
using System.Text;
using System.Data;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System.Collections.ObjectModel;
using Database.Contracts.Connection;
using Database.Connection;
using Database.Events;
using Database.Relations;

namespace Database.Transactions
{
    /// <summary>
    /// Represents a manager for handling transactions in the database.
    /// </summary>
    public class TransactionManager : ITransactionManager
    {
        //private Transaction? transaction;

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
        }

        public void SetConnection(string location, string source)
        {
            this.reader?.SetConnection(source, location);
            this.writer?.SetConnection(source, location);
        }

        /// <summary>
        /// Runs a transaction to create/update/delete one or more model.
        /// </summary>
        /// <param name="model">The model to run the transaction on.</param>
        /// <param name="operation">The operation to perform on the model.</param>
        /// <returns>The result of the transaction.</returns>
        public static QueryResult<SaveStatus> Save(ClauseManager clauseManager)
        {
            if (Instance.writer is not null)
            {
                throw new Exception("A transaction is already in progress.");
            }

            if (clauseManager.QueryType == Type.TYPE_SELECT)
            {
                throw new Exception("Cannot save a select query.");
            }

            Instance.writer = new Writer();

            QueryResult<SaveStatus> result = Instance.writer.Save(clauseManager);

            Instance.writer.Dispose();
            Instance.writer = null;

            return result;
        }

        /// <summary>
        /// Runs the transactions and returns a list of models.
        /// </summary>
        /// <typeparam name="T">The type of the models.</typeparam>
        /// <returns>A list of models.</returns>
        public List<IModel> Read<T>(ClauseManager clauseManager) where T : IModel
        {
            if (clauseManager.QueryType != Type.TYPE_SELECT)
            {
                throw new Exception("Transaction must be a select query with this function.");
            }

            List<IModel> models = this.reader.Read<T>(clauseManager);

            if (clauseManager.Relations is not null)
            {
                if (clauseManager.Relations.Count > 0)
                {
                    RelationManager.LoadRelations(clauseManager.Relations, models);
                }
            }

            return models;
        }

        /// <summary>
        /// Gets the connection used by the manager.
        /// </summary>
        /// <returns>The connection used by the manager.</returns>
        public IConnection GetConnection() => Instance.reader;

        protected static TransactionManager Instance => InstanceContainer.Instance.ConnectionManager();
    }
}
