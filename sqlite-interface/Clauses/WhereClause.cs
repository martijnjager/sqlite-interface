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
        protected struct Where
        {
            public Where() 
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
            if (condition is Where whereCondition)
            {
                string[] exemptions = new[] { "(", ")" };

                if (!exemptions.Contains(whereCondition.Value))
                {
                    if (whereCondition.Value is null || whereCondition.Value.Equals("null"))
                    {
                        string valueName = "@where" + (this.Parameters.Count).ToString();
                        this.Parameters.Add(valueName, "null");
                        whereCondition.Value = valueName;
                    }
                    else
                    {
                        if (whereCondition.Operator.Equals("in"))
                        {
                            string[] options = whereCondition.Value.Split(',');

                            int offset = this.Parameters.Count - options.Length;

                            whereCondition.Value = string.Join(",", options.Select(option => "@where" + (offset++).ToString()));

                            offset = this.Parameters.Count - options.Length;

                            foreach (string option in options)
                            {
                                this.Parameters.Add("where" + (offset++).ToString(), option);
                            }
                        }
                        else
                        {
                            string valueName = "@where" + (this.Parameters.Count).ToString();
                            this.Parameters.Add(valueName, whereCondition.Value);
                            whereCondition.Value = valueName;
                        }
                    }
                }

                AddCondition(whereCondition);
            }
        }

        public void AddWhereClause(string key, string op, string value)
        {
            Add(new Where { Column = key, Operator = op, Value = value, Boolean = "and" });
        }

        public void AddOrWhereClause(string key, string op, string value)
        {
            Add(new Where { Column = key, Operator = op, Value = value, Boolean = "or" });
        }

        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where { Column = "", Operator = "", Value = "(" });
            callback(model);
            Add(new Where { Column = "", Operator = "", Value = ")", Boolean = "and" });
        }

        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where { Column = relation, Operator = "exists", Value = "(" });
            callback(model);
            Add(new Where { Column = "", Operator = "", Value = ")", Boolean = "and" });
        }

        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model)
        {
            Add(new Where { Column = relation, Operator = "not exists", Value = "(" });
            callback(model);
            Add(new Where { Column = "", Operator = "", Value = ")", Boolean = "and" });
        }

        public void AddWhereInClause(string key, params object[] values)
        {
            StringBuilder stringBuilder = new("(");
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
                            stringBuilder.Append('\'').Append(item.ToString()).Append('\'').Append(',');
                        }
                    }
                }
            } else
            {
                stringBuilder.Append(string.Join(",", values.ToString()));
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1).Append(')');

            string value = stringBuilder.ToString();

            Add(new Where { Column = key, Operator = "in", Value = value, Boolean = "and" });
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

                //if (wheres[i].Operator == "in")
                //{
                    query.Append(wheres[i].Column).Append(' ')
                        .Append(wheres[i].Operator).Append(' ')
                        .Append(wheres[i].Value).Append(' ');

                    //continue;
                //}

                //query.Append(wheres[i].Column).Append(' ')
                //    .Append(wheres[i].Operator).Append(' ')
                //    .Append($"@where{i}")
                    //.Append(wheres[i].Value != null && !wheres[i].Value.Equals("null") ? 
                    //    $"'{wheres[i].Value}'" :
                    //        wheres[i].Value)
                    //.Append(' ');
            }

            return query.ToString();
        }

        public bool HasWhereColumn(string column)
        {
            return Conditions.Where(Where => Where is Where)
                .Cast<Where>()
                .Any(clause => clause.Column == column);
        }

        public void Bind(SQLiteCommand command)
        {
            Where[] wheres = this.GetConditions<Where>();

            for (int i = 0; i < wheres.Length; i++)
            {
                this.Parameters.TryGetValue(wheres[i].Value, out string? value);
                var parameter = command.CreateParameter();
                parameter.ParameterName = wheres[i].Value;

                if (value is null || value == "null")
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = value;
                }

                command.Parameters.Add(parameter);
            }
        }
    }
}
