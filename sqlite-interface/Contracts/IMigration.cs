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
        void Up(Blueprint table);
        //IDictionary<string, string> Up();

       void Down(Blueprint table);
    }
}
