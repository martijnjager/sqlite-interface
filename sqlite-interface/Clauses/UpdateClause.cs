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
            this.Add(new KeyValuePair<string, string>(key, value));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is KeyValuePair<string, string> update)
            {
                string valueName = "@update" + (this.Parameters.Count).ToString();

                if (update.Value is null || update.Value.Equals("null"))
                {
                    this.Parameters.Add(valueName, "null");
                }
                else
                {
                    this.Parameters.Add(valueName, update.Value);
                }

                update = new KeyValuePair<string, string>(update.Key, valueName);

                this.AddCondition(update);
            }
        }

        public override string Compile()
        {
            StringBuilder query = new(" SET ");

            KeyValuePair<string, string>[] updates = this.GetConditions<KeyValuePair<string, string>>();

            for (int i = 0; i < updates.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(", ");
                }

                query.Append(updates[i].Key)
                    .Append(" = '")
                    .Append(updates[i].Value)
                    .Append('\'');
            }

            return query.ToString();
        }

        public void Bind(SQLiteCommand command)
        {
            KeyValuePair<string, string>[] updates = this.GetConditions<KeyValuePair<string, string>>();

            for (int i = 0; i < updates.Length; i++)
            {
                this.Parameters.TryGetValue(updates[i].Value, out string? value);
                var parameter = command.CreateParameter();
                parameter.ParameterName = updates[i].Value;

                if (value is null || value == "null")
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = value;
                }

                command.Parameters.Add(parameter);
            }
        }
    }
}
