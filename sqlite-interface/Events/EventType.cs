using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Events
{
    public struct EventType
    {
        public const string SAVING = "saving";
        public const string SAVED = "saved";
        public const string CREATING = "creating";
        public const string CREATED = "created";
        public const string UPDATING = "updating";
        public const string UPDATED = "updated";
        public const string DELETING = "deleting";
        public const string DELETED = "deleted";
        public const string READ = "retrieved";
    }
}
