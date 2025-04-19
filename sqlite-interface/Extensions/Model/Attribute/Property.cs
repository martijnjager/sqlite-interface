using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions.Model.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class Property : System.Attribute
    {
        public string Name { get; set; }
        public Type Type { get; private set; }

        public Property(string column, Type type = null)
        {
            Name = column;
            Type = type ?? InferType(column);
        }

        private static Type InferType(string name)
        {
            return name switch
            {
                "int" => typeof(int),
                "string" => typeof(string),
                "bool" => typeof(bool),
                "float" => typeof(float),
                "double" => typeof(double),
                "decimal" => typeof(decimal),
                "DateTime" => typeof(DateTime),
                _ => typeof(string),
            };
        }
    }
}
