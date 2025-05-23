using Database;
using Database.Contracts;
using Database.Enums;
using Database.Extensions.Model;
using Database.Relations;
using System.Reflection;

namespace Database
{
    public class Repository<TModel> : IRepository<TModel> where TModel : IModel
    {
        protected TModel _model;

        public Repository()
        {
            var model = ModelRegistry.ModelByKey(typeof(TModel).Name);
            
            if (model is null)
            {
                throw new InvalidOperationException($"Model for type {typeof(TModel).Name} not found.");
            }

            this._model = (TModel)model;
        }

        public IModel Save(ParamBag data)
        {
            Model? m = this.GetModel();

            if (m is not null)
            {
                m.SetAttributes(data.ToDictionary());
                return m.Create();
            }

            throw new InvalidOperationException("Model not found.");
        }

        public IModel Update(string id, ParamBag data)
        {
            IModel? foundModel = this.GetModel()?.Find(id);

            if (foundModel is not null)
            {
                foundModel.SetAttributes(data.ToDictionary());
                return foundModel.Update();
            }

            throw new InvalidOperationException("Model not found.");
        }

        public IModel? FindBy(string key, string value, string? relations = null, string[]? except = null)
        {
            var query = this.GetModel()?.Where(key, value);

            if (except is not null)
            {
                this.DontSelect(except);
            }

            var foundModel = query.First();

            if (foundModel is not null && relations is not null)
            {
                foundModel.Load(relations);
            }

            return foundModel;
        }

        public Collection FindWhere(string key, string value, string? relations = null, string[]? except = null)
        {
            var query = this.GetModel().With(relations).Where(key, value);

            if (except is not null)
            {
                this.DontSelect(except);
            }

            var models = query.Get();

            if (relations is not null)
            {
                RelationManager.LoadRelations(this.GetModel().Relations.ToEagerLoad(), models);
            }

            return models;
        }

        public IModel? Get(string id, string? relations = null)
        {
            IModel? model = this.GetModel().Find(id);

            if (model is not null && relations is not null)
            {
                model.Load(relations);
            }

            return model;
        }

        public Collection All(string? relations = null)
        {
            Collection models = this.GetModel().With(relations).Get();

            //if (relations is not null)
            //{
            //    this.GetModel().With(relations);
            //    RelationManager.LoadRelations(this.GetModel().Relations.ToEagerLoad(), models);
            //}

            return models;
        }

        public void SetModel(TModel model)
        {
            this._model = model;
        }

        public Model? GetModel()
        {
            return this._model as Model;
        }

        protected void DontSelect(params string[] key)
        {
            this.GetModel().DontSelect(key);
        }
    }
}
