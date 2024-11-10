using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Console.Commands.Migration
{
    public class Make : BaseCommand, ICommand
    {
        public Make() { }

        public string Description => "Create a new migration, this will restart the program to compile the changes";

        public string Signature => "migrations:make model";

        public void Command()
        {
            string modelName = Parameter("model");

            Database.Migration.Create(modelName);
        }
    }
}
