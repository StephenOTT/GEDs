using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin;

namespace Geds.Parent.Guid.Lookup
{
    public class GuidLookup : IGedsPlugin
    {
        private SearchResultCollection adResults;
        private bool connected;
        //private PrincipalContext ctx;
        //private DirectoryEntry de;
        //private DirectorySearcher ds;

        public GuidLookup()
        {
            string server = "LDAP://OU=Employees,dc=flyinghippo,dc=ca";
   
            using (DirectoryEntry de = new DirectoryEntry(server))
            {
                using (DirectorySearcher ds = new DirectorySearcher(de))
                {
                    ds.SearchScope = SearchScope.Subtree;
                    ds.Filter = "(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
                    ds.PageSize = 1000;
                    ds.PropertiesToLoad.AddRange(new string[] { "objectClass", "objectGUID", "sn", "givenname" });

                    try
                    {
                        adResults = ds.FindAll();
                        connected = true;
                    }
                    catch (InvalidOperationException ex)
                    {
                        LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + ex.ToString());
                        connected = false;
                    }
                    catch (NotSupportedException nse)
                    {
                        LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + nse.ToString());
                        connected = false;
                    }
                }
            }

            
        }

        #region IGedsPlugin Members

        public void Dispose()
        {
            if (adResults != null)
                adResults.Dispose();
        }

        #endregion

        private void WriteInformation(string message)
        {
            LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + message);
        }
        private void WriteWarning(string message)
        {
            LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + message);
        }
        private void WriteCritical(string message)
        {
            LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + message);
        }
        private void WriteError(string message)
        {
            LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + message);
        }

        #region IGedsPlugin Members


        public string Execute(string fieldNameOfPlugin, Dictionary<string, IGedsEntity> entityRow)
        {
            if (!connected)
            {
                WriteInformation("No connection made.");
                return null;
            }

            if (entityRow == null)
            {
                WriteInformation("Missing entityRow data");
                return null;
            }

            if (String.IsNullOrEmpty(fieldNameOfPlugin))
            {
                WriteInformation("FieldName for Plugin is null or emtpy");
                return null;
            }

            if (!entityRow.ContainsKey(fieldNameOfPlugin))
            {
                WriteInformation("entityRow does not contain the key of the Field name passed.");
                return null;
            }

            IGedsEntity entityForPlugin = entityRow[fieldNameOfPlugin];

            bool generatedError = false;

            //Structure Lookup
            if (entityRow.ContainsKey("Organisational Unit Key"))
            {
                IGedsEntity entityForADFilter = entityRow["Organisational Unit Key"];

                if (entityForADFilter == null)
                {
                    WriteInformation("Entity for entityRow['Organisational Unit Key'] is null");
                    return null;
                }

                if (String.IsNullOrEmpty(entityForADFilter.Value))
                {
                    WriteInformation("entityForADFilter.Value is null or empty");
                    return null;
                }

                
                foreach (SearchResult record in IterateRecordsCollection(new string[] {"organizationalUnit"}))
                {
                    if (record.Properties.Contains("objectGUID"))
                    {
                        try
                        {
                            System.Guid guid = new System.Guid((byte[])record.Properties["objectGUID"][0]);
                            if (guid.ToString("N").Equals(entityForADFilter.Value))
                            {
                                return record.GetDirectoryEntry().Parent.Guid.ToString("N");
                            }
                        }
                        catch (Exception e)
                        {
                            LogInstance.LogPluginError(this.GetType().Namespace + "\n\n" + e.ToString());
                        }
                    }
                }
            }
            //Component Lookup    
            else if (entityRow.ContainsKey("Surname") && entityRow.ContainsKey("Given Name"))
            {
                string surname = entityRow["Surname"].Value;
                string givenname = entityRow["Given Name"].Value;

                if (String.IsNullOrEmpty(surname) || String.IsNullOrEmpty(givenname))
                {
                    WriteInformation("Surname or Givenname empyty or null");
                    return null;
                }

                foreach (SearchResult record in IterateRecordsCollection(new string[] { "person", "organizationPerson" }))
                {
                    if (record.Properties.Contains("sn") && record.Properties.Contains("givenname"))
                    {
                        if (surname.Equals(record.Properties["sn"][0].ToString())
                            && givenname.Equals(record.Properties["givenname"][0].ToString()))
                        {
                            return record.GetDirectoryEntry().Parent.Guid.ToString("N");
                        }
                    }
                }
            }
            return null;
        }

        private IEnumerable<SearchResult> IterateRecordsCollection(string[] propertyType)
        {
            if (!connected || adResults == null)
            {
                WriteInformation("AD results were empty.");
                yield break;
            }

            foreach (SearchResult record in adResults)
            {
                if (record.Properties.Contains("objectClass"))
                {
                    var objectType = record.Properties["objectClass"];
                    if (objectType != null)
                    {
                        foreach (var @object in objectType)
                        {
                            if (propertyType.Contains(@object.ToString()))
                            {
                                yield return record;
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region IGedsPlugin Members


        #endregion

        #region IGedsPlugin Members


        public IPluginLog LogInstance
        {
            get;
            set;
        }

        #endregion
    }
}
