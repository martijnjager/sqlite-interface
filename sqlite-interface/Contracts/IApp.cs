using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface IApp
    {
        T GetInstance<T>(string key);

        T GetInstance<T>();

        void SetSingleton(object instance, string key);
    }
}
