using Database;
using Database.Extensions.Model.Attribute;

namespace Backend.Database.Models
{
    [TableName("query_logs")]
    public class Querylog : Model
    {
        public Querylog() : base()
        {
            this.Attributes.EnableTimestamps();
        }
    }
}
