using Database.Extensions.Model.Attribute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Tests.Models
{
    [TableName("tests")]
    public class TestModel : Model
    {
        public TestModel(ReadOnlyCollection<DbColumn> columns = null) : base(columns)
        {

        }
    }
}
