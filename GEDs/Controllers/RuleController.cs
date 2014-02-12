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
    public abstract class RuleController<T, K> : Controller
        where K : ContentFieldTable
        where T : Rule<K>
    {
        protected IUnitOfWork unitOfWork;
        protected SortOrderManager<T> sortOrderManager;

        public virtual ActionResult Index()
        {
            var entities = unitOfWork.Repository<T>()
                            .Query()
                            .Include(i => i.LookupColumn)
                            .Include(i => i.ActionColumn)
                            .OrderBy(o => o.OrderBy(or => or.Order))
                            .Get()
                            .ToList();

            return View(entities);
        }

        public virtual ActionResult Create()
        {
            if(!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var lookupColumnList = GenerateHTMLListFieldList();
            var actionColumnList = GenerateHTMLListRuleAction();
            var replaceColumnList = GenerateHTMLListFieldList(null, true, null);
            int maxOrderIndex = sortOrderManager.GetMaxOrderIndex();

            ViewBag.LookupColumnList = lookupColumnList;
            ViewBag.ActionColumnList = actionColumnList;
            ViewBag.ReplaceColumnList = replaceColumnList;
            ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrderIndex + 1, maxOrderIndex + 1);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(T entity)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    sortOrderManager.IncrementOrderIndex(entity.Order);

                    entity.State = Entities.ObjectState.Added;
                    unitOfWork.Repository<T>().InsertGraph(entity);
                    unitOfWork.Save();

                    return RedirectToAction("Index");
                }

                var lookupColumnList = GenerateHTMLListFieldList();
                var actionColumnList = GenerateHTMLListRuleAction();
                var replaceColumnList = GenerateHTMLListFieldList(null, true, entity.ReplaceColumn);
                int maxOrderIndex = sortOrderManager.GetMaxOrderIndex();

                ViewBag.LookupColumnList = lookupColumnList;
                ViewBag.ActionColumnList = actionColumnList;
                ViewBag.ReplaceColumnList = replaceColumnList;
                ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrderIndex + 1, maxOrderIndex + 1);

                return View(entity);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            return RedirectToAction("Index");
        }

        public virtual ActionResult Details(int id)
        {
            var entity = unitOfWork.Repository<T>()
                            .Query()
                            .Include(i => i.ActionColumn)
                            .Include(i => i.LookupColumn)
                            .Filter(f => f.Id == id)
                            .Get()
                            .FirstOrDefault();

            if (entity != null)
            {
                ViewBag.LookupColumnName = entity.LookupColumn.Field;
                ViewBag.ActionColumnName = entity.ActionColumn.Field;

                return View(entity);
            }

            return HttpNotFound();
        }

        public virtual ActionResult Delete(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>().FindById(id);

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(T entity)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            if (entity != null)
            {
                entity.State = Entities.ObjectState.Deleted;
                unitOfWork.Repository<T>().Delete(entity);
                unitOfWork.Save();
            }

            return RedirectToAction("Index");
        }

        public virtual ActionResult Edit(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>().FindById(id);

            var lookupColumnList = GenerateHTMLListFieldList(entity.LookupColumnId);
            var actionColumnList = GenerateHTMLListFieldList(entity.ActionColumnId);
            var replaceColumnList = GenerateHTMLListFieldList(null, true, entity.ReplaceColumn);
            var actionTypeList = GenerateHTMLListRuleAction();

            ViewBag.LookupColumnList = lookupColumnList;
            ViewBag.ActionColumnList = actionColumnList;
            ViewBag.ReplaceColumnList = replaceColumnList;
            ViewBag.ActionTypeList = actionTypeList;
            ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, null, entity.Order);

            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(T entity)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    var oldEntity = unitOfWork.Repository<T>()
                                        .Query()
                                        .AsNoTracking()
                                        .Filter( f=> f.Id == entity.Id)
                                        .Get()
                                        .FirstOrDefault();

                    if (oldEntity != null)
                    {
                        sortOrderManager.UpdateOrderIndex(oldEntity.Order, entity.Order);
                    }

                    entity.State = Entities.ObjectState.Modified;
                    unitOfWork.Repository<T>().Update(entity);
                    unitOfWork.Save();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            var lookupColumnList = GenerateHTMLListFieldList(entity.LookupColumnId);
            var actionColumnList = GenerateHTMLListFieldList(entity.ActionColumnId);
            var replaceColumnList = GenerateHTMLListFieldList(null, true, entity.ReplaceColumn);
            var actionTypeList = GenerateHTMLListRuleAction();

            ViewBag.LookupColumnList = lookupColumnList;
            ViewBag.ActionColumnList = actionColumnList;
            ViewBag.ReplaceColumnList = replaceColumnList;
            ViewBag.ActionTypeList = actionTypeList;
            ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, null, entity.Order);

            return View(entity);
        }

        public virtual ActionResult Reset() {
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
                                    Name = s.Name,
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

            model.RouteActionControl = typeof(T).Name;
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

        protected override void Dispose(bool disposing)
        {
            if (unitOfWork != null)
                unitOfWork.Dispose();
            
            base.Dispose(disposing);
        }

        private List<SelectListItem> GenerateHTMLListRuleAction(int? selected = null)
        {
            var entities = unitOfWork.Repository<RuleAction>().Query().Get();
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var entity in entities)
            {
                SelectListItem item = new SelectListItem();

                item.Text = entity.Value;
                item.Value = entity.Id.ToString();

                if (selected != null && (int)selected == entity.Id)
                    item.Selected = true;

                selectList.Add(item);
            }

            return selectList;
        }

        private List<SelectListItem> GenerateHTMLListFieldList(int? selected = null, bool valueAsName = false, string valueSelected = null)
        {
            var entities = unitOfWork.Repository<K>()
                                .Query()
                                .OrderBy(o => o.OrderBy(or => or.Field))
                                .Get()
                                .ToList();

            List<SelectListItem> selectList = new List<SelectListItem>();
            foreach (var entity in entities)
            {
                SelectListItem item = new SelectListItem();

                item.Text = entity.Field;
                item.Value = (valueAsName) ? entity.Field : entity.Id.ToString();

                if (!valueAsName)
                {
                    if (selected != null && (int)selected == entity.Id)
                        item.Selected = true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(valueSelected) && valueSelected.Equals(entity.Field))
                        item.Selected = true;
                }

                selectList.Add(item);
            }

            return selectList;
        }
    }
}