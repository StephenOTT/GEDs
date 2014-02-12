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
    public class StructureRuleController : RuleController<StructureRule, Entities.Models.Structure>
    {
        public StructureRuleController()
        {
            unitOfWork = new UnitOfWork(new GedsContext());
            sortOrderManager = new SortOrderManager<StructureRule>(unitOfWork);
        }

        //
        // GET: /StructureRule/

        public override ActionResult Index()
        {
            return base.Index();
        }

        public override ActionResult Details(int id)
        {
            return base.Details(id);
        }

        public override ActionResult Create()
        {
            return base.Create();
        }

        public override ActionResult Edit(int id)
        {
            return base.Edit(id);
        }

        public override ActionResult Delete(int id)
        {
            return base.Delete(id);
        }

    }
}
