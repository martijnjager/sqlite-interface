using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IHavingClause : IClause
    {
        public void AddHavingClause(string key, string op, string value);

        public void AddOrHavingClause(string key, string op, string value);

        public void AddNestedHavingClause(Action<Eloquent> callback, Eloquent model);

        public void AddHasClause(string relation, Action<Eloquent> callback, Eloquent model);

        public void AddHasNotClause(string relation, Action<Eloquent> callback, Eloquent model);

        public void AddHavingInClause(string key, params object[] values);
    }
}
