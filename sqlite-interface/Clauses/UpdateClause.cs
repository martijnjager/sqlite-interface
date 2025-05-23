using Database.Contracts.Clause;
using System.Text;

namespace Database.Clauses
{
    public class UpdateClause : BaseClause, IUpdateClause
    {
        public new const string PREFIX = "@update";
        
        protected record Update(string Column, object Value) : Base(Column, Value);

        public void AddUpdateClause(string key, string value)
        {
            this.Add(new Update(key, value));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Update update)
            {
                string valueName = PREFIX + Guid.NewGuid().ToString("N")[..16];

                if (IsNull(update.Value.ToString()))
                {
                    this.Parameters.Add(valueName, "null");
                }
                else
                {
                    this.Parameters.Add(valueName, update.Value.ToString());
                }

                update = update with { Value = valueName };

                this.AddCondition(update);
            }
        }

        public override string Compile()
        {
            StringBuilder query = new(" SET ");

            Update[] updates = this.GetConditions<Update>();

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
