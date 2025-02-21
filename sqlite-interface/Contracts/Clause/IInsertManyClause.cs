using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IInsertManyClause : IClause
    {
        public void AddInsertManyClause(List<IDictionary<string, string>> values);
    }
}
