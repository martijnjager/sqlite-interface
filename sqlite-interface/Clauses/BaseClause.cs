using Database.Contracts;
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
    }
}
