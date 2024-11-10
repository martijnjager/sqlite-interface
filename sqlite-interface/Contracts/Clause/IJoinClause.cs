using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IJoinClause : IClause
    {
        public void AddJoinClause(string table, string first, string op, string second);
    }
}
