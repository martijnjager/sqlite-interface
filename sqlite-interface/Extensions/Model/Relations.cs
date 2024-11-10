using Database.Contracts;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Database.Extensions.Model
{
    public static class Relations
    {
        public static List<ToLoad> BuildRelationTree(this IRelation model, string relations)
        {
            List<ToLoad> relationsToLoad = new();
            
            foreach (string relation in relations.Split(','))
            {
                string[] keys = relation.Split('.');
                ToLoad current = relationsToLoad.FirstOrDefault((r) => r.Key == keys[0]);

                if (current.Key is null)
                {
                    current = new ToLoad(keys[0], null);

                    relationsToLoad.Add(current);
                }

                for (int i = 1; i < keys.Length; i++)
                {
                    current.AddChild(keys[i]);
                    current = current.Children.Last();
                }
            }

            return relationsToLoad;
        }

        public static MethodInfo? GetRelationMethod(this IRelation relation, string methodName)
        {
            MethodInfo method = relation.GetType().GetMethod(methodName.Plural());

            method ??= relation.GetType().GetMethod(methodName.Singular());

            method ??= relation.GetType().GetMethod(methodName);

            return method;
        }

        //public static void Load(this List<IModel> collection, string relations)
        //{
        //    List<ToLoad> toLoads = collection.First().BuildRelationTree(relations);
        //    MethodInfo relationMethod = collection.First().GetRelationMethod(relations.Split('.')[0]);
        //    if (relationMethod is null)
        //    {
        //        return;
        //    }

        //    Join query = relationMethod.Invoke(collection.First(), null) as Join;
        //}

        //public static Eloquent With(this IEloquent model, params string[] relations)
        //{
        //    model.relations.AddToEagerLoad(relations);

        //    return (Eloquent)model;
        //}
    }
}
