using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public enum ContentFieldMappingType
    {
        DataReader,
        Plugin,
        String
    }

    public interface IGedUnit
    {
        string FieldName { get; set; }
        int Order { get; set; }
        bool Mandatory { get; set; }
        int MaxLength { get; set; }
        bool Ignore { get; set; }
        ContentFieldMappingType Type { get; set; }
        string DefaultValue { get; set; }
        string Source { get; set; }
        string Validation { get; set; }

        void Dispose();
    }
}
