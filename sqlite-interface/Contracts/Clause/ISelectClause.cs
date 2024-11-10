using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Clause
{
    public interface ISelectClause : IClause
    {
        public void AddColumn(string column, string alias = "");
    }
}
