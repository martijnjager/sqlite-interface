using Database.Contracts.Clause;
using Database.QueryBuilder;
using System.Text;

namespace Database.Clauses
{
    public class HavingClause : BaseClause, IHavingClause
    {
        public new const string PREFIX = "@having";
        protected record Having (string? Column, string? Operator, object? Value, string? Boolean, SubQuery? Subquery = null) : Base(Column, Value, Subquery);

        protected override void Add<T>(T condition)
        {
            if (condition is Having having)
            {
                string[] exemptions = new[] { "(", ")" };

                if (having.Value is RawSql)
                {
                    AddCondition(having);
                    return;
                }

                if (!exemptions.Contains(having.Value))
                {
                    if (IsNull(having.Value.ToString()))
                    {
                        string newValue = HandleValue(having, PREFIX, true);
                        having = having with { Value = newValue };
                    }
                    else
                    {
                        if (having.Operator.Equals(Operator.In))
                        {
                            string newValue = HandleInOperator(having, PREFIX);
                            having = having with { Value = newValue };
                        }
                        else
                        {
                            string newValue = HandleValue(having, PREFIX, true);
                            having = having with { Value = newValue };
                        }
                    }
                }

                AddCondition(having);
            }
        }

        public void AddHavingClause(string key, string op, object value)
        {
            AddCondition(new Having(key, op, value, "and"));
        }

        public void AddOrHavingClause(string key, string op, object value)
        {
            AddCondition(new Having(key, op, value, "or"));
        }

        public void AddNestedHavingClause(Action<Eloquent> callback, Eloquent model)
        {
            AddCondition(new Having("", "", "(", ""));
            callback(model);
            AddCondition(new Having("", "", ")", ""));
        }

        public void AddHasClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            AddCondition(new Having(relation, Operator.Exists, "(", "("));
            callback(model);
            AddCondition(new Having("", "", ")", ""));
        }

        public void AddHasNotClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            AddCondition(new Having(relation, Operator.NotExists, "(", "("));
            callback(model);
            AddCondition(new Having("", "", ")", ""));
        }

        public void AddHavingInClause(string key, params object[] values)
        {
            AddCondition(new Having(key, Operator.In, "(" + string.Join(", ", values) + ")", "and"));
        }

        public override string Compile()
        {
            StringBuilder query = new("");
            Having[] havings = this.GetConditions<Having>();

            for (int i = 0; i < havings.Length; i++)
            {
                if (i > 0)
                {
                    query.Append(havings[i].Boolean).Append(' ');
                }

                if (havings[i].Subquery is SubQuery)
                {
                    var subquery = this.CompileSubQuery(havings[i]);

                    query.Append(subquery).Append(' ');
                } else
                {
                    query.Append(havings[i].Column).Append(' ')
                        .Append(havings[i].Operator).Append(' ')
                        .Append($"{PREFIX}{i}").Append(' ');
                }
            }

            return query.ToString();
        }
    }
}
