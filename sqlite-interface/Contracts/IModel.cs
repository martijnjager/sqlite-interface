using Database.Enums;
using Database.Extensions.Model;

namespace Database.Contracts
{
    public interface IModel : IExpando
    {
        void Assign(ParamBag bag);

        dynamic GetValue(string attribute);

        void Load(string relations);

        IModel Create<T>() where T : IModel;

        IModel Update<T>() where T : IModel;

        QueryResult<SaveStatus> Delete();

        bool IsTrashed();

        void Set(string attribute, dynamic value);
    }
}
