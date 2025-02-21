using Database.Providers;
using Database.Console;
using Database.Console.Commands;
using Database.Console.Commands.Migration;
using Database.Grammar;
using System;
using Command = Database.Console.BaseCommand;
using Database.Transactions;
using System.Reflection;
using Database.Events;

namespace Database
{
    public static class Boot
    {
        public static void Start(Assembly executingAssembly)
        {
            InstanceContainer.RegisterSingleton<ParamBag>();
            InstanceContainer.RegisterSingleton<GrammarCompiler>();
            InstanceContainer.RegisterSingleton<Command>();
            InstanceContainer.RegisterSingleton<Configuration>();
            InstanceContainer.RegisterSingleton<Provider>();
            InstanceContainer.RegisterSingleton<Migration>();
            InstanceContainer.RegisterSingleton<TransactionManager>();
            InstanceContainer.RegisterSingleton<Events.Dispatcher>();
            InstanceContainer.RegisterSingleton<Dispatcher>();
            //Command.RegisterCommand<Migrate>();
            //Command.RegisterCommand<RevertMigrate>();
            //Command.RegisterCommand<Make>();
            InstanceContainer.LoadMigrationTypes(executingAssembly);
            InstanceContainer.LoadModelTypes(executingAssembly);
        }

        public static void RegisterProvider<T>()
        {
            InstanceContainer.Get<Provider>().Register<T>();
        }

        public static ParamBag ParamBag(this InstanceContainer app)
        {
            return app.GetInstance<ParamBag>();
        }

        public static Command BaseCommand(this InstanceContainer app)
        {
            return app.GetInstance<Command>();
        }

        public static Configuration Configuration(this InstanceContainer app)
        {
            return app.GetInstance<Configuration>();
        }

        public static TransactionManager ConnectionManager(this  InstanceContainer app)
        {
            return app.GetInstance<TransactionManager>();
        }

        public static void Restart(this InstanceContainer app)
        {
            //System.Diagnostics.Process.Start(AppDomain.CurrentDomain.FriendlyName);
            //Environment.Exit(0);
        }

        internal static Events.Dispatcher EventDispatcher(this InstanceContainer app)
        {
            return app.GetInstance<Events.Dispatcher>();
        }

        public static Dispatcher Dispatcher(this InstanceContainer app)
        {
            return app.GetInstance<Dispatcher>();
        }
    }
}
