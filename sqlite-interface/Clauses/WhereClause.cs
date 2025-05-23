using Database.Contracts.Clause;
using Database.Contracts.Relation;
using Database.QueryBuilder;
using System.Text;

namespace Database.Clauses
{
    public class WhereClause : BaseClause, IWhereClause
    {
        public new const string PREFIX = "@where";

        protected record Where(string? Column = null, string? Operator = null, object? Value = null, string? Boolean = null, SubQuery? Subquery = null) : Base(Column, Value, Subquery);

        protected override void Add<T>(T condition)
        {
            if (condition is Where whereCondition)
            {
                string[] exemptions = new[] { "(", ")" };

                if (whereCondition.Value is RawSql)
                {
                    AddCondition(whereCondition);
                    return;
                }

                if (!exemptions.Contains(whereCondition.Value))
                {
                    if (IsNull(whereCondition.Value.ToString()))
                    {
                        string newValue = HandleValue(whereCondition, PREFIX, true);
                        whereCondition = whereCondition with { Value = newValue };
                    }
                    else
                    {
                        if (whereCondition.Operator.Equals(Operator.In))
                        {
                            string newValue = HandleInOperator(whereCondition, PREFIX);
                            whereCondition = whereCondition with { Value = newValue };
                        }
                        else
                        {
                            string newValue = HandleValue(whereCondition, PREFIX, false);
                            whereCondition = whereCondition with { Value = newValue };
                        }
                    }
                }

                AddCondition(whereCondition);
            }
        }

        public void AddWhereClause(string key, string op, object value)
        {
            Add(new Where(key, op, value, "and"));
        }

        public void AddOrWhereClause(string key, string op, object value)
        {
            Add(new Where(key, op, value, "or"));
        }

        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where("", "", "(", ""));
            callback(model);
            Add(new Where("", "", ")", "and"));
        }

        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model, IJoin join)
        {
            callback(model);
            var subqueryClause = new SubQuery(model, SubQueryType.Exists);
            AddCondition(new Where(relation, "", "", "and", subqueryClause));
        }

        public void AddWhereHasClause(string relation, Eloquent model, IJoin join)
        {
            var subqueryClause = new SubQuery(model, SubQueryType.Exists);
            AddCondition(new Where(relation, "", "", "and", subqueryClause));
        }

        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model, IJoin join)
        {
            callback(model);
            var subqueryClause = new SubQuery(model, SubQueryType.NotExists);
            AddCondition(new Where(relation, "", "", "and", subqueryClause));
        }

        public void AddWhereHasNotClause(string relation, Eloquent model, IJoin join)
        {
            var subqueryClause = new SubQuery(model, SubQueryType.NotExists);
            AddCondition(new Where(relation, "", "", "and", subqueryClause));
        }

        public void AddWhereInClause(string key, params object[] values)
        {
            StringBuilder stringBuilder = new("");
            if (values.GetType().Name.Contains("[]"))
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

                if (wheres[i].Subquery is SubQuery)
                {
                    var subquery = this.CompileSubQuery(wheres[i]);

                    query.Append(subquery).Append(' ');
                } else
                {
                    query.Append(wheres[i].Column).Append(' ')
                   .Append(wheres[i].Operator).Append(' ');

                    if (wheres[i].Value is RawSql raw)
                    {
                        query.Append(raw.Value).Append(' ');
                    } else
                    {
                        query.Append(wheres[i].Value).Append(' ');
                    }
                }
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
