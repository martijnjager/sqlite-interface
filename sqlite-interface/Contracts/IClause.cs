using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface IClause
    {
        public string Compile();

        public void Clear();

        public T[] GetConditions<T>();

        public bool HasConditions();

        public void Bind(DbCommand command);
    }
}
