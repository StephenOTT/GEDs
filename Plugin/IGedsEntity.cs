using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public interface IGedsEntity
    {
        string Value { get; set; }
        IGedUnit GedsUnit { get; set; }

        string ApplyGedsRule();
        bool ValidateEntity();
        bool SkipEntity();
        void Dispose();
    }
}
