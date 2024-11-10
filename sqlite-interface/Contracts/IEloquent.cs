using Database.Attribute;
using Database.Relations;

namespace Database.Contracts
{
    public interface IEloquent : IRelation, IAttribute
    {
        ICollection<string> GetKeys();

        IDictionary<string, string> GetAttributes();

        IList<string> GetRawAttributes();

        IDictionary<string, Relations.Relation> GetRelations();

        string GetTable();

        IModel? Find<T>(string key) where T : IModel;

        Eloquent Select(params string[] columns);

        Eloquent Select(IDictionary<string, string> columns);

        Eloquent Join(string table, string key, string op, string value);

        Eloquent Where(string key, string op, string value);

        Eloquent WhereNull(string where);

        Eloquent WhereNotNull(string where);

        Eloquent OrWhere(string key, string op, string value);

        Eloquent Where(Action<Eloquent> callback);

        Eloquent GroupBy(string key);

        Eloquent Having(string key, string op, string value);

        Eloquent Limit(int limit);

        Eloquent WithTrashed();

        List<IModel> Get<T>() where T : IModel;

        IModel? First<T>() where T : IModel;
    }
}
