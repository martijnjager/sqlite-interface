using Database.Contracts;
using Database.Extensions;
using Database.Models;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;

namespace Database
{
    public class MigrationRegistry : Singleton<MigrationRegistry>
    {
        private readonly List<Type> models;

        internal MigrationRegistry() : base()
        {
            this.models = new();
        }

        public static void LoadMigrationTypes(Assembly executingAssembly, string inputNamespace = null)
        {
            if (string.IsNullOrEmpty(inputNamespace))
            {
                inputNamespace = "Backend.Database.Migrations.Tables";
            }

            Type[] typeList = executingAssembly.GetTypes();
            // Need to fetch all types that are in the Database.Migrations namespace

            List<Type> migrations = typeList.Where(t => t.FullName.Contains(inputNamespace) && !t.Name.Contains('<')).ToList();

            foreach (Type migration in migrations)
            {
                Migration.RegisterMigration(migration);
            }
        }
    }
}
