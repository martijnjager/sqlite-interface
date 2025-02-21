using Database.Console;
using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Transactions
{
    public class Transaction
    {
        public Type Type { get; private set; }

        public SQLiteCommand Query { get; private set; }

        private SQLiteConnection Connection;

        private SQLiteTransaction _transaction;

        public Transaction(SQLiteConnection connection, SQLiteCommand query, SQLiteTransaction transaction)
        {
            BaseCommand.WriteLine("query: " + query.CommandText);
            this.Query = query;
            this.SetType(query.CommandText);
            this.Connection = connection;
            this._transaction = transaction;
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
        }

        public void Commit()
        {
            this._transaction.Commit();
        }

        public void Rollback()
        {
            this._transaction.Rollback();
        }
    }
}
