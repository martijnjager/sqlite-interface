using Database;
using Database.Extensions;
using Database.Extensions.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    [TableName("migrations")]
    internal class Migration : Model, IUseTimestamps
    {
        public Migration(ReadOnlyCollection<DbColumn> columns = null) : base(columns) 
        {
        }
    }
}
