using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entities.Models;

namespace Repository
{
    public static class SettingsRepository
    {
        public enum SettingTypes
        {
            CONNECTION_STRING,
            GEDS_EMAIL_TO,
            GEDS_EMAIL_CC,
            GEDS_EMAIL_FROM,
            GEDS_EMAIL_SUBJECT,
            GEDS_EMAIL_BODY,
            GEDS_AUTO_JOB_SECS,
            GEDS_HEADER_SOURCE_AGENT_NAME,
            GEDS_HEADER_DEPARTMENTAL_ACRONYM,
            GED_MANDATORYKEY_MISSING_JUNK_DATA,
            GEDS_SORTING_LOOKUP_COLUMN_STRUCTURE,
            GEDS_SORTING_LOOKUP_COMPARE_STRUCTURE,
            GEDS_SORTING_LOOKUP_COLUMN_COMPONENT,
            GEDS_SORTING_LOOKUP_COMPARE_COMPONENT,
            APP_EMAIL_CRITICAL_ISSUE,
            APP_LOCATION_CDF,
            APP_EMAIL_CRITICAL_ISSUE_SUBJECT,
            APP_EMAIL_AUTOSEND_GEDS,
            APP_GEDS_OUTPUT_LOCATION,
            APP_SMTP_SERVER,
            APP_PLUGIN_LOCATION
        }

        public static string GetSettingValue(this IRepository<Setting> context, SettingTypes type)
        {
            string settingName = type.ToString();
            var value = context.Query().Filter(x => x.Name.Equals(settingName)).Get().FirstOrDefault();

            if (value == null)
                return null;

            return value.Value;
        }

        public static string[] GetSettingValueCommaDelimited(this IRepository<Setting> context, SettingTypes type)
        {
            string value = context.GetSettingValue(type);
            if (value == null)
                return null;
            return value.Split(',');
        }

        public static bool GetSettingValueBoolean(this IRepository<Setting> context, SettingTypes type)
        {
            string value = context.GetSettingValue(type);

            if (value == null)
                return false;

            switch (value.ToUpper())
            {
                case "1":
                case "TRUE":
                case "YES":
                    return true;
            }
            return false;

        }
    }
}
