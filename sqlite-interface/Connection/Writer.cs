using Database.Clauses;
using Database.Contracts.Connection;
using Database.Enums;
using Database.Transactions;

namespace Database.Connection
{
    internal class Writer : BaseConnection, IWriter, IDisposable
    {
        private bool disposed = false;

        public Writer(string? location = null, string? source = null) : base(location, source)
        {
        }

        public QueryResult<SaveStatus> Save(Transactions.Transaction transaction)
        {
            QueryResult<SaveStatus> result = new QueryResult<SaveStatus>();

            try
            {
                result = this.RunQuery(transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                result.SetStatus(SaveStatus.Error);
                result.SetMessage(ex.Message);
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
