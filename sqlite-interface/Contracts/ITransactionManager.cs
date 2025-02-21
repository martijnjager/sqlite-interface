using Database.Clauses;
using Database.Contracts;
using Database.Enums;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface ITransactionManager
    {
        IConnection GetConnection();

        //void AddQuery(ClauseManager clauseManager, List<Join> joins);

        //QueryResult<SaveStatus> RunTransactions();
        List<IModel> Read<T>(ClauseManager clauseManager) where T : IModel;
    }
}
