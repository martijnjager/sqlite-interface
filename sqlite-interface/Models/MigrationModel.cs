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
    [Property("id")]
    [Property("migration")]
    [Property("up")]
    [Property("down")]
    [Property("batch")]
    [Property("created_at")]
    [Property("updated_at")]
    [Property("deleted_at")]
    [TableName("migrations")]
    internal class Migration : Model, IUseTimestamps
    {
        public Migration(ReadOnlyCollection<DbColumn> columns = null) : base(columns) 
        {
        }
    }
}
