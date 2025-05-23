using Database.Contracts.Clause;
using Database.QueryBuilder;
using System.Text;

namespace Database.Clauses
{
    public class InsertClause : BaseClause, IInsertClause
    {
        public new const string PREFIX = "@insert";

        protected record Insert(string Column, object Value) : Base(Column, Value);

        public void AddInsertClause(string key, object value)
        {
            Add(new Insert(key, value));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Insert insert)
            {

                if (insert.Value is RawSql)
                {
                    AddCondition(insert);
                    return;
                }

                string guid = Guid.NewGuid().ToString("N")[..16];
                this.Parameters.Add($"{PREFIX}{guid}", insert.Value.ToString());

                AddCondition(new Insert(insert.Column, $"{PREFIX}{guid}"));
            }
        }

        public override string Compile()
        {
            Insert[] inserts = this.GetConditions<Insert>();

            StringBuilder query = new(" (");

            CompileColumns(query, inserts); 

            query.Append(") VALUES (");

            CompileValues(query, inserts);

            query.Append(')');

            return query.ToString();
        }

        protected void CompileColumns(StringBuilder query, Insert[] inserts)
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

        protected void CompileValues(StringBuilder query, Insert[] inserts)
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
