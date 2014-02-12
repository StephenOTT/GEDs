using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin;

namespace Service.DataSets
{
    internal interface IGedsRepository
    {
        IGedsProperty Structures { get; set; }
        IGedsProperty Components { get; set; }

        List<Dictionary<string, IGedsEntity>> StructureEntities { get; set; }
        List<Dictionary<string, IGedsEntity>> ComponentEntities { get; set; }

        void Dispose();
    }
}
