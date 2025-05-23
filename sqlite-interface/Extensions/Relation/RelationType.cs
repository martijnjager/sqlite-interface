using Database.Contracts;
using Database.Extensions;
using Database.Relations;
using Database.Relations.Join;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions.Relation
{
    public static class RelationType
    {
        /// <summary>
        /// Adds a "has one" condition to the query.
        /// </summary>
        /// <typeparam name="T">The type of the relation.</typeparam>
        /// <returns>The <see cref="Join"/> instance.</returns>
        public static HasOne HasOne<T>(this IModel query, string foreignkey)
        {
            return new HasOne(query, typeof(T), query.Attributes.PrimaryKey, foreignkey);
        }

        /// <summary>
        /// Adds a "has one" condition to the query.
        /// </summary>
        /// <typeparam name="T">The type of the relation.</typeparam>
        /// <returns>The <see cref="Join"/> instance.</returns>
        public static HasMany HasMany<T>(this IModel query, string foreignkey)
        {
            return new HasMany(query, typeof(T), query.Attributes.PrimaryKey, foreignkey);
        }

        /// <summary>
        /// Adds a "belongs to" condition to the query.
        /// Generates the foreign key by method name
        /// </summary>
        /// <typeparam name="T">The type of the relation.</typeparam>
        /// <param name="foreignId">The foreign key.</param>
        /// <returns>The <see cref="Join"/> instance.</returns>
        public static BelongsTo BelongsTo<T>(this IModel query, string foreignKey, string primaryKey)
        {
            //MethodBase previousMethod = Stack.GetPreviousMethod();
            //string foreignKey = previousMethod.Name.ToSnakeCase() + "_id";

            return new BelongsTo(query, typeof(T), primaryKey, foreignKey);
        }

        /// <summary>
        /// Adds a "belongs to many" condition to the query.
        /// Generates the foreign key by method name
        /// Generates the primary key by the query's table name
        /// </summary>
        /// <typeparam name="IntersectionTable">The type of the intersection table.</typeparam>
        /// <typeparam name="TargetTable">The type of the target table.</typeparam>
        /// <returns>The <see cref="Join"/> instance.</returns>
        public static BaseJoin BelongsToMany<IntersectionTable, TargetTable>(this IModel query)
        {
            MethodBase previousMethod = Stack.GetPreviousMethod();
            string foreignKey = previousMethod.Name.ToSnakeCase() + "_id"; // role_id
            string primaryKey = query.GetTable().ToSnakeCase().Singular() + "_id"; // user_id

            return new BelongsToMany(query, typeof(IntersectionTable), typeof(TargetTable), primaryKey, foreignKey);
        }
    }
}
