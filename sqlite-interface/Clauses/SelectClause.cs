using Database.Clauses;
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
    public class SelectClause : BaseClause, ISelectClause
    {
        protected struct Select
        {
            public Select() 
            {
                Column = string.Empty;
                Alias = string.Empty;
            }

            public string Column { get; set; }
            public string Alias { get; set; }
        }
        public void AddColumn(string column, string alias = "")
        {
            Add(new Select { Column = column, Alias = alias });
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
                    query.Append(selects[i].Column);
                    if (selects[i].Alias != "")
                    {
                        query.Append(" AS ").Append(selects[i].Alias);
                    }
                }
            }
            return query.ToString();
        }

        public void Bind(SQLiteCommand command)
        {
            // No binding required
        }
    }
}
