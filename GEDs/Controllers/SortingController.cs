using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Entities.Models;
using Repository;
using Data;
using GEDs.Enums;
using GEDs.Helpers;
using GEDs.ViewModel;

namespace GEDs.Controllers
{

    public class SortingController : Controller
    {
        public SortingController()
        {
        }

        //
        // GET: /Sorting/

        public ActionResult Index()
        {
            return View();
        }

    }
}
