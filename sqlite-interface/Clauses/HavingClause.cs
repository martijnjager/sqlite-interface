using Database.Contracts;
using Database.Contracts.Clause;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    public class HavingClause : BaseClause, IHavingClause
    {
        protected record Having (string Column, string Operator, string Value, string Boolean) : Base(Column, Value);

        protected override void Add<T>(T condition)
        {
            if (condition is Having having)
            {
                string[] exemptions = new[] { "(", ")" };

                if (!exemptions.Contains(having.Value))
                {
                    if (IsNull(having.Value))
                    {
                        string newValue = HandleValue(having, "@having", true);
                        having = having with { Value = newValue };
                    }
                    else
                    {
                        if (having.Operator.Equals(Operator.In))
                        {
                            string newValue = HandleInOperator(having, "@having");
                            having = having with { Value = newValue };
                        }
                        else
                        {
                            string newValue = HandleValue(having, "@having", true);
                            having = having with { Value = newValue };
                        }
                    }
                }

                AddCondition(having);
            }
        }

        public void AddHavingClause(string key, string op, string value)
        {
            AddCondition(new Having(key, op, value, "and"));
        }

        public void AddOrHavingClause(string key, string op, string value)
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

                query.Append(havings[i].Column).Append(' ')
                    .Append(havings[i].Operator).Append(' ')
                    .Append($"@having{i}").Append(' ');
            }

            return query.ToString();
        }
    }
}
