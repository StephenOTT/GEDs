using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin;

namespace Service.DataSets
{
    internal class GedsProperty : IGedsProperty
    {
        #region IGedsRepository Members

        public Dictionary<string, IGedUnit> Properties { get; set; }

        public GedsProperty()
        {
            Properties = new Dictionary<string, IGedUnit>();
        }

        public IEnumerable<IGedUnit> GedUnits()
        {
            foreach (KeyValuePair<string, IGedUnit> kvP in Properties)
            {
                yield return kvP.Value;
            }
        }

        public List<string> AllKeys()
        {
            return Properties.Keys.ToList();
        }

        public bool HasProperty(string property)
        {
            if (String.IsNullOrEmpty(property.Trim()))
                return false;

            return Properties.Keys.Contains(property);
        }

        public bool IgnorePropery(string property)
        {
            if (String.IsNullOrEmpty(property.Trim()))
                return true;

            var gedUnit = Properties[property];
            if (gedUnit != null)
            {
                return gedUnit.Ignore;
            }
            return false;
        }

        public bool MandatoryProperty(string property)
        {
            if (String.IsNullOrEmpty(property.Trim()))
                return true;

            var gedUnit = Properties[property];
            if (gedUnit != null)
            {
                return gedUnit.Mandatory;
            }
            return false;
        }

        public IGedUnit GetGedUnit(string property)
        {
            if (String.IsNullOrEmpty(property.Trim()))
                return null;

            return Properties[property];
        }

        public void Dispose()
        {
            if (Properties != null)
            {
                foreach (var kvp in Properties)
                {
                    kvp.Value.Dispose();
                }
            }
        }

        #endregion
        
    }
}
