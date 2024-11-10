using System.Data.SQLite;

namespace Database
{
    public sealed class QueryResult<SaveStatus>
    {
        public SaveStatus Status { get; private set; }

        public object Data { get; private set; }

        public string Message { get; private set; }

        public SQLiteDataReader? Reader { get; private set; }

        public QueryResult() { }

        public void SetStatus(SaveStatus status) { Status = status; }

        public void SetData(object data) { Data = data; }

        public void SetMessage(string message) { Message = message; }

        public void SetReader(SQLiteDataReader? reader) { Reader = reader; }
    }
}
