using Database.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Relation
{
    public interface IJoin
    {
        public Type MainTable { get; }
        public Type TargetTable { get; } // belongsToMany target table
        public Type IntersectionTable { get; } // belongsToMany intersection table
        public string PrimaryKey { get; }
        public string ForeignKey { get; }
        public RelationType Type { get; }

        Eloquent BuildQuery(string key, dynamic[] values);
    }
}
