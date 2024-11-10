using Database.Contracts;
using Database.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    //public class BaseRepository : IRepository
    //{
    //    protected Model _model;

    //    protected const int OPERATION_WAIT_TIME = 1000;

    //    public BaseRepository(Model model)
    //    {
    //        this._model = model;
    //    }

    //    public QueryResult<SaveStatus> Save<T>(ParamBag data)
    //    {
    //        Contract.Requires(data.GetParameters().Count > 0);
    //        this._model.Assign(data);
    //        return this._model.Save<T>();
    //    }

    //    public QueryResult<SaveStatus> Update<T>(ParamBag data)
    //    {
    //        Contract.Requires(data.GetParameters().Count > 0);

    //        var primaryKey = this._model.GetKeys().First();

    //        if (this._model.Find(primaryKey).GetValue(primaryKey) == null)
    //        {
    //            throw new Exception("Cant update model");
    //        }

    //        this._model.Assign(data);
    //        return this._model.Save<T>();
    //    }

    //    public Model FindBy(string key, string value)
    //    {
    //        return this._model.Where(key, value).First();
    //    }

    //    public List<Model> FindWhere(string key, string value)
    //    {
    //        return this._model.Where(key, value).Get();
    //    }

    //    public Model Get(string id)
    //    {
    //        return this._model.Find(id);
    //    }

    //    public List<Model> All()
    //    {
    //        return this._model.Get();
    //    }

    //    public void SetModel(Model model)
    //    {
    //        this._model = model;
    //    }

    //    public ICollection<string> Columns()
    //    {
    //        return this._model.GetKeys();
    //    }
    //}
}
