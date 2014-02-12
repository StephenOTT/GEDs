using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using GEDs.Helpers;
using GEDs.Enums;
using Entities.Models;

namespace GEDs.ViewModel
{
    public class SortingViewModel
    {
        public class SortingView
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string Name { get; set; }
        }

        public List<SortingView> SortList { get; set; }

        public List<SelectListItem> OrderIndex { get; set; }

        public string BackLink { get; set; }
        public string RouteActionName { get; set; }
        public string RouteActionControl { get; set; }
    }
}