using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IInsertClause : IClause
    {
        public void AddInsertClause(string key, string value);
    }
}
