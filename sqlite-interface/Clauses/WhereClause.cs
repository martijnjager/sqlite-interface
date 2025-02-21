using Database.Contracts.Clause;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    public class WhereClause : BaseClause, IWhereClause
    {
        protected record Where(string Column, string Operator, string Value, string Boolean) : Base(Column, Value);

        protected override void Add<T>(T condition)
        {
            if (condition is Where whereCondition)
            {
                string[] exemptions = new[] { "(", ")" };

                if (!exemptions.Contains(whereCondition.Value))
                {
                    if (IsNull(whereCondition.Value))
                    {
                        string newValue = HandleValue(whereCondition, "@where", true);
                        whereCondition = whereCondition with { Value = newValue };
                    }
                    else
                    {
                        if (whereCondition.Operator.Equals(Operator.In))
                        {
                            string newValue = HandleInOperator(whereCondition, "@where");
                            whereCondition = whereCondition with { Value = newValue };
                        }
                        else
                        {
                            string newValue = HandleValue(whereCondition, "@where", false);
                            whereCondition = whereCondition with { Value = newValue };
                        }
                    }
                }

                AddCondition(whereCondition);
            }
        }

        public void AddWhereClause(string key, string op, string value)
        {
            Add(new Where(key, op, value, "and"));
        }

        public void AddOrWhereClause(string key, string op, string value)
        {
            Add(new Where(key, op, value, "or"));
        }

        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where("", "", "(", ""));
            callback(model);
            Add(new Where("", "", ")", "and"));
        }

        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where(relation, Operator.Exists, "(", ""));
            callback(model);
            Add(new Where("", "", ")", "and"));
        }

        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where(relation, Operator.NotExists, "(", ""));
            callback(model);
            Add(new Where("", "", ")", "and"));
        }

        public void AddWhereInClause(string key, params object[] values)
        {
            StringBuilder stringBuilder = new("");
            if (values.GetType().Name == "Object[]")
            {
                if (values[0] is List<string> ||
                    values[0] is List<int> ||
                    values[0] is List<bool> ||
                    values[0] is List<float> ||
                    values[0] is List<double>)
                {
                    if (values[0] is IEnumerable<object> enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            stringBuilder.Append(item.ToString()).Append(',');
                        }
                    }
                }
                else if (values is IEnumerable<object> enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        stringBuilder.Append(item.ToString()).Append(',');
                    }
                } else
                {
                    stringBuilder.Append(string.Join(",", values));
                }
            }
            else
            {
                stringBuilder.Append(string.Join(",", values.ToString()));
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            string value = stringBuilder.ToString();

            Add(new Where(key, Operator.In, value, "and"));
        }

        public override string Compile()
        {
            StringBuilder query = new("");
            Where[] wheres = this.GetConditions<Where>();

            for (int i = 0; i < wheres.Length; i++)
            {                
                if (i > 0)
                {
                    query.Append(wheres[i].Boolean).Append(' ');
                }

                query.Append(wheres[i].Column).Append(' ')
                    .Append(wheres[i].Operator).Append(' ')
                    .Append(wheres[i].Value).Append(' ');
            }

            query.Remove(query.Length - 1, 1);

            return query.ToString();
        }

        public bool HasWhereColumn(string column)
        {
            return Conditions.Where(Where => Where is Where)
                .Cast<Where>()
                .Any(clause => clause.Column == column);
        }
    }
}
