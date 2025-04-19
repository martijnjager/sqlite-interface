using Database.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface ICache<T>
    {
        void Store(string key, T value);

        T Get(string key);

        void Remove(string key);

        void Clear();

        bool Contains(string key);
    }
}
