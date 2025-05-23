using Database.Contracts.Clause;
using System.Data.Common;
using System.Text;

namespace Database.Clauses
{
    public class SelectClause : BaseClause, ISelectClause
    {
        protected record Select(string? Column, object? Value, SubQuery? Subquery = null): Base(Column, Value, Subquery);

        public void AddColumn(string column, string alias = "")
        {
            Add(new Select(column, alias));
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Select select)
            {
                AddCondition(select);
            }
        }

        public override string Compile()
        {
            StringBuilder query = new("SELECT ");

            Select[] selects = this.GetConditions<Select>();

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

                    if (selects[i].Subquery is SubQuery)
                    {
                        var subquery = this.CompileSubQuery(selects[i]);

                        query.Append(subquery).Append(' ');
                    } else
                    {
                        query.Append(selects[i].Column);
                        if (!string.IsNullOrEmpty(selects[i].Value as string))
                        {
                            query.Append(" AS ").Append(selects[i].Value);
                        }
                    }
                }
            }
            return query.ToString();
        }

        public new void Bind(DbCommand command)
        {
            // No binding required
        }
    }
}
