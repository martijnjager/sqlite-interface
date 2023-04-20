using Database.Grammar;
using SqlDatabaseInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public static class Boot
    {
        public static void Start()
        {
            InstanceContainer.RegisterSingleton<ParamBag>("paramBag");
            InstanceContainer.RegisterSingleton<GrammarCompiler>("grammarCompiler");
            BindModel<MigrationModel>("migrations");
        }

        public static void RegisterMigration<T>()
        {
            Migration.RegisterMigration<T>();
        }

        public static void BindModel<T>(string model)
        {
            InstanceContainer.Bind<T>(model);
        }

        public static void RegisterSeeder<T>()
        {
            Migration.RegisterSeeder<T>();
        }

        public static void RunMigrations()
        {
            Migration.RunMigrations();
        }
    }
}
