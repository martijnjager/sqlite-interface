using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Database.Console;
using Database.Contracts;
using Database.Migrations;
using Database.Models;
using Database.Migrations.Table;

namespace Database
{
    public class Migration : Connection
    {
        private List<IMigration> migrationList;
        private List<ISeeder> seederList;
        protected Blueprint table { get; private set; }

        private const string MIGRATION_STUB_LOCATION = "E:\\OneDrive\\Projects\\sqlite-interface\\SqlDatabaseInterface\\Migrations\\Migration.stub";
        private const string MODEL_STUB_LOCATION = "E:\\OneDrive\\Projects\\sqlite-interface\\SqlDatabaseInterface\\Migrations\\Model.stub";
        private const string MIGRATION_LOCATION = "E:\\OneDrive\\Projects\\docker\\Server\\Server\\Database\\Migrations\\Tables\\";
        private const string MODEL_LOCATION = "E:\\OneDrive\\Projects\\docker\\Server\\Server\\Database\\Models\\";

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

        public static void RunMigrationsUp()
        {
            RunDefaultMigration();
            MigrationModel model = new MigrationModel();
            List<string> migrationsRun = model.Get().Pluck("migration");
            var batch = model.OrderBy("batch", "desc").First();
            var newBatchIndex = 1;

            if (batch != null)
            {
                int.TryParse(batch.GetValue("batch"), out newBatchIndex);
                newBatchIndex++;
            }

            Instance.migrationList.ForEach((migration) => 
            {
                if (!migrationsRun.Contains(migration.GetType().FullName))
                {
                    Instance.table = new Blueprint();
                    BaseCommand.WriteLine("Running migration: " +  migration.GetType().FullName);
                    migration.Up(Instance.table);

                    string table = Builder.Handle(Instance.table);

                    Instance.RunSaveQuery(table);

                    ParamBag bag = new ParamBag();

                    bag.Add("migration", migration.GetType().FullName);
                    bag.Add("batch", newBatchIndex.ToString());

                    model = new MigrationModel();
                    model.Assign(bag);
                    model.Save<MigrationModel>();
                    BaseCommand.WriteLine("Finished migration");
                    Instance.table = new Blueprint();
                }
            });

            RunSeeders();
        }

        public static void RunMigrationsDown()
        {
            MigrationModel model = new MigrationModel();
            var batchIndex = model.OrderBy("batch", "desc").First().GetValue("batch");
            Collection models = model.Where("batch", batchIndex).Get();
            List<string> migrationNames = models.Pluck("migration");

            Instance.migrationList.ForEach((migration) =>
            {
                if (migrationNames.Contains(migration.GetType().FullName))
                {
                    BaseCommand.WriteLine("Reversing migration" + migration.GetType().FullName);
                    migration.Down(Instance.table);
                    string table = Builder.Handle(Instance.table);
                    Instance.RunSaveQuery(table);

                    ParamBag bag = new ParamBag();

                    bag.Add("migration", migration.GetType().FullName);
                    model.Where("migration", migration.GetType().FullName)
                        .First()
                        .Delete();
                    BaseCommand.WriteLine("Finished reverting");
                    Instance.table = new Blueprint();
                }
            });
        }

        private static void RunDefaultMigration()
        {
            MigrationTable migration = new MigrationTable();
            migration.Up(Instance.table);
            string table = Builder.Handle(Instance.table);
            Instance.RunSaveQuery(table);
        }

        public static void RunSeeders()
        {
            Instance.seederList.ForEach((seed) =>
            {
                seed.PreloadData();
            });
        }

        public static void Create(string model, string customLocation = null)
        {
            //CreateDll();
            Create(model, MIGRATION_STUB_LOCATION, MIGRATION_LOCATION);
            CreateModel(model);

            //InstanceContainer.Instance.Restart();
        }

        private static void CreateModel(string model)
        {
            Create(model, MODEL_STUB_LOCATION, MODEL_LOCATION);
            //CreateDll("model");
        }

        private static void Create(string model, string stubLocation, string placementLocation)
        {
            string[] content = File.ReadAllLines(stubLocation);

            model = model.Substring(0, 1).ToUpper() + model.Substring(1);

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i].Contains("{Name}"))
                {
                    content[i] = content[i].Replace("{Name}", $"{model}");
                }
            }

            if (stubLocation.Equals(MIGRATION_STUB_LOCATION))
            {
                model += "Table";
            }

            string fileName = placementLocation + model + ".cs";

            if (!File.Exists(fileName))
            {
                File.WriteAllLines(fileName, content);
            }
        }

        //private static void CreateDll()
        //{
        //    Configuration config = InstanceContainer.Instance.Configuration();

        //    string migrations = config.Key("migrations");

        //    if (string.IsNullOrEmpty(migrations))
        //    {
        //        throw new Exception("Migrations location not specified");
        //    }

        //    string[] files = Directory.GetFiles(migrations);
        //    FileInfo[] fileData = new FileInfo[files.Length];

        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        fileData[i] = new FileInfo(files[i]);
        //    }

        //    string outputDll = string.Format(@"{0}\{1}.dll",
        //        Environment.CurrentDirectory, "migration");

        //    CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

        //    CompilerParameters parameters = new CompilerParameters()
        //    {
        //        GenerateExecutable = false,
        //        OutputAssembly = outputDll,
        //        GenerateInMemory = true,
        //    };
        //    parameters.ReferencedAssemblies.Add("SqlDatabaseInterface.dll");

        //    CompilerResults result = provider.CompileAssemblyFromFile(parameters, files);

        //    if (result.Errors.Count > 0)
        //    {
        //        foreach (CompilerError error in result.Errors)
        //        {
        //            BaseCommand.WriteLine(error.ToString());
        //        }

        //        throw new Exception("Compiling migration failed");
        //    }

        //    BaseCommand.WriteLine("Compiling migration successful");
        //}
    }
}
