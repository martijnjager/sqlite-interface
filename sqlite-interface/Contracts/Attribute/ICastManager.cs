using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Attribute
{
    public interface ICastManager : ITimestampManager
    {
        void AddTimestampCasts();
        void AddCasts(List<Tuple<string, System.Type>> casts);

        List<Tuple<string, System.Type>> casts { get; }
    }
}
