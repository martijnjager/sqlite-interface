using Database.Contracts.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Attribute
{
    public class CastManager : TimestampManager, ICastManager
    {
        public CastManager()
        {

        }

        /// <summary>
        /// Casts for the attributes.
        /// </summary>
        public List<Tuple<string, System.Type>> casts { get; private set; }

        public void AddTimestampCasts()
        {
            this.AddCasts(new List<Tuple<string, System.Type>>()
            {
                new(CREATED_AT, typeof(DateTime)),
                new(UPDATED_AT, typeof(DateTime)),
                new(DELETED_AT, typeof(DateTime)),
            });
        }

        public void AddCasts(List<Tuple<string, System.Type>> casts)
        {
            if (this.casts is null)
            {
                this.casts = casts;
            }
            else
            {
                var newCasts = casts.FindAll(x => !this.casts.Contains(x));
                this.casts.AddRange(newCasts);
            }
        }
    }
}
