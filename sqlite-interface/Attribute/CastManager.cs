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
            this.Casts = new List<Tuple<string, System.Type>>();
        }

        /// <summary>
        /// Casts for the attributes.
        /// </summary>
        public List<Tuple<string, System.Type>> Casts { get; private set; }

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
            if (this.Casts is null)
            {
                this.Casts = casts;
            }
            else
            {
                var newCasts = casts.FindAll(x => !this.Casts.Contains(x));
                this.Casts.AddRange(newCasts);
            }
        }
    }
}
