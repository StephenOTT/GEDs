using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Job : Base
    {
        public string Guid { get; set; }
        public bool Status { get; set; }

        public DateTime JobStarted { get; set; }
        public DateTime JobCompleted { get; set ;}
        
        public string StructureFileLocation { get; set; }
        public string ComponentFileLocation { get; set; }

        public DateTime? DataSentOn { get; set; }
        public string SentTo { get; set; }
    }
}
