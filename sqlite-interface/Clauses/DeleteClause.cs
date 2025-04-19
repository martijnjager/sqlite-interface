using Database.Contracts.Clause;
using System.Data.Common;

namespace Database.Clauses
{
    public class DeleteClause : BaseClause, IDeleteClause
    {
        public void AddDeleteClause()
        {
            this.Add(false);
        }

        protected override void Add<T>(T condition)
        {
            if (condition is bool delete)
            {
                AddCondition(delete);
            }
        }

        public void AddSoftDeleteClause()
        {
            this.Add(true);
        }

        public override string Compile()
        {
            if (this.Conditions.Count == 0)
            {
                return string.Empty;
            }

            return "DELETE";
        }

        public new void Bind(DbCommand connection)
        {
            // Nothing to do here
        }
    }
}
