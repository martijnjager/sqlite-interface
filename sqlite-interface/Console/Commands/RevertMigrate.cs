using Database.Contracts;

namespace Database.Console.Commands
{
    public class RevertMigrate : BaseCommand, ICommand
    {
        public RevertMigrate() { }

        public string Description => "Reverts migrations of last batch only";

        public string Signature => "migrations:rollback";

        public void Command()
        {
            Database.Migration.RunMigrationsDown();
        }
    }
}
