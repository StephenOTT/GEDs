using System;
using System.Collections.Generic;
using Plugin;

namespace Entities.Models
{
    public abstract class LookupTable<T> : IObjectState
    {
        public int Id { get; set; }

        public string Value { get; set; }
        public T ValueCode { get; set; }

        public ObjectState State { get; set; }
    }

    public class ContentFieldMapping : LookupTable<ContentFieldMappingType>
    {
    }
     
}
