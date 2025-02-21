using Database.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Clauses
{
    public abstract class BaseClause
    {
        protected record Base(string Column, string Value);

        protected List<object> Conditions { get; private set; } = new List<object>();

        protected IDictionary<string, string> Parameters { get; private set; } = new Dictionary<string, string>();

        protected void AddCondition(object clause)
        {
            Conditions ??= new List<object>();

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

        public void Bind(SQLiteCommand command)
        {
            foreach (KeyValuePair<string, string> parameter in this.Parameters)
            {
                SQLiteParameter queryParameter = command.CreateParameter();
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
            string valueName = type + (this.Parameters.Count + 1).ToString();
            string value = isNullValue ? "null" : condition.Value;
            this.Parameters.Add(valueName, value);
            return valueName;
        }

        protected string HandleInOperator(Base condition, string type)
        {
            string[] options = condition.Value.Split(',');

            int offset = options.Length + this.Parameters.Count;

            string newValue = "(" + string.Join(",", options.Select(option => type + (offset--).ToString())) + ")";

            offset = options.Length + this.Parameters.Count;

            foreach (string option in options)
            {
                this.Parameters.Add(type + (offset--).ToString(), option);
            }

            return newValue;
        }
    }
}
