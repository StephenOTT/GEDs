using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Entities.Models;
using Data;
using Repository;
using System.ComponentModel.DataAnnotations;

namespace GEDs.Controllers
{
    public class SettingsController : Controller
    {
        private IUnitOfWork unitOfWork;

        public SettingsController()
        {
            unitOfWork = new UnitOfWork(new GedsContext());
        }

        //
        // GET: /Settings/

        public ActionResult Index()
        {
            var settings = unitOfWork.Repository<Setting>().Query().Get().ToList();
            return View(settings);
        }

        //
        // GET: /Settings/Details/5

        public ActionResult Details(int id)
        {
            var setting = unitOfWork.Repository<Setting>()
                            .FindById(id);
            return View(setting);
        }

        //
        // GET: /Settings/Edit/5

        public ActionResult Edit(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var setting = unitOfWork.Repository<Setting>()
                            .FindById(id);

            if (setting == null)
                return HttpNotFound();

            return View(setting);
        }

        //
        // POST: /Settings/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string Description, string Value)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            Setting setting = unitOfWork.Repository<Setting>().FindById(id);

            if (setting != null)
            {
                setting.Value = Value;
                setting.Description = Description;
                setting.State = Entities.ObjectState.Modified;

                ValidationContext vCtx = new ValidationContext(setting);
                List<ValidationResult> res = new List<ValidationResult>();
                if (Validator.TryValidateObject(setting, vCtx, res, true))
                {
                    unitOfWork.Repository<Setting>().Update(setting);
                    unitOfWork.Save();

                    return RedirectToAction("Index");
                }
                else
                {
                    return View(setting);
                }
            }

            return RedirectToAction("Index");
        }

       
    }
}
