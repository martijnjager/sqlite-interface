﻿using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    internal class MigrationModel : Model
    {
        public MigrationModel(): base("migrations") 
        {
            this.timestamps = true;
        }
    }
}