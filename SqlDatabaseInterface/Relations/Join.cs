using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Relations
{
    public class Join
    {
        public string MainTable { get; private set; }
        public string Table { get; private set; }
        public string Type { get; private set; }
        public string ForeignId { get; private set; }
        public string Id { get; private set; }

        public string Direction { get; private set; }

        public Join(string model, string table, string id, string foreignId, string direction)
        {
            MainTable = model;
            Table = table;
            Type = "left";
            Id = id;
            ForeignId = foreignId;
            Direction = direction;
        }
    }
}
