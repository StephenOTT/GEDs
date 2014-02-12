using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Entities.Models;
using System.Web.Mvc;

namespace GEDs.ViewModel
{
    public class LogViewModel
    {
        public class LogData
        {
            public int Id { get; set; }
            public string JobGuid { get; set; }
            public string Type { get; set; }
            public string Severity { get; set; }
            public DateTime Added { get; set; }
            private string title;
            public string Title { get { return title; } set { if (value == null) title = String.Empty; else if (value.Length > 100) title = value.Substring(0, 100) + "..."; else title = value; } }
            private string message;
            public string Message { get { return message; } set { if (value == null) message = String.Empty; else if (value.Length > 100) message = value.Substring(0, 100) + "..."; else message = value; } }
        }

        public List<LogData> Logs { get; set; }
        public int LogCount { get; set; }
        public string Severity { get; set; }
        public string Type { get; set; }
        public int Page { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Guid { get; set; }
        public int DisplayCount { get; set; }

        public string ParameterUrl(string[] ignoreParamNames = null)
        {
            Dictionary<string, string> @params = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(Guid))
                @params.Add("Guid", Guid);

            if (!String.IsNullOrEmpty(Severity))
                @params.Add("Severity", Severity);

            if (!String.IsNullOrEmpty(Type))
                @params.Add("Type", Type);

            if (Page > 1)
                @params.Add("Page", Page.ToString());

            if (ignoreParamNames != null)
            {
                foreach (var paramName in ignoreParamNames)
                {
                    if (@params.ContainsKey(paramName))
                        @params.Remove(paramName);
                }
            }

            if (@params.Count > 0)
            {
                return String.Join("&", @params.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());
            }

            return "";
        }

        private List<SelectListItem> types;
        public List<SelectListItem> Types
        {
            get
            {
                if (types == null)
                {
                    Array values = Enum.GetValues(typeof(LogType));
                    types = new List<SelectListItem>();
                    foreach (var value in values)
                    {
                        SelectListItem item = new SelectListItem();
                        item.Value = value.ToString();
                        item.Text = Enum.GetName(typeof(LogType), value).ToString();

                        if (!String.IsNullOrEmpty(Type) && item.Text.Equals(Type))
                            item.Selected = true;

                        types.Add(item);
                    }

                }

                return types;
            }
        }

        private List<SelectListItem> severities;
        public List<SelectListItem> Severities
        {
            get
            {
                if (severities == null)
                {
                    Array values = Enum.GetValues(typeof(LogSeverity));
                    severities = new List<SelectListItem>();
                    foreach (var value in values)
                    {
                        SelectListItem item = new SelectListItem();
                        item.Value = value.ToString();
                        item.Text = Enum.GetName(typeof(LogSeverity), value).ToString();

                        if (!String.IsNullOrEmpty(Severity) && item.Text.Equals(Severity))
                            item.Selected = true;

                        severities.Add(item);
                    }

                }

                return severities;
            }
        }
                
    }
}