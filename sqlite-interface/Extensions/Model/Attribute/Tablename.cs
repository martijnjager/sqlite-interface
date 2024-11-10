namespace Database.Extensions.Model.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : System.Attribute
    {
        public string Name { get; private set; }

        public TableNameAttribute(string table)
        {
            Name = table;
        }
    }
}
