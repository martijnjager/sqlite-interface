using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class ParamBag
    {
        private readonly List<Tuple<string, string>> Keys;

        public ParamBag() {
            this.Keys = new List<Tuple<string, string>>();
        }

        public ParamBag Add(string key, string data)
        {
            Tuple<string, string> item = new Tuple<string, string>(key, data);
            this.Keys.Add(item);

            return this;
        }

        public List<Tuple<string, string>> GetParameters() => this.Keys;

        public List<string> GetKeys()
        {
            List<string> keys = new List<string>();

            foreach (Tuple<string, string> item in this.Keys)
            {
                keys.Add(item.Item1);
            }

            return keys;
        }
        public List<string> GetValues()
        {
            List<string> keys = new List<string>();

            foreach (Tuple<string, string> item in this.Keys)
            {
                keys.Add(item.Item2);
            }

            return keys;
        }

        public string GetValue(string parameter)
        {
            foreach (Tuple<string,string> p in this.Keys)
            {
                if (p.Item1.Equals(parameter))
                {
                    return p.Item2;
                }
            }

            return null;
        }

        public void Clear()
        {
            this.Keys.Clear();
        }
    }
}
