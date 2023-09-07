using Database.Contracts;

namespace Database.Console.Commands
{
    public class Migrate : BaseCommand, ICommand
    {
        public Migrate() { }

        public string Description => "Runs migrations that have not been run yet.";

        public string Signature => "migrations:run";

        public void Command()
        {
            Database.Migration.RunMigrationsUp();
        }
    }
}
