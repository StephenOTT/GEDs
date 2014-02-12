using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public abstract class OrderBase : Base
    {        
        public int Order { get; set; }
    }
}
