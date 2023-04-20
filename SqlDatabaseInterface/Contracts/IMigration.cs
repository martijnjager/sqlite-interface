using Database.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface IMigration
    {
        Blueprint Up();
        //IDictionary<string, string> Up();

        string Down();
    }
}
