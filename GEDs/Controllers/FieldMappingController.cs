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
    public abstract class FieldMappingController<T> : Controller where T : ContentFieldTable
    {
        protected IUnitOfWork unitOfWork;
        protected SortOrderManager<T> sortOrderManager;

        //
        // GET: /StructureFieldMapping/

        public virtual ActionResult Index()
        {
            var fields = unitOfWork.Repository<T>()
                            .Query()
                            .OrderBy(o => o.OrderBy(op => op.Order))
                            .Include(i => i.ContentFieldMapping)
                            .Get()
                            .ToList();

            return View(fields);
        }

        //
        // GET: /StructureFieldMapping/Details/5

        public virtual ActionResult Details(int id)
        {
            var fields = unitOfWork.Repository<T>()
                            .Query()
                            .Include(i => i.ContentFieldMapping)
                            .Get()
                            .FirstOrDefault();
            return View(fields);
        }

        //
        // GET: /StructureFieldMapping/Create

        public virtual ActionResult Create()
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var maxOrderIndex = sortOrderManager.GetMaxOrderIndex();
            ViewBag.FieldType = GenerateHTMLSelectFieldType(null);
            ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrderIndex + 1, maxOrderIndex + 1);
            ViewBag.MaxLength = GenerateHTMLSelectMaxLength();

            return View();
        }

        //
        // POST: /StructureFieldMapping/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(T entity)
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

                return View();
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            ViewBag.FieldType = GenerateHTMLSelectFieldType(null);
            int maxOrderIndex = sortOrderManager.GetMaxOrderIndex();
            ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, maxOrderIndex + 1, maxOrderIndex + 1);
            ViewBag.MaxLength = GenerateHTMLSelectMaxLength(entity.MaxLength);

            return View(entity);
        }

        //
        // GET: /StructureFieldMapping/Edit/5

        public virtual ActionResult Edit(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>().FindById(id);
            ViewBag.FieldType = GenerateHTMLSelectFieldType(entity.ContentFieldMappingId);
            ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, null, entity.Order);
            ViewBag.MaxLength = GenerateHTMLSelectMaxLength(entity.MaxLength);

            return View(entity);
        }

        //
        // POST: /StructureFieldMapping/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(T entity)
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
                                        .Filter(f => f.Id == entity.Id)
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

                ViewBag.FieldType = GenerateHTMLSelectFieldType(entity.ContentFieldMappingId);
                ViewBag.OrderIndex = sortOrderManager.GenerateSelectListOrderIndexFor(1, null, entity.Order);
                ViewBag.MaxLength = GenerateHTMLSelectMaxLength(entity.MaxLength);

                return View(entity);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /StructureFieldMapping/Delete/5

        public virtual ActionResult Delete(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            var entity = unitOfWork.Repository<T>()
                            .FindById(id);

            return View(entity);
        }

        //
        // POST: /StructureFieldMapping/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, FormCollection collection)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            try
            {
                var entity = unitOfWork.Repository<T>()
                            .FindById(id);

                if (entity != null)
                {
                    sortOrderManager.DecrementOrderIndex(entity.Order);
                    entity.State = Entities.ObjectState.Deleted;
                    unitOfWork.Repository<T>().Delete(entity);
                    unitOfWork.Save();
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
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
                                .Select(s => new SortingViewModel.SortingView {
                                    Id = s.Id,
                                    Name = s.Field,
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

            model.RouteActionControl = typeof(T).Name + "FieldMapping";
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

        private List<SelectListItem> GenerateHTMLSelectFieldType(int? selected = null)
        {
            var entities = unitOfWork.Repository<ContentFieldMapping>()
                            .Query()
                            .Get()
                            .ToList();

            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var entity in entities)
            {
                SelectListItem item = new SelectListItem();
                item.Text = entity.Value;
                item.Value = entity.Id.ToString();

                if (selected != null)
                {
                    if (entity.Id == (int)selected)
                        item.Selected = true;
                }

                selectList.Add(item);
            }

            return selectList;
        }

        private List<SelectListItem> GenerateHTMLSelectMaxLength(int? selected = null)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            for (int x = 1; x <= 500; x++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = x.ToString();
                item.Value = x.ToString();

                if (selected != null && (int)selected == x)
                    item.Selected = true;

                selectList.Add(item);
            }

            return selectList;
        }
    }
}