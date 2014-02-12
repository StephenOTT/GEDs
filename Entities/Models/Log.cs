using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public enum LogType
    {
        General = 100,
        DataReader,
        Plugin,
        Structure,
        Component
    }

    public enum LogSeverity
    {
        Information = 200,
        Warning,
        Error,
        Critical
    }

    public partial class Log : Base
    {

        public string JobGuid { get; set; }

        public int Type { get; set; }
        public int Severity { get; set; }

        public DateTime Added { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        public string ClassName { get; set; }
        public string MethodName { get; set; }
    }
}
