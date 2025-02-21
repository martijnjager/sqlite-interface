using Database.Clauses;
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
    public class SelectClause : BaseClause, ISelectClause
    {
        public void AddColumn(string column, string alias = "")
        {
            Add(new Base(column, alias));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Base select)
            {
                AddCondition(select);
            }
        }

        public override string Compile()
        {
            StringBuilder query = new("SELECT ");

            Base[] selects = this.GetConditions<Base>();

            if (selects.Length == 0)
            {
                query.Append('*');
            }
            else
            {
                for (int i = 0; i < selects.Length; i++)
                {
                    if (i > 0)
                    {
                        query.Append(", ");
                    }
                    query.Append(selects[i].Column);
                    if (!string.IsNullOrEmpty(selects[i].Value))
                    {
                        query.Append(" AS ").Append(selects[i].Value);
                    }
                }
            }
            return query.ToString();
        }

        public new void Bind(SQLiteCommand command)
        {
            // No binding required
        }
    }
}
