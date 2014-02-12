using System;
using System.Diagnostics;
using System.Text;
using System.Security;
using System.Collections.Generic;
using EL = System.Diagnostics.EventLog;
using ExternalPlugin = Plugin;
using Entities.Models;
using Repository;
using Data;

namespace Service.EventLog
{
    internal class LogManager
    {
        private string sessionId;
        private Dictionary<string, Log> loggers;
        private static volatile LogManager instance;
        private static object syncRoot = new Object();

        private LogManager(string sessionId)
        {
            loggers = new Dictionary<string, Log>();
            this.sessionId = sessionId;
        }

        public static void Instantiate(string sessionId)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new LogManager(sessionId);
                }
            }
            else {
                instance.sessionId = sessionId;
                foreach (var kvp in instance.loggers)
                {
                    kvp.Value.UpdateSession(sessionId);
                }
            }
        }

        public static Log GetClassLogger(Type @class)
        {
            if (instance == null)
            {
                return new Log();
            }

            if (!instance.loggers.ContainsKey(@class.Name))
            {
                var classLog = new Log(@class.Name, instance.sessionId);
                instance.loggers.Add(@class.Name, classLog);
            }

            return instance.loggers[@class.Name];
        }
    }

    internal class Log : ExternalPlugin.IPluginLog
    {
        private string className;
        private IUnitOfWork unitOfWork;
        private string session;

        public Log()
        {
            unitOfWork = null;
        }

        public Log(string className, string session) {
            this.className = className;
            unitOfWork = new UnitOfWork(new GedsContext());
            this.session = session;
        }

        public void UpdateSession(string sessionId)
        {
            this.session = sessionId;
        }

        public void Dispose()
        {
            if (unitOfWork != null)
                unitOfWork.Dispose();
        }

        public bool Write(string title, string message, LogSeverity severity = LogSeverity.Information, LogType type = LogType.General,
            string methodName = null)
        {
            if (unitOfWork == null)
                return false;

            if (title == null)
                title = string.Empty;

            if (message == null)
                message = string.Empty;

            Entities.Models.Log entry = new Entities.Models.Log();
            entry.JobGuid = session;
            entry.Title = title;
            entry.Message = message;
            entry.MethodName = methodName;
            entry.ClassName = className;
            entry.Added = DateTime.Now;
            entry.Severity = (int)severity;
            entry.Type = (int)type;
            entry.State = Entities.ObjectState.Added;

            unitOfWork.Repository<Entities.Models.Log>().InsertGraph(entry);
            unitOfWork.Save();

            return true;
        }


        public void LogPluginCritical(string message)
        {
            Write(null, message, LogSeverity.Critical, LogType.Plugin);
        }

        public void LogPluginInformation(string message)
        {
            Write(null, message, LogSeverity.Information, LogType.Plugin);
        }

        public void LogPluginError(string message)
        {
            Write(null, message, LogSeverity.Error, LogType.Plugin);
        }

        public void LogPluginWarning(string message)
        {
            Write(null, message, LogSeverity.Warning, LogType.Plugin);
        }
    }
}
