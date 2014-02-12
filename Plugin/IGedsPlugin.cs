using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public interface IGedsPlugin
    {
        //string Execute(IGedUnit field, List<IGedsEntity> populatedFields);
        //string Execute(int dataReaderIndex, IDataReader dataReader, IGedUnit field);
        IPluginLog LogInstance { get; set; }
        string Execute(string fieldNameOfPlugin, Dictionary<string, IGedsEntity> entityRow);
        void Dispose();
    }
}
