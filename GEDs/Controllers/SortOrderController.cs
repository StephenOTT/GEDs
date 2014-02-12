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
    public abstract class SortOrderController<T> : Controller where T : OrderPriority
    {
        protected IUnitOfWork unitOfWork;
        protected SortOrderManager<T> sortOrderManager;

        public virtual ActionResult Index()
        {
            var entities = unitOfWork.Repository<T>()
                                    .Query()
                                    .OrderBy(o => o.OrderBy(sop => sop.Order))
                                    .Get()
                                    .ToList();

            return View(entities);
        }

        //
        // GET: /StructureSortOrder/Create

        public virtual ActionResult Create()
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var maxOrder = sortOrderManager.GetMaxOrderIndex();
            List<SelectListItem> orderIndexList = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrder + 1, maxOrder + 1);
            ViewBag.OrderIndexList = orderIndexList;

            return View();
        }

        //
        // POST: /StructureSortOrder/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(T entity)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                sortOrderManager.IncrementOrderIndex(entity.Order, false);

                entity.State = Entities.ObjectState.Added;
                unitOfWork.Repository<T>().InsertGraph(entity);

                unitOfWork.Save();

                return RedirectToAction("Index");
            }

            var maxOrder = sortOrderManager.GetMaxOrderIndex();
            List<SelectListItem> orderIndexList = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrder + 1, maxOrder + 1);
            ViewBag.OrderIndexList = orderIndexList;

            return View(entity);
        }

        //
        // GET: /StructureSortOrder/Edit/5

        public virtual ActionResult Edit(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>()
                                    .FindById(id);

            if (entity == null)
                return HttpNotFound();

            var maxOrder = sortOrderManager.GetMaxOrderIndex();
            List<SelectListItem> orderIndexList = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrder, entity.Order);

            ViewBag.OrderIndexList = orderIndexList;

            return View(entity);
        }

        //
        // POST: /StructureSortOrder/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(T entity)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                int oldOrder = unitOfWork.Repository<T>()
                                    .Query()
                                    .Filter(cop => cop.Id == entity.Id)
                                    .AsNoTracking()
                                    .Get()
                                    .Select(cop => cop.Order)
                                    .FirstOrDefault();

                sortOrderManager.UpdateOrderIndex(oldOrder, entity.Order, false);

                entity.State = Entities.ObjectState.Modified;
                unitOfWork.Repository<T>().Update(entity);
                unitOfWork.Save();

                //ModelState.Clear();

                return RedirectToAction("Index");
            }

            var maxOrder = sortOrderManager.GetMaxOrderIndex();
            List<SelectListItem> orderIndexList = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrder, entity.Order);
            ViewBag.OrderIndexList = orderIndexList;

            return View(entity);
        }

        //
        // GET: /Profile/Delete/5

        public virtual ActionResult Delete(int id = 0)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>().FindById(id);
            if (entity == null)
            {
                return HttpNotFound();
            }
            return View(entity);
        }

        //
        // POST: /Profile/Delete/5

        [HttpPost, ActionName("Delete")]
        public virtual ActionResult DeleteConfirmed(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>().FindById(id);
            if (entity != null)
            {
                sortOrderManager.DecrementOrderIndex(entity.Order);

                entity.State = Entities.ObjectState.Deleted;
                unitOfWork.Repository<T>().Delete(entity);
                unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }

        public virtual ActionResult Reset()
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            sortOrderManager.ResetOrderIndex();
            return RedirectToAction("Index");
        }

        public virtual ActionResult Sort()
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var model = new SortingViewModel();

            model.SortList = unitOfWork.Repository<T>()
                                .Query()
                                .AsNoTracking()
                                .OrderBy(o => o.OrderBy(ob => ob.Order))
                                .Get()
                                .Select(s => new SortingViewModel.SortingView
                                {
                                    Id = s.Id,
                                    Name = s.Value,
                                    Order = s.Order
                                })
                                .ToList();

            model.OrderIndex = model.SortList
                                    .Select(s => new SelectListItem
                                    {
                                        Selected = false,
                                        Text = s.Order.ToString(),
                                        Value = s.Order.ToString()
                                    })
                                    .ToList();

            model.RouteActionControl = "StructureSortOrder";
            model.RouteActionName = "Sort";

            return View("~/Views/Sorting/Sort.cshtml", model);
        }

        [HttpPost]
        public virtual ActionResult Sort(FormCollection form)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            if (form == null)
                return HttpNotFound();

            bool hasChanges = false;
            var entities = unitOfWork.Repository<T>().Query().Get().ToList();

            foreach (var id in form.AllKeys)
            {
                int entityId = 0;
                if (Int32.TryParse(id, out entityId))
                {
                    var entity = entities
                                    .Where(e => e.Id == entityId)
                                    .FirstOrDefault();

                    if (entity != null)
                    {
                        int newOrderPos = -1;
                        if (Int32.TryParse(form[id], out newOrderPos) && entity.Order != newOrderPos)
                        {
                            entity.Order = newOrderPos;
                            entity.State = Entities.ObjectState.Modified;
                            unitOfWork.Repository<T>().Update(entity);
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges)
                unitOfWork.Save();

            return RedirectToAction("Index");
        }
    }
}