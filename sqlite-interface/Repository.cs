using Database;
using Database.Contracts;
using Database.Enums;
using Database.Relations;

namespace Database
{
    public class Repository<TModel> : IRepository<TModel> where TModel : IModel
    {
        protected TModel _model;

        protected const int OPERATION_WAIT_TIME = 1000;

        public Repository()
        {
            this._model = (TModel?)InstanceContainer.ModelByKey(typeof(TModel).Name);
        }

        public QueryResult<SaveStatus> Save(ParamBag data)
        {
            Model m = this.GetModel();
            m.Assign(data);
            return m.Save<TModel>();
        }

        public QueryResult<SaveStatus> Update(ParamBag data)
        {
            Model m= this.GetModel();
            var primaryKey = m.GetKeys().First();

            if (m.Find<TModel>(primaryKey).GetValue(primaryKey) == null)
            {
                throw new Exception("Cant update model");
            }

            m.Assign(data);
            return m.Save<TModel>();
        }

        public IModel FindBy(string key, string value, string? relations = null)
        {
            IModel? model = this.GetModel().Where(key, value).First<Model>();

            if (model is not null && relations is not null)
            {
                model.Load(relations);
            }

            return model;
        }

        public List<IModel> FindWhere(string key, string value, string? relations = null)
        {
            List<IModel> models = this.GetModel().Where(key, value).Get<Model>();

            if (relations is not null)
            {
                foreach (Model model in models.Cast<Model>())
                {
                    model.Load(relations);
                }
            }

            return models;
        }

        public IModel? Get(string id, string? relations = null)
        {
            IModel? model = this.GetModel().Find<Model>(id);

            if (model is not null && relations is not null)
            {
                model.Load(relations);
            }

            return model;
        }

        public List<IModel> All(string? relations = null)
        {
            List<IModel> models = this.GetModel().With(relations).Get<Model>();

            if (relations is not null)
            {
                var ids = models.Select(m => m.GetValue("id")).Distinct().ToArray();

                RelationManager.LoadRelations(this.GetModel().Relations.ToEagerLoad(), models);

                //foreach (Model model in models.Cast<Model>())
                //{
                //    model.Load(relations);
                //}
            }

            return models;
        }

        public void SetModel(TModel model)
        {
            this._model = model;
        }

        public Model GetModel() => this._model as Model;
    }
}
