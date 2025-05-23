using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.QueryBuilder
{
    public readonly struct RawSql
    {
        public string Value { get; }

        public RawSql(string value)
        {
            Value = value;
        }

        public static implicit operator string(RawSql rawSql)
        {
            return rawSql.Value;
        }

        public static implicit operator RawSql(string value)
        {
            return new RawSql(value);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is RawSql rawSql && Value == rawSql.Value;
        }
    }
}
