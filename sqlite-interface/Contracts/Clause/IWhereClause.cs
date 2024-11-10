using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IWhereClause : IClause
    {
        public void AddWhereClause(string key, string op, string value);

        public void AddOrWhereClause(string key, string op, string value);

        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model);

        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model);

        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model);

        public void AddWhereInClause(string key, params object[] values);

        public bool HasWhereColumn(string key);
    }
}
