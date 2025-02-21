using Database.Clauses;
using Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Connection
{
    internal interface IWriter : IConnection, IDisposable
    {
        QueryResult<SaveStatus> Save(ClauseManager clauseManager);
    }
}
