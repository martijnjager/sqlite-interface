using Database.Providers;
using Database.Console;
using Database.Console.Commands;
using Database.Console.Commands.Migration;
using Database.Grammar;
using Command = Database.Console.BaseCommand;
using Database.Transactions;
using System.Reflection;
using Database.Relations;
using Database.Cache;

namespace Database
{
    public static class Boot
    {
        public static void Start(Assembly executingAssembly)
        {
            App.RegisterSingleton<ParamBag>();
            App.RegisterSingleton<GrammarCompiler>();
            App.RegisterSingleton<Command>();
            App.RegisterSingleton<Configuration>();
            App.RegisterSingleton<Provider>();
            App.RegisterSingleton<Migration>();
            App.RegisterSingleton<TransactionManager>();
            App.RegisterSingleton<Events.Dispatcher>();
            App.RegisterSingleton<JobDispatcher>();
            //Command.RegisterCommand<Migrate>();
            //Command.RegisterCommand<RevertMigrate>();
            //Command.RegisterCommand<Make>();
            MigrationRegistry.LoadMigrationTypes(executingAssembly);
            ModelRegistry.LoadModelTypes(executingAssembly);
            RegisterProvider<CacheInvalidatorProvider>();
        }

        public static void RegisterProvider<T>()
        {
            App.Get<Provider>().Register<T>();
        }

        public static ParamBag ParamBag(this App app)
        {
            return App.Get<ParamBag>();
        }

        public static Command BaseCommand(this App app)
        {
            return App.Get<Command>();
        }

        public static Configuration Configuration(this App app)
        {
            return App.Get<Configuration>();
        }

        public static TransactionManager ConnectionManager(this  App app)
        {
            return App.Get<TransactionManager>();
        }

        public static void Restart(this App app)
        {
            //System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
            //Environment.Exit(0);
        }

        public static Events.Dispatcher EventDispatcher(this App app)
        {
            return App.Get<Events.Dispatcher>();
        }

        public static JobDispatcher Dispatcher(this App app)
        {
            return App.Get<JobDispatcher>();
        }
    }
}
