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
    public class ComponentFieldMappingController : FieldMappingController<Entities.Models.Component>
    {
        public ComponentFieldMappingController()
        {
            unitOfWork = new UnitOfWork(new GedsContext());
            sortOrderManager = new SortOrderManager<Entities.Models.Component>(unitOfWork);
        }

        public override ActionResult Index()
        {
            return base.Index();
        }

        public override ActionResult Create()
        {
            return base.Create();
        }

        public override ActionResult Details(int id)
        {
            return base.Details(id);
        }

        public override ActionResult Edit(int id)
        {
            return base.Edit(id);
        }

        public override ActionResult Delete(int id)
        {
            return base.Delete(id);
        }

        public override ActionResult Sort()
        {
            return base.Sort();
        }

        public override ActionResult Sort(FormCollection form)
        {
            return base.Sort(form);
        }
    }
}
