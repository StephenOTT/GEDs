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
    public class SortOrderManager<T> where T : OrderBase
    {
        private IUnitOfWork unitOfWork;

        public SortOrderManager(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }

        public int GetMaxOrderIndex()
        {
            var maxStructOrder = unitOfWork.Repository<T>()
                                    .Query()
                                    .OrderBy(o => o.OrderByDescending(op => op.Order))
                                    .AsNoTracking()
                                    .Get()
                                    .Select(op => op.Order)
                                    .FirstOrDefault();

            return maxStructOrder;
        }

        public int GetMinOrderIndex()
        {
            var minStructOrder = unitOfWork.Repository<T>()
                                    .Query()
                                    .OrderBy(o => o.OrderBy(op => op.Order))
                                    .AsNoTracking()
                                    .Get()
                                    .Select(op => op.Order)
                                    .FirstOrDefault();

            return minStructOrder;
        }

        public List<SelectListItem> GenerateSelectListOrderIndexFor(int min = 1, int? max = null, int? selected = null)
        {
            int minStructOrder = min;
            int maxStructOrder = (max != null) ? (int)max : GetMaxOrderIndex();


            List<SelectListItem> orderIndexList = new List<SelectListItem>();
            if (maxStructOrder > 0)
            {
                for (int x = 1; x <= maxStructOrder; x++)
                {
                    SelectListItem index = new SelectListItem();
                    index.Text = x.ToString();
                    index.Value = x.ToString();

                    if (selected != null && selected == x)
                    {
                        index.Selected = true;
                    }

                    orderIndexList.Add(index);
                }
            }
            else
            {
                orderIndexList.Add(new SelectListItem { Selected = true, Value = "1", Text = "1" });
            }

            return orderIndexList;
        }

        //used on inserts
        public bool UpdateOrderIndex(int oldPosition, int newPosition, bool save = false)
        {
            var entities = unitOfWork.Repository<T>()
                                .Query()
                                .OrderBy(o => o.OrderBy(op => op.Order))
                                .AsNoTracking()
                                .Get()
                                .ToList();

            for (int x = oldPosition; x < newPosition; x++)
            {
                if (x < entities.Count)
                {
                    var entity = entities[x];
                    entity.Order -= 1;
                    entity.State = Entities.ObjectState.Modified;
                    unitOfWork.Repository<T>().Update(entity);
                }
            }

            if (save)
                unitOfWork.Save();

            return true;
        }

        //Used on creates
        public bool IncrementOrderIndex(int orderPosition, bool save = false)
        {
            var entities = unitOfWork.Repository<T>()
                                .Query()
                                .Filter(f => f.Order >= orderPosition)
                                .AsNoTracking()
                                .Get()
                                .ToList();

            foreach (var entity in entities)
            {
                entity.Order += 1;
                entity.State = Entities.ObjectState.Modified;
                unitOfWork.Repository<T>().Update(entity);
            }

            if (save)
                unitOfWork.Save();

            return true;
        }

        //Used on deletes
        public bool DecrementOrderIndex(int orderPosition, bool save = false)
        {
            var entities = unitOfWork.Repository<T>()
                                .Query()
                                .Filter(f => f.Order > orderPosition)
                                .AsNoTracking()
                                .Get()
                                .ToList();

            foreach (var entity in entities)
            {
                entity.Order -= 1;
                entity.State = Entities.ObjectState.Modified;
                unitOfWork.Repository<T>().Update(entity);
            }

            if (save)
                unitOfWork.Save();

            return true;
        }

        //reset
        public bool ResetOrderIndex()
        {
            var entities = unitOfWork.Repository<T>()
                                .Query()
                                .OrderBy(o => o.OrderBy(op => op.Order))
                                .AsNoTracking()
                                .Get()
                                .ToList();

            for (int x = 0; x < entities.Count; x++)
            {
                entities[x].Order = x + 1;
                entities[x].State = Entities.ObjectState.Modified;
                unitOfWork.Repository<T>().Update(entities[x]);
            }

            unitOfWork.Save();

            return true;
        }
    }
}