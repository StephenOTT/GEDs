using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Entities.Models;
using Data;
using GEDs.Helpers;

namespace GEDs.ViewModel
{
    public class HomeViewModel
    {
        public IEnumerable<Job> Jobs { get; set; }
        public bool ServiceStatus { get; set; }
    }

    public class DemoViewModel
    {
        public string Guid { get; set; }

        public List<GEDs.Helpers.Structure> Parents { get; set; }
        public List<GEDs.Helpers.Structure> Organizations { get; set; }
        public List<GEDs.Helpers.Component> People { get; set; }

        public GEDs.Helpers.Component User { get; set; }
        public GEDs.Helpers.Structure Organization { get; set; }
    }
       
}