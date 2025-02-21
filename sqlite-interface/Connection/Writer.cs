using Database.Clauses;
using Database.Contracts.Connection;
using Database.Enums;
using Database.Transactions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Connection
{
    internal class Writer : BaseConnection, IWriter, IDisposable
    {
        private bool disposed = false;

        public Writer(string? location = null, string? source = null) : base(location, source)
        {
        }

        public QueryResult<SaveStatus> Save(ClauseManager clauseManager)
        {
            QueryResult<SaveStatus> result = new QueryResult<SaveStatus>();
            Transaction? transaction = null;

            try
            {
                // Opens the connection, begins the transaction and
                // binds parameters into query
                transaction = clauseManager.Compile();
                result = this.RunQuery(transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                result.SetStatus(SaveStatus.Error);
                result.SetMessage(ex.Message);
            }
            finally
            {
                transaction?.Close();
            }

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    this.CloseConnection();
                }
                // Dispose unmanaged resources
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Writer()
        {
            Dispose(false);
        }
    }
}
