using Database.Contracts.Cache;
using Database.Enums;
using Database.Extensions.Model;

namespace Database.Contracts
{
    public interface IModel : IExpando, ICacheable
    {
        void SetAttributes(IDictionary<string, dynamic?> bag);

        dynamic GetValue(string attribute);

        void Load(string relations);

        IModel Create();

        IModel Update();

        QueryResult<SaveStatus> Delete();

        bool IsTrashed();

        void Set(string attribute, dynamic value);
    }
}
