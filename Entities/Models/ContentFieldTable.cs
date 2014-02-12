using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public abstract class ContentFieldTable : OrderBase
    {
        public string Field { get; set; }
        public string Source { get; set; }
        public bool Skip { get; set; }

        [Range(1, 500)]
        public int MaxLength { get; set; }
        public bool Mandatory { get; set; }
        public string Default { get; set; }
        public string Validation { get; set; }

        public int ContentFieldMappingId { get; set; }
        public virtual ContentFieldMapping ContentFieldMapping { get; set; }
    }

    public class Structure : ContentFieldTable
    {
    }

    public class Component : ContentFieldTable
    {
        //public int ContentFieldMappingId { get; set; }
        //public virtual ContentFieldMapping ContentFieldMapping { get; set; }
    }
}
