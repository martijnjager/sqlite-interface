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
    public class HavingClause : BaseClause, IHavingClause
    {
        protected struct Having
        {
            public Having() 
            {
                Column = string.Empty;
                Operator = string.Empty;
                Value = string.Empty;
                Boolean = string.Empty;
            }

            public string Column { get; set; }
            public string Operator { get; set; }

            public string Value { get; set; }

            public string Boolean { get; set; }
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Having having)
            {
                string[] exemptions = new[] { "(", ")" };

                if (!exemptions.Contains(having.Value))
                {
                    if (having.Value is null || having.Value.Equals("null"))
                    {
                        string valueName = "@having" + (this.Parameters.Count).ToString();
                        this.Parameters.Add(valueName, "null");
                        having.Value = valueName;
                    }
                    else
                    {
                        if (having.Operator.Equals("in"))
                        {
                            string[] options = having.Value.Split(',');

                            int offset = this.Parameters.Count - options.Length;

                            having.Value = string.Join(",", options.Select(option => "@having" + (offset++).ToString()));

                            offset = this.Parameters.Count - options.Length;

                            foreach (string option in options)
                            {
                                this.Parameters.Add("having" + (offset++).ToString(), option);
                            }
                        }
                        else
                        {
                            string valueName = "@having" + (this.Parameters.Count).ToString();
                            this.Parameters.Add(valueName, having.Value);
                            having.Value = valueName;
                        }
                    }
                }

                AddCondition(having);
            }
        }

        public void AddHavingClause(string key, string op, string value)
        {
            AddCondition(new Having { Column = key, Operator = op, Value = value, Boolean = "and" });
        }

        public void AddOrHavingClause(string key, string op, string value)
        {
            AddCondition(new Having { Column = key, Operator = op, Value = value, Boolean = "or" });
        }

        public void AddNestedHavingClause(Action<Eloquent> callback, Eloquent model)
        {
            AddCondition(new Having { Column = "", Operator = "", Value = "(" });
            callback(model);
            AddCondition(new Having { Column = "", Operator = "", Value = ")", Boolean = "and" });
        }

        public void AddHasClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            AddCondition(new Having { Column = relation, Operator = "exists", Value = "(" });
            callback(model);
            AddCondition(new Having { Column = "", Operator = "", Value = ")", Boolean = "and" });
        }

        public void AddHasNotClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            AddCondition(new Having { Column = relation, Operator = "not exists", Value = "(" });
            callback(model);
            AddCondition(new Having { Column = "", Operator = "", Value = ")", Boolean = "and" });
        }

        public void AddHavingInClause(string key, params object[] values)
        {
            AddCondition(new Having { Column = key, Operator = "in", Value = "(" + string.Join(", ", values) + ")", Boolean = "and" });
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

        public void Bind(SQLiteCommand command)
        {
            Having[] havings = this.GetConditions<Having>();

            for (int i = 0; i < havings.Length; i++)
            {
                this.Parameters.TryGetValue($"@having{i}", out string? value);

                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@having{i}";

                if (value is null || value == "null")
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = value;
                }
            }
        }
    }
}
