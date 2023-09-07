using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Console.Commands
{
    public class SecurityToken : BaseCommand, ICommand
    {
        public string Description => "Generate a security token";

        public string Signature => "token:generate";

        public void Command()
        {

        }
    }
}
