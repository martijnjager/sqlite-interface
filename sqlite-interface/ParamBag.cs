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
        private readonly List<Tuple<string, dynamic, bool>> Keys;

        public ParamBag()
        {
            this.Keys = new List<Tuple<string, dynamic, bool>>();
        }

        public ParamBag Add(string key, dynamic data, bool raw = false)
        {
            Tuple<string, dynamic, bool> item = new(key, data, raw);
            this.Keys.Add(item);

            return this;
        }

        public List<Tuple<string, dynamic, bool>> GetParameters() => this.Keys;

        public List<string> GetKeys()
        {
            List<string> keys = new();

            foreach (Tuple<string, dynamic, bool> item in this.Keys)
            {
                keys.Add(item.Item1);
            }

            return keys;
        }

        public List<string> GetValues()
        {
            List<string> keys = new();

            foreach (Tuple<string, dynamic, bool> item in this.Keys)
            {
                keys.Add(item.Item2);
            }

            return keys;
        }

        public string GetValue(string parameter)
        {
            foreach (Tuple<string, dynamic, bool> p in this.Keys)
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

        public static bool TryParse(string input, out ParamBag parameters)
        {
            string[] data = input.Split(',');

            parameters = InstanceContainer.Instance.ParamBag();

            if (data.Length < 1)
            {
                return false;
            }

            foreach (string key in data)
            {
                string[] parameterData = key.Split(':');
                if (parameterData.Length > 1)
                {
                    parameters.Add(parameterData[0], parameterData[1]);
                }
            }

            return true;
        }

        public bool HasKey(string key)
        {
            return this.GetKeys().Contains(key);
        }

        public bool HasAnyKey(params string[] keys)
        {
            foreach (string key in keys)
            {
                if (this.HasKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAllKeys(params string[] keys)
        {
            foreach (string key in keys)
            {
                if (!this.HasKey(key))
                {
                    return false;
                }
            }

            return true;
        }

        public IDictionary<string, string> GetByKeys(string[] keys)
        {
            IDictionary<string, string> foundKeys = new Dictionary<string, string>();

            foreach (string key in keys)
            {
                if (this.HasKey(key))
                {
                    foundKeys.Add(key, this.GetValue(key));
                }
            }

            return foundKeys;
        }
    }
}
