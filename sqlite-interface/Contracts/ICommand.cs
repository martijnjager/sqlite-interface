using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts
{
    public interface ICommand
    {
        string Description { get; }
        string Signature { get; }

        /// <summary>
        /// Registers the function to call to run the command
        /// </summary>
        void Command();
    }
}
