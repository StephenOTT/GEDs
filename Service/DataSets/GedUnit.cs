using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;

using Plugin;

namespace Service.DataSets
{
    internal class GedUnit : IGedUnit
    {
        public string FieldName { get; set; }
        public int Order { get; set; }
        public bool Mandatory { get; set; }
        public int MaxLength { get; set; }
        public bool Ignore { get; set; }
        public ContentFieldMappingType Type { get; set; }
        public string DefaultValue { get; set; }
        public string Source { get; set; }
        public string Validation { get; set; }

        public GedUnit()
        {
            Order = 0;
            FieldName = String.Empty;
            Mandatory = false;
            Ignore = false;
            MaxLength = 0;
            Type = ContentFieldMappingType.String;
            DefaultValue = " ";
            Source = "";
        }

        public void Dispose()
        {

        }
    }
}
