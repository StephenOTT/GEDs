using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin;

namespace Service.DataSets
{
    internal class GedsRepository : IGedsRepository
    {

        public GedsRepository()
        {
            Structures = new GedsProperty();
            Components = new GedsProperty();

            StructureEntities = new List<Dictionary<string, IGedsEntity>>();
            ComponentEntities = new List<Dictionary<string, IGedsEntity>>();
        }

        #region IGedsRepository Members

        public IGedsProperty Structures { get; set; }
        public IGedsProperty Components { get; set; }

        public List<Dictionary<string, IGedsEntity>> StructureEntities { get; set; }
        public List<Dictionary<string, IGedsEntity>> ComponentEntities { get; set; }

        public void Dispose()
        {
            if (Structures != null)
                Structures.Dispose();

            if (Components != null)
                Components.Dispose();

            if (StructureEntities != null)
            {
                foreach (var keyValuePair in StructureEntities)
                {
                    foreach (var kvp in keyValuePair)
                    {
                        kvp.Value.Dispose();
                    }
                }
            }

            if (ComponentEntities != null)
            {
                foreach (var keyValuePair in ComponentEntities)
                {
                    foreach (var kvp in keyValuePair)
                    {
                        kvp.Value.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}
