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
        internal Structure Data;

        private const string ConfigName = "config.json";

        public Configuration()
        {
            ReadConfiguration();
        }

        private void ReadConfiguration()
        {
            IEnumerable<string> text = File.ReadAllLines(ConfigName);

            Parse(text);
        }

        public string Key(string key)
        {
            Structure currentPosition = this.Data;

            while (!currentPosition.Key.Equals(key) && currentPosition.Next != null)
            {
                currentPosition = currentPosition.Next;
            }

            return currentPosition.Key.Equals(key) ? currentPosition.Value : null;
        }

        private void Parse(IEnumerable<string> text)
        {
            Structure currentPosition = null;
            if (this.Data == null)
            {
                this.Data = new Structure();
                currentPosition = this.Data;
            //} else
            //{
            //    this.FindLastPosition(ref currentPosition);
            }

            for (int i = 0; i < text.Count(); i++)
            {
                string[] keyValue = text.ElementAt(i).Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                currentPosition.Key = keyValue[0];
                currentPosition.Value = keyValue[1];

                if (i < text.Count() - 1)
                {
                    currentPosition.Next = new Structure();
                    var newPosition = currentPosition.Next;
                    newPosition.Previous = currentPosition;
                    currentPosition = newPosition;
                }
            }
        }

        //private void FindLastPosition(ref Structure currentPosition)
        //{
        //    currentPosition = this.Data;

        //    while (!string.IsNullOrEmpty(currentPosition.Key) && (currentPosition.Next != null))
        //    {
        //        currentPosition = currentPosition.Next;
        //    }
        //}
    }
}
