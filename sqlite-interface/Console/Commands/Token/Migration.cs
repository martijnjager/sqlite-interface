using Database.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Console.Commands.Token
{
    public class Migration : BaseCommand, ICommand
    {
        public string Description => "Generates security_token migration and runs the migration";

        public string Signature => "token:migration";

        public void Command()
        {
            Database.Migration.Create("security_tokens");
            Database.Migration.RunMigrationsUp();
        }
    }
}
