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
    public class UpdateClause : BaseClause, IUpdateClause
    {
        public void AddUpdateClause(string key, string value)
        {
            this.Add(new Base(key, value));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Base update)
            {
                string valueName = "@update" + (this.Parameters.Count + 1).ToString();

                if (IsNull(update.Value))
                {
                    this.Parameters.Add(valueName, "null");
                }
                else
                {
                    this.Parameters.Add(valueName, update.Value);
                }

                update = update with { Value = valueName };

                this.AddCondition(update);
            }
        }

        public override string Compile()
        {
            StringBuilder query = new(" SET ");

            Base[] updates = this.GetConditions<Base>();

            for (int i = 0; i < updates.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append(updates[i].Column)
                    .Append(" = ")
                    .Append(updates[i].Value);
            }

            return query.ToString();
        }
    }
}
