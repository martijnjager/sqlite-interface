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
    public class JoinClause : BaseClause, IJoinClause
    {
        protected struct Join
        {
            public Join() { }
            public string Table { get; set; }
            public string First { get; set; }
            public string Second { get; set; }
            public string Operator { get; set; }
        }

        public void AddJoinClause(string table, string first, string op, string second)
        {
            Add(new Join { Table = table, First = first, Operator = op, Second = second });
        }

        protected override void Add<T>(T condition)
        {
            if (condition is Join joinCondition)
            {
                AddCondition(joinCondition);
            }
        }

        public override string Compile()
        {
            StringBuilder query = new("");
            Join[] joins = this.GetConditions<Join>();

            for (int i = 0; i < joins.Length; i++)
            {
                query.Append(" JOIN ");
                query.Append(joins[i].Table);
                query.Append(" ON ");
                query.Append(joins[i].First);
                query.Append(' ');
                query.Append(joins[i].Operator);
                query.Append(' ');
                query.Append(joins[i].Second);
            }

            return query.ToString();
        }

        public void Bind(SQLiteCommand command)
        {
            // No binding required
        }
    }
}
