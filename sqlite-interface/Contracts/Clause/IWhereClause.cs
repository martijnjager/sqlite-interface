using Database.Contracts.Relation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IWhereClause : IClause
    {
        public void AddWhereClause(string key, string op, object value);

        public void AddOrWhereClause(string key, string op, object value);

        public void AddNestedWhereClause(Action<Eloquent> callback, Eloquent model);

        public void AddWhereHasClause(string relation, Action<Eloquent> callback, Eloquent model, IJoin join);

        public void AddWhereHasClause(string relation, Eloquent model, IJoin join);

        public void AddWhereHasNotClause(string relation, Action<Eloquent> callback, Eloquent model, IJoin join);

        public void AddWhereHasNotClause(string relation, Eloquent model, IJoin join);

        public void AddWhereInClause(string key, params object[] values);

        public bool HasWhereColumn(string key);
    }
}
