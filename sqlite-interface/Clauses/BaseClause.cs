using Database.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    public abstract class BaseClause
    {
        public const string PREFIX = "";

        protected record Base(string? Column = null, object? Value = null, SubQuery? Subquery = null);

        protected record SubQuery(
            Eloquent query,
            string Type
        );

        protected struct SubQueryType
        {
            public const string Exists = "exists";
            public const string NotExists = "not exists";
            public const string In = "in";
            public const string NotIn = "not in";
        };

        protected List<object> Conditions { get; private set; } = [];

        public IDictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();

        public bool HasSubquery()
        {
            return Conditions.Any(condition => (condition as Base).Subquery is not null);
        }

        public List<object> GetConditions()
        {
            return Conditions;
        }

        protected string CompileSubQuery(Base condition)
        {
            var subquery = condition.Subquery.query.queryBuilder.Compile();
            string sql = subquery.TrimEnd(';');

            foreach (var item in condition.Subquery.query.queryBuilder.GetParameters())
            {
                this.Parameters.Add(item.Key, item.Value);
            }

            return condition.Subquery.Type switch
            {
                SubQueryType.Exists => $" EXISTS ({sql})",
                SubQueryType.NotExists => $" NOT EXISTS ({sql})",
                SubQueryType.In => $"{condition.Column} IN ({sql})",
                SubQueryType.NotIn => $"{condition.Column} NOT IN ({sql})",
                _ => $"{condition.Column} ({sql})"
            };
        }

        protected void AddCondition(object clause)
        {
            Conditions ??= [];

            Conditions.Add(clause);
        }

        public abstract string Compile();

        protected abstract void Add<T>(T condition);

        public void Clear()
        {
            Conditions.Clear();
        }

        public T[] GetConditions<T>()
        {
            return Conditions.Where(condition => condition is T).Cast<T>().ToArray();
        }

        public bool HasConditions()
        {
            return Conditions.Count > 0;
        }

        public void Bind(DbCommand command)
        {
            foreach (KeyValuePair<string, string> parameter in this.Parameters)
            {
                DbParameter queryParameter = command.CreateParameter();
                queryParameter.ParameterName = parameter.Key;
                queryParameter.Value = DBNull.Value;

                if (!IsNull(parameter.Value))
                {
                    queryParameter.Value = parameter.Value;
                }

                command.Parameters.Add(queryParameter);
            }
        }

        protected static bool IsNull(string? value)
        {
            return value is null || value.Equals("null");
        }

        protected string HandleValue(Base condition, string type, bool isNullValue = false)
        {
            string valueName = type + Guid.NewGuid().ToString("N")[..16];
            string value = isNullValue ? "null" : condition.Value as string;
            this.Parameters.Add(valueName, value);
            return valueName;
        }

        protected string HandleInOperator(Base condition, string type)
        {
            string[] options = condition.Value.ToString().Split(',');

            StringBuilder builder = new("(");

            foreach (string option in options)
            {
                string guid = type + Guid.NewGuid().ToString("N")[..16];
                builder.Append(guid + ",");
                this.Parameters.Add(guid, option);
            }

            builder.Remove(builder.Length - 1, 1).Append(")");

            return builder.ToString();
        }
    }
}
