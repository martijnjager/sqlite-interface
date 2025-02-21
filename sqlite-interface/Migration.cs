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
using Database.Collections;
using System.Dynamic;
using Database.Extensions;
using MigrationModel = Database.Models.Migration;
using Database.Connection;

namespace Database
{
    public class Migration : BaseConnection
    {
        private readonly List<IMigration> migrationList;
        private readonly List<ISeeder> seederList;
        protected Blueprint Table { get; private set; }

        private const string MIGRATION_STUB_LOCATION = "E:\\OneDrive\\Projects\\sqlite-interface\\SqlDatabaseInterface\\Migrations\\Stub\\Migration.stub";
        private const string MODEL_STUB_LOCATION = "E:\\OneDrive\\Projects\\sqlite-interface\\SqlDatabaseInterface\\Models\\Stub\\Model.stub";
        private const string MIGRATION_LOCATION = "E:\\OneDrive\\Projects\\docker\\Server\\Server\\Database\\Migrations\\Tables\\";
        private const string MODEL_LOCATION = "E:\\OneDrive\\Projects\\docker\\Server\\Server\\Database\\Models\\";

        public static Migration Instance => InstanceContainer.Get<Migration>();

        public Migration()
        {
            this.migrationList = new List<IMigration>();
            this.seederList = new List<ISeeder>();
            this.Table = new Blueprint();
        }

        public static void RegisterMigration(Type migrationType)
        {
            IMigration migration = (IMigration)Factory.Create(migrationType);
            Instance.migrationList.Add(migration);

            if (migration is ISeeder)
            {
                RegisterSeeder(migrationType);
            }
        }

        public static void RegisterSeeder(Type seederType)
        {
            ISeeder seeder = (ISeeder)Factory.Create(seederType);
            Instance.seederList.Add(seeder);
        }

        /// <summary>
        /// Runs the migrations that have not been run yet per batch
        /// Fetches the migrations that have been run and compares them to the registered migrations
        /// 
        /// </summary>
        public static void RunMigrationsUp()
        {
            RunDefaultMigration();
            MigrationModel model = new();
            List<IModel> models = model.Get<Model>();

            // Get the batch index of the last executed migration
            List<string> migrationsRun = models.ToDictionaryArray().Pluck("migration");
            var newBatchIndex = 1;
            try
            {
                var batchData = models.OrderByDescending(m => m.GetValue("batch")).FirstOrDefault();

                if (batchData is not null)
                {
                    int.TryParse(batchData.GetValue("batch"), out newBatchIndex);
                    newBatchIndex++;
                }
            } catch (Exception e)
            {
                // No migrations have been run yet

                BaseCommand.WriteLine(e.Message);
            }

            var migrationsToRun = Instance.migrationList.Where(m => !migrationsRun.Contains(m.GetType().FullName));

            foreach (var migration in migrationsToRun) 
            {
                Instance.Table = new Blueprint();
                Blueprint tableDown = new();
                BaseCommand.WriteLine("Running migration: " +  migration.GetType().FullName);
                migration.Up(Instance.Table);
                migration.Down(tableDown);

                string query = Builder.Handle(Instance.Table.GetBase());
                string queryDown = Builder.Handle(tableDown.GetBase());

                ParamBag bag = new();

                // Save the migration to the database
                model = new MigrationModel();

                // Save the executed migration query to the database
                bag.Add("migration", migration.GetType().FullName)
                    .Add("batch", newBatchIndex.ToString())
                    .Add("up", query, true)
                    .Add("down", queryDown, true);
                model.Assign(bag);
                model.Create<MigrationModel>();
                Instance.RunSaveQuery(query);
                BaseCommand.WriteLine("Finished migration");
                Instance.Table = new Blueprint();
            };

            RunSeeders();
        }

        /// <summary>
        /// Reverses the last batch of migrations
        /// </summary>
        public static void RunMigrationsDown()
        {
            MigrationModel model = new();
            var batchIndex = model.OrderBy("batch", "desc").First<Model>()?.GetValue("batch");
            List<IModel> models = model.Where("batch", batchIndex).Get();
            List<string> migrations= models.ToDictionaryArray().Pluck("down");

            foreach (string migrationDown in migrations)
            {
                Instance.RunSaveQuery(migrationDown);
            }
            //Instance.migrationList.ForEach((migration) =>
            //{
            //    if (migrationNames.Contains(migration.GetType().FullName))
            //    {
            //        BaseCommand.WriteLine("Reversing migration" + migration.GetType().FullName);
            //        migration.Down(Instance.table);
            //        string table = Builder.Handle(Instance.table);
            //        Instance.RunSaveQuery(table);

            //        ParamBag bag = new ParamBag();

            //        bag.Add("migration", migration.GetType().FullName);
            //        model.Where("migration", migration.GetType().FullName)
            //            .First()
            //            .Delete();
            //        BaseCommand.WriteLine("Finished reverting");
            //        Instance.table = new Blueprint();
            //    }
            //});
        }

        /// <summary>
        /// Runs the default migrations for registering the executed migrations
        /// </summary>
        private static void RunDefaultMigration()
        {
            MigrationTable migration = new();
            migration.Up(Instance.Table);
            string table = Builder.Handle(Instance.Table.GetBase());
            Instance.RunSaveQuery(table);
        }

        /// <summary>
        /// Runs registered seeders
        /// </summary>
        public static void RunSeeders()
        {
            Instance.seederList.ForEach((seed) =>
            {
                seed.PreloadData();
            });
        }

        /// <summary>
        /// Creates a new migration and model by name and location
        /// </summary>
        /// <param name="model"></param>
        public static void Create(string model)
        {
            //CreateDll();
            Create(model, MIGRATION_STUB_LOCATION, MIGRATION_LOCATION);
            CreateModel(model);

            //InstanceContainer.Instance.Restart();
        }

        /// <summary>
        /// Creates a new model by name
        /// </summary>
        /// <param name="model"></param>
        private static void CreateModel(string model)
        {
            Create(model, MODEL_STUB_LOCATION, MODEL_LOCATION);
            //CreateDll("model");
        }

        /// <summary>
        /// Creates a new file for model/migration by name and location
        /// </summary>
        /// <param name="model"></param>
        /// <param name="stubLocation"></param>
        /// <param name="placementLocation"></param>
        private static void Create(string model, string stubLocation, string placementLocation)
        {
            string[] content = File.ReadAllLines(stubLocation);

            model = string.Concat(model[..1].ToUpper(), model.AsSpan(1));

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
