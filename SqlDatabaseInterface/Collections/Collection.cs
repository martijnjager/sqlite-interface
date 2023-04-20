using Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Collections
{
    public class Collection : ICollection<Model>
    {
        protected List<Model> items;

        public Collection(List<Model> newItems = null)
        {
            if (newItems != null) this.items = newItems;
            else this.items = new List<Model>();
        }

        public int Count => items.Count();

        public bool IsReadOnly => false;

        public void Add(Model item)
        {
            bool hasItem = items.Any(i => i.GetValue("id") == item.GetValue("id"));

            if (!hasItem)
            {
                this.items.Add(item);
            }
        }

        public void Clear()
        {
            this.items.Clear();
        }

        public bool Contains(Model item)
        {
            return items.Any(i => i.GetValue("id") == item.GetValue("id"));
        }

        public void CopyTo(Model[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public Model First()
        {
            if (this.Count() < 1)
            {
                return null;
            }

            return this.items.First();
        }

        public Model FirstWhere(string column, string value)
        {
            Model model = null;

            foreach (Model entry in this.items)
            {
                if (entry.GetValue(column).Equals(value))
                {
                    model = entry;
                }
            }

            return model;
        }

        public Model Last()
        {
            if (this.Count() < 1)
            {
                return null;
            }

            return this.items.Last();
        }

        public List<Model> All()
        {
            return this.items;
        }

        public IEnumerator<Model> GetEnumerator()
        {
            return null;
        }

        public bool Remove(Model item)
        {
            if (this.Contains(item))
            {
                return this.items.Remove(item);
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }

        public Array ToArray()
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

            foreach (Model model in this.items)
            {
                Dictionary<string, string> key = new Dictionary<string, string>();
                foreach (var x in model.GetKeys())
                {
                    key.Add(x, model.GetValue(x));
                }
                data.Add(key);
            }

            return data.ToArray();
        }

        public List<string> Pluck(string key)
        {
            Array data = this.ToArray();
            List<string> foundData = new List<string>();

            foreach (Dictionary<string, string> model in data)
            {
                if (model.ContainsKey(key))
                {
                    foundData.Add(model[key]);
                }
            }

            return foundData;
        }
        public bool HasItems()
        {
            return this.items.Count > 0;
        }
    }
}
