using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public abstract class Base : IObjectState
    {
        public int Id { get; set; }
        public ObjectState State { get; set; }
    }
}
