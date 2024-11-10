using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface IDeleteClause : IClause
    {
        public void AddDeleteClause();

        public void AddSoftDeleteClause();
    }
}
