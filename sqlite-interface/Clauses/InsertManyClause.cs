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
    public class InsertManyClause : BaseClause, IInsertManyClause
    {
        public void AddInsertManyClause(List<IDictionary<string, string>> values)
        {
            this.Add(values);
        }

        protected override void Add<T>(T condition)
        {
            if (condition is List<IDictionary<string, string>> insert)
            {
                int line = 0;
                foreach (IDictionary<string, string> value in insert)
                {
                    int index = 0;
                    foreach (KeyValuePair<string, string> pair in value)
                    {
                        string name = $"@{pair.Key}_{line}_{index}";
                        this.Parameters.Add(name, pair.Value);
                        AddCondition(new Base(name, pair.Value));
                        index++;
                    }
                    line++;
                }
            }
        }

        public override string Compile()
        {
            Base[] inserts = this.GetConditions<Base>();

            // uniquely get the string before the first underscore
            string[] keys = inserts.Select(x => x.Column.Split('_')[0]).Distinct().ToArray();

            StringBuilder query = new(" (");

            foreach (string key in keys) {
                query.Append(key);
                query.Append(", ");
            }

            query.Remove(query.Length - 2, 2);
            query.Append(") VALUES (");

            int currentInsertIndex = 0;
            foreach (Base insert in inserts)
            {
                if (insert.Column.Contains($"_{currentInsertIndex}_"))
                {
                    query.Append(insert.Column);
                    query.Append(", ");
                }
                else
                {
                    query.Remove(query.Length - 2, 2);
                    query.Append("), (");
                    query.Append(insert.Column);
                    query.Append(", ");
                    currentInsertIndex++;
                }
            }

            query.Remove(query.Length - 2, 2);
            query.Append(')');

            return query.ToString();
        }
    }
}
