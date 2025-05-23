using Database.Clauses;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.QueryBuilder
{
    internal class ParameterBinder
    {
        public static DbCommand Bind(ClauseManager clauses, DbCommand command)
        {
            if (clauses.wheres.HasConditions())
            {
                clauses.wheres.Bind(command);
            }

            if (clauses.havings.HasConditions())
            {
                clauses.havings.Bind(command);
            }

            if (clauses.insert.HasConditions())
            {
                clauses.insert.Bind(command);
            }

            if (clauses.insertMany.HasConditions())
            {
                clauses.insertMany.Bind(command);
            }

            if (clauses.update.HasConditions())
            {
                clauses.update.Bind(command);
            }

            if (clauses.delete.HasConditions())
            {
                clauses.delete.Bind(command);
            }
            return command;
        }
    }
}
