using Database.Clauses;
using Database.Enums;
using Database.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Connection
{
    internal interface IReader : IConnection
    {
        Task<QueryResult<SaveStatus>> ReadAsync(Transaction transaction);
 
        Collection Read(Transaction transaction);
    }
}
