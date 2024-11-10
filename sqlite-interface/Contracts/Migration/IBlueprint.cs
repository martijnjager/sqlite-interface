using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Contracts.Migration
{
    public interface IBlueprint
    {
        IBlueprint PrimaryKey(string? id = null);
        IBlueprint String(string column);
        IBlueprint Text(string column);
        IBlueprint Int(string column);
        IBlueprint Timestamps();
        IBlueprint SoftDeletes();
        IBlueprint Table(string table);
    }
}
