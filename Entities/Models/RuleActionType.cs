using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public enum RuleActionType
    {
        Replace,
        ReplaceWithMatch,
        Delete
    }

    public partial class RuleAction : LookupTable<RuleActionType>
    {
        
    }
}
