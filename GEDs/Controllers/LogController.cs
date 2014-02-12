using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Data;
using Entities.Models;
using Repository;
using System.Linq.Expressions;
using GEDs.ViewModel;

namespace GEDs.Controllers
{
    public class LogController : Controller
    {
        private IUnitOfWork unitOfWork;
        private int DisplayCount = 50;

        public LogController()
        {
            unitOfWork = new UnitOfWork(new GedsContext());
        }

        public ActionResult Index(
            string Guid = null, 
            int? Page = null, 
            string Severity = null,
            string Type = null,
            string Title = null,
            string Message = null)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            RepositoryQuery<Log> query = unitOfWork.Repository<Log>().Query();
            LogViewModel model = new LogViewModel();

            model.DisplayCount = DisplayCount;
            
            if (!string.IsNullOrEmpty(Guid))
            {
                query = query.Filter(i => i.JobGuid == Guid);
                model.Guid = Guid;
            }

            if (!string.IsNullOrEmpty(Severity))
            {
                LogSeverity severity;
                if (Enum.TryParse(Severity, out severity))
                {
                    query = query.Filter(i => i.Severity == (int)severity);
                    model.Severity = severity.ToString();
                }
            }

            if (!string.IsNullOrEmpty(Type))
            {
                LogType type;
                if (Enum.TryParse(Type, out type))
                {
                    query = query.Filter(i => i.Type == (int)type);
                    model.Type = type.ToString();
                }
            }

            if (!string.IsNullOrEmpty(Title))
            {
                query = query.Filter(i => i.Title.Contains(Title));
                model.Title = Title;
            }

            if (!string.IsNullOrEmpty(Message))
            {
                query = query.Filter(i => i.Message.Contains(Message));
                model.Message = Message;
            }

            if (Page == null)
                Page = 1;

            int logTotalCount;
            model.Logs = query
                            .OrderBy(o => o.OrderBy(ob => ob.Added))
                            .GetPage((int)Page, DisplayCount, out logTotalCount)
                            .Select(s => new GEDs.ViewModel.LogViewModel.LogData {
                                Id = s.Id,
                                Added = s.Added,
                                JobGuid = s.JobGuid,
                                Message = s.Message,
                                Title = s.Title,
                                Severity = ConvertSeverityIndexToString(s.Severity.ToString()),
                                Type = ConvertTypeIndexToString(s.Type.ToString())
                            })
                            .ToList();
            model.LogCount = logTotalCount;
            model.Page = (int)Page;

            return View(model);
        }

        //
        // GET: /Log/Details/5

        public ActionResult Details(int id)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            Log log = unitOfWork.Repository<Log>().FindById(id);

            if (log != null)
            {
                ViewBag.Type = ConvertTypeIndexToString(log.Type.ToString());
                ViewBag.Severity = ConvertSeverityIndexToString(log.Severity.ToString());

                return View(log);
            }
            return HttpNotFound();
        }

        protected override void Dispose(bool disposing)
        {
            if (unitOfWork != null)
                unitOfWork.Dispose();

            base.Dispose(disposing);
        }

        public string ConvertSeverityIndexToString(string index)
        {
            LogSeverity severity;
            if(Enum.TryParse(index, out severity))
                return severity.ToString();
            return "Unknown";
        }

        public string ConvertTypeIndexToString(string index)
        {
            LogType type;
            if(Enum.TryParse(index, out type))
                return type.ToString();
            return "Unknown";
        }
    }
}
