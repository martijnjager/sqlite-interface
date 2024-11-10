using Database.Contracts;
using Database.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Relations
{
    public struct Relation
    {
        public RelationType Type { get; set; }
        public List<IModel> Models { get; set; }
    }

    public struct ToLoad
    {
        public string ParentKey { get; private set; }
        
        public string Key { get; private set; }

        public List<ToLoad> Children { get; private set; }

        public ToLoad(string key, string parentKey)
        {
            ParentKey = parentKey;
            Key = key;
            Children = new();
        }

        public void AddChild(string key)
        {
            if (!Children.Any(c => c.Key == key))
            {
                Children.Add(new ToLoad { ParentKey = Key, Key = key, Children = new() });
            }
        }
    }
}
