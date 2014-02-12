using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public abstract class RuleBase : OrderBase
    {
        public string Name { get; set; }

        public string RegularExpression { get; set; }

        public int RuleActionId { get; set; }
        public RuleAction RuleAction { get; set; }

        public string ReplaceValue { get; set; }
        public string ReplaceColumn { get; set; }
    }

    public abstract class Rule<T> : RuleBase where T : class
    {
        public int LookupColumnId { get; set; }
        public T LookupColumn { get; set; }

        public int ActionColumnId { get; set; }
        public T ActionColumn { get; set; }
    }

    public partial class StructureRule : Rule<Structure>
    {
    }

    public partial class ComponentRule : Rule<Component>
    {
    }
}
