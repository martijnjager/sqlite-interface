using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Contracts;

namespace Database.Providers
{
    public class Provider
    {
        private readonly List<IProvider> providers;
        public Provider() 
        {
            providers = new List<IProvider>();
        }

        public void Register<T>()
        {
            IProvider provider = (IProvider)Factory.Create<T>();

            providers.Add(provider);
        }

        public void Boot()
        {
            foreach (IProvider provider in  providers)
            {
                provider.Boot(InstanceContainer.Instance);
            }
        }
    }
}
