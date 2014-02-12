using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public abstract class OrderPriority : OrderBase
    {
        public string Value { get; set; }
    }

    public class StructureOrderPriority : OrderPriority
    {
    }

    public class ComponentOrderPriority : OrderPriority
    {
    }
}
