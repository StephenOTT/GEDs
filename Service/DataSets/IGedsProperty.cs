using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin;

namespace Service.DataSets
{
    internal interface IGedsProperty
    {
        Dictionary<string, IGedUnit> Properties { get; set; }

        IEnumerable<IGedUnit> GedUnits();
        List<string> AllKeys();
        bool HasProperty(string property);
        bool IgnorePropery(string property);
        bool MandatoryProperty(string property);
        IGedUnit GetGedUnit(string property);

        void Dispose();
    }
}
