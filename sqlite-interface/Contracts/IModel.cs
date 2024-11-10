using Database.Enums;
using Database.Extensions.Model;

namespace Database.Contracts
{
    public interface IModel : IExpando
    {
        void Assign(ParamBag bag);

        dynamic GetValue(string attribute);

        void Load(string relations);

        QueryResult<SaveStatus> Save<T>() where T : IModel;

        QueryResult<SaveStatus> Delete();

        bool IsTrashed();
    }
}
