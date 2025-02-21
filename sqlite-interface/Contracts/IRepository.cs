﻿using Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface IRepository<TModel> where TModel : IModel
    {
        IModel Save(ParamBag data);

        IModel FindBy(string key, string value, string? relations = null, string[]? except = null);

        List<IModel> FindWhere(string key, string value, string? relations = null, string[]? except = null);

        IModel Get(string id, string? relations = null);

        List<IModel> All(string? relations = null);

        void SetModel(TModel model);

        IModel Update(string id, ParamBag data);
    }
}
