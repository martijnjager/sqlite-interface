using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Database.Contracts;
using Database.Migrations;
using SqlDatabaseInterface;
using SqlDatabaseInterface.Migrations;

namespace Database
{
    public class Migration : Connection
    {
        private List<IMigration> migrationList;
        private List<ISeeder> seederList;
        protected Blueprint table { get; }

        private static readonly object @lock = new object();
        private static Migration instance = null;
        public static Migration Instance
        {
            get
            {
                lock (@lock)
                {
                    if (instance == null)
                    {
                        instance = new Migration();
                    }
                    return instance;
                }
            }
        }

        public Migration()
        {
            this.migrationList = new List<IMigration>();
            this.seederList = new List<ISeeder>();
            this.table = new Blueprint();
        }

        public static void RegisterMigration<T>()
        {
            IMigration migration = (IMigration)Factory.Create<T>();
            Instance.migrationList.Add(migration);
        }

        public static void RegisterSeeder<T>()
        {
            ISeeder seeder = (ISeeder)Factory.Create<T>();
            Instance.seederList.Add(seeder);
        }

        public static void RunMigrations()
        {
            RunDefaultMigration();
            MigrationModel model = new MigrationModel();
            List<string> migrationsRun = model.Get().Pluck("migration");
            Instance.migrationList.ForEach((migration) =>
            {
                if (!migrationsRun.Contains(migration.GetType().FullName))
                {
                    Blueprint definitions = migration.Up();

                    string table = Builder.Handle(definitions);

                    Instance.RunNonQuery(table);

                    ParamBag bag = new ParamBag();

                    bag
                    //.Add(definitions.primaryKey, null)
                        .Add("migration", migration.GetType().FullName);

                    model = new MigrationModel();
                    model.Assign(bag);
                    model.Save<MigrationModel>();
                }
            });

            RunSeeders();
        }

        private static void RunDefaultMigration()
        {
            MigrationTable migration = new MigrationTable();
            Blueprint definitions = migration.Up();
            string table = Builder.Handle(definitions);
            Instance.RunNonQuery(table);
        }

        public static void RunSeeders()
        {
            Instance.seederList.ForEach((seed) =>
            {
                seed.PreloadData();
            });
        }
    }
}
