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
            Add(new Base(key, value));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Base insert)
            {
                int count = this.Parameters.Count;
                this.Parameters.Add($"@insert{count}", insert.Value);

                AddCondition(new Base(insert.Column, $"@insert{count}"));
            }
        }

        public override string Compile()
        {
            Base[] inserts = this.GetConditions<Base>();

            StringBuilder query = new(" (");

            CompileColumns(query, inserts);

            query.Append(") VALUES (");

            CompileValues(query, inserts);

            query.Append(')');

            return query.ToString();
        }

        protected void CompileColumns(StringBuilder query, Base[] inserts)
        {
            for (int i = 0; i < inserts.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append(inserts[i].Column);
            }
        }

        protected void CompileValues(StringBuilder query, Base[] inserts)
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
    }
}
