using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;

using ExternalPlugin = Plugin;
using Entities.Models;

namespace Service.DataReader
{
    #region Extensions

    /// <summary>
    /// SearchResult Extension for pulling string from field, returns null if fail
    /// </summary>
    /// 
    public static class SearchResultExtensions
    {
        public static string GetFieldValue(this SearchResult sr, string fieldName, int col)
        {
            if (sr == null)
                return null;

            if (col < 0)
                col = 0;

            if (String.IsNullOrEmpty(fieldName.Trim()))
                return null;

            if (!sr.Properties.Contains(fieldName))
                return null;

            //doesnt have a column with the sent request, set it back to 0
            if (sr.Properties[fieldName].Count <= col)
                col = 0;

            try
            {
                object resultProperty = sr.Properties[fieldName][col];
                if (resultProperty.GetType() == typeof(Byte[]))
                {
                    if (fieldName.Equals("objectguid", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Guid guid = new Guid((byte[])resultProperty);
                        return guid.ToString("N");
                    }
                    else
                    {
                        return System.Text.Encoding.Default.GetString((byte[])resultProperty);
                    }
                }

                return resultProperty.ToString();
            }
            catch (Exception ex)
            {
                EventLog.LogManager.GetClassLogger(sr.GetType()).Write("Activedirectory Extension Issue",
                    String.Format("Error grabbing field data. \nFieldName = {0}\n\n{1} ", fieldName, ex.ToString()),
                    LogSeverity.Warning,
                    LogType.DataReader,
                    "GetFieldValue");
                return null;
            }
        }
    }
    #endregion

    internal class ActiveDirectoryConnector : ExternalPlugin.IDataReader
    {
        private bool Connected { get; set; }
        private string LDAPServer { get; set; }
        private string[] OUField { get; set; }
        private string ObjectClassName { get; set; }
        private string[] PeopleField { get; set; }
        private string ExcludeDisabledAccounts { get; set; }
        private SearchResultCollection adResults { get; set; }
        private EventLog.Log logger;

        public ActiveDirectoryConnector()
        {
            logger = EventLog.LogManager.GetClassLogger(this.GetType());
        }

        #region IDataReader Members

        public void Connect(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString.Trim()))
            {
                Connected = false;
                logger.Write("Activedirectory Connection",
                    String.Format("Connection failed due to emtpy or null connection string. connectionString = {0}", (connectionString != null) ? connectionString : "NULL"),
                    LogSeverity.Warning,
                    LogType.DataReader,
                    "Connect");
                return;
            }

            string[] connectionParameters = connectionString.Split(';');

            foreach (var connectionParameter in connectionParameters)
            {
                string[] parameters = connectionParameter.Split(new char []{'='}, 2);

                if (parameters.Length > 1)
                {
                    switch (parameters[0])
                    {
                        case "Server": LDAPServer = parameters[1]; break;
                        case "OUField": OUField = parameters[1].Split(','); break;
                        case "ObjectClass": ObjectClassName = parameters[1]; break;
                        case "PeopleField": PeopleField = parameters[1].Split(','); break;
                        case "ExcludeDisabledAccounts": ExcludeDisabledAccounts = parameters[1]; break;
                    }
                }
            }

            Connected = ValidateConnectionString();

            return;
        }

        private void Connect(DirectoryEntry de)
        {
            if (de == null)
            {
                Connected = false;
                logger.Write("Activedirectory Connection",
                    "Connection failed due to null Directory Entry.",
                    LogSeverity.Warning,
                    LogType.DataReader,
                    "Connect");
                return;
            }
        }

        public bool BuildDataList()
        {
            if (!Connected)
                return false;

            if (adResults != null)
                adResults.Dispose();

            DirectoryEntry de = new DirectoryEntry(LDAPServer);
            DirectorySearcher ds = new DirectorySearcher();
            ds.SearchRoot = de;
            ds.SearchScope = SearchScope.Subtree;
            ds.PageSize = 1000;

            if(ExcludeDisabledAccounts.Equals("1"))
                ds.Filter = "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";

            try
            {
                adResults = ds.FindAll();
            } 
            catch(Exception ex)
            {
                logger.Write("Activedirectory Issue",
                    ex.ToString(),
                    LogSeverity.Error,
                    LogType.DataReader,
                    "BuildDataList");
                Connected = false;
                return false;
            }

            return true;
        }

        private SearchResult Fetch(int dataRecordId)
        {
            if (dataRecordId >= 0 && dataRecordId < adResults.Count)
            {
                return adResults[dataRecordId];
            }

            return null;
        }

        public string Fetch(int dataRecordId, string fieldName)
        {
            var objectType = Fetch(dataRecordId);
            if (objectType != null)
            {
                return objectType.GetFieldValue(fieldName, 0);
            }

            return null;
        }

        public ExternalPlugin.IDataReader FetchParent(int dataRecordId)
        {
            var objectType = Fetch(dataRecordId);
            if (objectType != null)
            {
                var parent = objectType.GetDirectoryEntry().Parent;
                if (parent != null)
                {
                    ActiveDirectoryConnector parentDataReader = new ActiveDirectoryConnector();
                    DirectorySearcher ds = new DirectorySearcher(parent);
                    parentDataReader.adResults = ds.FindAll();
                    parentDataReader.Connected = true;
                    parentDataReader.ExcludeDisabledAccounts = this.ExcludeDisabledAccounts;
                    parentDataReader.LDAPServer = this.LDAPServer;
                    parentDataReader.ObjectClassName = this.ObjectClassName;
                    parentDataReader.OUField = this.OUField;
                    parentDataReader.PeopleField = this.PeopleField;

                    return parentDataReader as ExternalPlugin.IDataReader;
                }
            }

            return null;
        }

        public ExternalPlugin.IDataReader[] FetchChildren(int dataRecordId)
        {
            var objectType = Fetch(dataRecordId);
            if (objectType != null)
            {
                var parent = objectType.GetDirectoryEntry().Children;
                if (parent != null)
                {
                    List<ActiveDirectoryConnector> childrenDataReader = new List<ActiveDirectoryConnector>();

                    foreach(var entry in parent)
                    {
                        ActiveDirectoryConnector parentDataReader = new ActiveDirectoryConnector();
                        DirectorySearcher ds = new DirectorySearcher(entry as DirectoryEntry);
                        parentDataReader.adResults = ds.FindAll();
                        parentDataReader.Connected = true;
                        parentDataReader.ExcludeDisabledAccounts = this.ExcludeDisabledAccounts;
                        parentDataReader.LDAPServer = this.LDAPServer;
                        parentDataReader.ObjectClassName = this.ObjectClassName;
                        parentDataReader.OUField = this.OUField;
                        parentDataReader.PeopleField = this.PeopleField;

                        childrenDataReader.Add(parentDataReader);
                    }


                    return childrenDataReader.ToArray() as ExternalPlugin.IDataReader[];
                }
            }

            return null;
        }

        public ExternalPlugin.DataReaderType GetType(int dataRecordId)
        {
            if (dataRecordId >= 0 && dataRecordId < adResults.Count)
            {
                var objectType = adResults[dataRecordId].Properties[ObjectClassName];
                if (objectType != null)
                {
                    foreach (var @object in objectType)
                    {
                        if(OUField.Contains(@object.ToString()))
                            return ExternalPlugin.DataReaderType.Structure;
                        else if(PeopleField.Contains(@object.ToString()))
                            return ExternalPlugin.DataReaderType.Component;
                    }
                }
            }

            return ExternalPlugin.DataReaderType.Unknown;
        }

        public IEnumerable<int> DataList()
        {
            if (Connected && adResults != null)
            {
                for (int x = 0; x < adResults.Count; x++)
                {
                    if(adResults[x].Properties.Contains(ObjectClassName))
                        yield return x;
                }
            }
        }

        public void Dispose()
        {
            if (!Connected)
                return;

            if (adResults != null)
                adResults.Dispose();
        }

        #endregion

        private bool ValidateConnectionString()
        {
            if (String.IsNullOrEmpty(LDAPServer.Trim()))
                return false;

            if(OUField != null && OUField.Length > 0)
            {
                if(String.IsNullOrEmpty(OUField[0].Trim()))
                    OUField[0] = "organizationalUnit";
            }
            else {
                OUField = new string[1] { "organizationalUnit" };
            }

            if(PeopleField != null && PeopleField.Length > 0)
            {
                if(String.IsNullOrEmpty(PeopleField[0].Trim()))
                    PeopleField[0] = "organizationPerson";
            }
            else
            {
                PeopleField = new string[2] { "organizationPerson", "person" };
            }

            if(String.IsNullOrEmpty(ObjectClassName))
                ObjectClassName = "objectClass";

            if(string.IsNullOrEmpty(ExcludeDisabledAccounts))
                ExcludeDisabledAccounts = "1"; //TRUE

            return true;

        }
    }
}
