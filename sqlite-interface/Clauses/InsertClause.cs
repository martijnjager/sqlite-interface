using Database.Contracts;
using Database.Contracts.Clause;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    public class InsertClause : BaseClause, IInsertClause
    {
        public void AddInsertClause(string key, string value)
        {
            Add(new KeyValuePair<string, string>(key, value));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is KeyValuePair<string, string> insert)
            {
                int count = this.Parameters.Count;
                this.Parameters.Add($"@insert{count}", insert.Value);

                AddCondition(new KeyValuePair<string, string>(insert.Key, $"@insert{count}"));
            }
        }

        public override string Compile()
        {
            KeyValuePair<string, string>[] inserts = this.GetConditions<KeyValuePair<string, string>>();

            StringBuilder query = new(" (");

            CompileColumns(query, inserts);

            query.Append(") VALUES (");

            CompileValues(query, inserts);

            query.Append(')');

            return query.ToString();
        }

        protected void CompileColumns(StringBuilder query, KeyValuePair<string, string>[] inserts)
        {
            for (int i = 0; i < inserts.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append(inserts[i].Key);
            }
        }

        protected void CompileValues(StringBuilder query, KeyValuePair<string, string>[] inserts)
        {
            for (int i = 0; i < inserts.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append(inserts[i].Value);
            }
        }

        public void Bind(SQLiteCommand command)
        {
            KeyValuePair<string, string>[] inserts = this.GetConditions<KeyValuePair<string, string>>();

            for (int i = 0; i < inserts.Length; i++)
            {
                var parameter = command.CreateParameter();

                parameter.ParameterName = $"@insert{i}";
                this.Parameters.TryGetValue($"@insert{i}", out string? value);

                if (value is null || value.Equals("null"))
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = value;
                }
            }
        }
    }
}
