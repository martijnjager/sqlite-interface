using Database.Contracts;
using Database.Extensions.Model;
using System.Data.Common;
using System.Data.SQLite;
using System.Text.Json.Serialization;

namespace Database
{
    public sealed class QueryResult<SaveStatus>
    {
        [JsonPropertyName("status")]
        public SaveStatus Status { get; private set; } = default!;

        [JsonPropertyName("data")]
        public object Data { get; private set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; private set; } = string.Empty;

        [JsonIgnore]
        public DbDataReader? Reader { get; private set; }

        public QueryResult() { }

        public QueryResult<SaveStatus> SetStatus(SaveStatus status) { Status = status; return this; }

        public QueryResult<SaveStatus> SetData(object data)
        {
            if (data is IModel model)
            {
                Data = model.ToExpando();
            }

            return this; 
        }

        public QueryResult<SaveStatus> SetMessage(string message) { Message = message; return this; }

        public QueryResult<SaveStatus> SetReader(DbDataReader? reader) { Reader = reader; return this; }

        public static QueryResult<SaveStatus> Create(object data, SaveStatus status, string message = "")
        {
            if (data is IModel model)
            {
                data = model.ToExpando();
            }

            return new QueryResult<SaveStatus>
            {
                Status = status,
                Data = data,
                Message = message
            };
        }
    }
}
