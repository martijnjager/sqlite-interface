using Database.Console;
using Database.QueryBuilder;
using Database.Relations;
using System.Data.Common;

namespace Database.Transactions
{
    public class Transaction
    {
        public Type Type { get; private set; }

        public DbCommand Query { get; private set; }

        public DbConnection Connection { get; private set; }

        private readonly DbTransaction _transaction;

        public IDictionary<string, object> Metatada { get; private set; } = new Dictionary<string, object>();

        public Transaction(DbConnection connection, DbCommand query, DbTransaction transaction)
        {
            this.Query = query;
            this.SetType(query.CommandText);
            this.Connection = connection;
            this._transaction = transaction;
        }

        public void SetRelationsToLoad(List<ToLoad> relations)
        {
            if (relations.Count > 0)
            {
                Metatada["relations"] = relations;
            }
        }

        public List<ToLoad> GetRelationsToLoad()
        {
            return Metatada.TryGetValue("relations", out object? relations) && relations is List<ToLoad> relationList
                ? relationList as List<ToLoad>
                : new List<ToLoad>();
        }

        public void SetMetadata(string key, object value)
        {
            Metatada[key] = value;
        }

        public T? GetMetadata<T>(string key, T? defaultValue = default)
        {
            return Metatada.TryGetValue(key, out object? value) && value is T typedValue ? typedValue : defaultValue;
        }

        public object? GetMetadata(string key)
        {
            return Metatada.TryGetValue(key, out object? value) ? value : null;
        }

        public bool HasMetadata(string key)
        {
            return Metatada.ContainsKey(key);
        }

        private void SetType(string query)
        {
            query = query.ToLower();

            string firstWord = query.Split(' ')[0];

            this.Type = firstWord switch
            {
                "insert" => Type.TYPE_INSERT,
                "update" => Type.TYPE_UPDATE,
                "delete" => Type.TYPE_DELETE,
                _ => Type.TYPE_SELECT,
            };
        }

        public void Close()
        {
            this.Query.Dispose();
            this._transaction.Dispose();
            this.Connection.Close();
            this.Metatada.Clear();
        }

        public void Commit()
        {
            this._transaction.Commit();
        }

        public void Rollback()
        {
            this._transaction.Rollback();
        }

        public void SetQuery(QueryBuilder.Builder builder, string query)
        {
            this.Query.CommandText = query;
            BaseCommand.WriteLine("query: " + query);
            this.Query.CommandType = System.Data.CommandType.Text;
            ParameterBinder.Bind(builder.clauses, this.Query);
        }

        public static Transaction CreateTransaction(QueryBuilder.Builder builder, string query)
        {
            var conn = App.Instance.ConnectionManager().GetConnection().SqliteConnection();

            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            var transaction = conn.BeginTransaction();

            DbCommand command = conn.CreateCommand();
            //ParameterBinder.Bind(builder.clauses, command);

            Transaction queryTransaction = new(conn, command, transaction);
            queryTransaction.SetRelationsToLoad(builder.Relations);
            queryTransaction.SetQuery(builder, query);

            return queryTransaction;
        }
    }
}
