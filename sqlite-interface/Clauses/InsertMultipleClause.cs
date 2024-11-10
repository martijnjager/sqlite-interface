using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    public class InsertMultipleClause : BaseClause, IClause
    {
        public void AddInsertClause(IDictionary<string, string> values)
        {
            this.AddCondition(values);
        }

        protected override void Add<T>(T condition)
        {
            throw new NotImplementedException();
        }

        public override string Compile()
        {
            IDictionary<string, string>[] inserts = this.GetConditions<IDictionary<string, string>>();

            StringBuilder query = new(" (");

            for (int i = 0; i < inserts.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append('(');

                KeyValuePair<string, string>[] values = inserts[i].ToArray();

                for (int j = 0; j < values.Length; j++)
                {
                    if (j > 0)
                    {
                        query.Append(", ");
                    }

                    query.Append(values[j].Key);
                }

                query.Append(')');
            }

            query.Append(") VALUES (");

            for (int i = 0; i < inserts.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append('(');

                KeyValuePair<string, string>[] values = inserts[i].ToArray();

                for (int j = 0; j < values.Length; j++)
                {
                    if (j > 0)
                    {
                        query.Append(", ");
                    }

                    query.Append($"@insert{i}{j}");
                }

                query.Append(')');
            }

            query.Append(')');

            return query.ToString();
        }

        public void Bind(SQLiteCommand command)
        {
            IDictionary<string, string>[] inserts = this.GetConditions<IDictionary<string, string>>();

            for (int i = 0; i < inserts.Length; i++)
            {
                KeyValuePair<string, string>[] values = inserts[i].ToArray();

                for (int j = 0; j < values.Length; j++)
                {
                    command.Parameters.AddWithValue($"@insert{i}{j}", values[j].Value);
                }
            }
        }
    }
}
