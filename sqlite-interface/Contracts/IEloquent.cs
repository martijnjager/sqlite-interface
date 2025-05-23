using Database.Attribute;
using Database.Relations;

namespace Database.Contracts
{
    public interface IEloquent : IRelation, IAttribute
    {
        ICollection<string> GetKeys();

        IDictionary<string, string> GetAttributes();

        IDictionary<string, dynamic> GetCustomAttributes();

        IList<string> GetRawAttributes();

        IDictionary<string, Relations.Relation> GetRelations();

        string GetTable();

        IModel? Find(string key);

        Eloquent Select(params string[] columns);

        Eloquent Select(IDictionary<string, string> columns);

        Eloquent Join(string table, string key, string op, string value);

        Eloquent Where(string key, object op, object value);

        Eloquent WhereNull(string where);

        Eloquent WhereNotNull(string where);

        Eloquent OrWhere(string key, object op, object value);

        Eloquent Where(Action<Eloquent> callback);

        Eloquent WhereHas(string relation, Action<Eloquent> callback);

        Eloquent WhereHas(string relation);

        Eloquent WhereDoesntHave(string relation, Action<Eloquent> callback);

        Eloquent WhereDoesntHave(string relation);

        Eloquent GroupBy(string key);

        Eloquent Having(string key, object op, object value);

        Eloquent Limit(int limit);

        Eloquent WithTrashed();

        Collection Get();

        IModel? First();

        IModel? Create();

        IModel? Update();

        Collection CreateMany(ParamBag[] bags);
    }
}
