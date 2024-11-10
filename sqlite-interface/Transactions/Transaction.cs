using Database.Console;
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

        public Transaction(SQLiteCommand query)
        {
            BaseCommand.WriteLine("query: " + query.CommandText);
            this.Query = query;
            this.SetType(query.CommandText);
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
    }
}
