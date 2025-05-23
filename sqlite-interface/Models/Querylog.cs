using Database;
using Database.Extensions;
using Database.Extensions.Model.Attribute;

namespace Backend.Database.Models
{
    [TableName("query_logs")]
    public class Querylog : Model, IUseTimestamps
    {
        public Querylog() : base()
        {
        }
    }
}
