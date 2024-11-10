using Database.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Database
{
    public class Configuration
    {
        internal Structure? Data;

        internal JsonDocument? JsonData;

        private const string ConfigName = "config.json";

        public Configuration()
        {
            ReadConfiguration();

            this.Data = null;
            this.JsonData = null;
        }

        private void ReadConfiguration()
        {
            if (File.Exists(ConfigName) == false)
            {
                File.Create(ConfigName);
            }

            string text = File.ReadAllText(ConfigName);

            this.JsonData = JsonDocument.Parse(text);
        }

        public string? Key(string key)
        {
            if (this.JsonData is null)
            {
                return null;
            }

            return FindKey(this.JsonData.RootElement, key);
        }

        private static string? FindKey(JsonElement element, string key)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(key, out JsonElement value))
                {
                    return value.ToString();
                }

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string? result = FindKey(property.Value, key);

                    if (result is not null)
                    {
                        return result;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string? result = FindKey(item, key);

                    if (result is not null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
