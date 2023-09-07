using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Config
{
    internal class Structure
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Structure Next { get; set; }

        //public Structure Child { get; set; }

        //public Structure Parent { get; set; }

        public Structure Previous { get; set; }

        public Structure() { }
    }
}
