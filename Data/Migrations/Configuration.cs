namespace Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Entities.Models;
    using Plugin;

    internal sealed class Configuration : DbMigrationsConfiguration<Data.GedsContext>
    {
        public Configuration()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Data.GedsContext context)
        {
            bool performSeed = false;

            if (!performSeed)
                return;

            //Add Settings
            context.Set<Setting>().AddOrUpdate(
                new Setting { Id = 1, Name = "CONNECTION_STRING", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 5, Name = "GEDS_EMAIL_TO", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 6, Name = "GEDS_EMAIL_CC", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 7, Name = "GEDS_EMAIL_FROM", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 8, Name = "GEDS_EMAIL_SUBJECT", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 9, Name = "GEDS_EMAIL_BODY", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 10, Name = "GEDS_AUTO_JOB_SECS", Value = "172800", State = Entities.ObjectState.Added },
                new Setting { Id = 11, Name = "GEDS_HEADER_SOURCE_AGENT_NAME", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 12, Name = "GEDS_HEADER_DEPARTMENTAL_ACRONYM", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 13, Name = "GED_MANDATORYKEY_MISSING_JUNK_DATA", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 14, Name = "GEDS_SORTING_LOOKUP_COLUMN_STRUCTURE", Value = "Organisation List Sequence Number", State = Entities.ObjectState.Added },
                new Setting { Id = 15, Name = "GEDS_SORTING_LOOKUP_COMPARE_STRUCTURE", Value = "Description English", State = Entities.ObjectState.Added },
                new Setting { Id = 16, Name = "GEDS_SORTING_LOOKUP_COLUMN_COMPONENT", Value = "Person List Sequence Number", State = Entities.ObjectState.Added },
                new Setting { Id = 17, Name = "GEDS_SORTING_LOOKUP_COMPARE_COMPONENT", Value = "ExtraDescription", State = Entities.ObjectState.Added },
                new Setting { Id = 18, Name = "LDAP_SETTING_EXCLUDE_DISABLEDACCOUNTS", Value = "true", State = Entities.ObjectState.Added },
                new Setting { Id = 19, Name = "APP_EMAIL_CRITICAL_ISSUE", Value = "C:\\Program Files (x86)\\cdfedit\\App\\CDFEdit.exe", State = Entities.ObjectState.Added },
                new Setting { Id = 20, Name = "APP_LOCATION_CDF", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 21, Name = "APP_EMAIL_CRITICAL_ISSUE_SUBJECT", Value = "GEDS Critical Issue", State = Entities.ObjectState.Added },
                new Setting { Id = 22, Name = "APP_EMAIL_AUTOSEND_GEDS", Value = "true", State = Entities.ObjectState.Added },
                new Setting { Id = 23, Name = "APP_GEDS_OUTPUT_LOCATION", Value = "D:\\GEDS\\", State = Entities.ObjectState.Added },
                new Setting { Id = 24, Name = "APP_SMTP_SERVER", Value = "", State = Entities.ObjectState.Added },
                new Setting { Id = 25, Name = "APP_PLUGIN_LOCATION", Value = "D:\\GEDS\\Plugins\\", State = Entities.ObjectState.Added }
                );

            
            //Content Field Mapping 
            var activeDirectoryContentFieldMapping = new ContentFieldMapping() { Id = 1, Value = "Data Reader", ValueCode = ContentFieldMappingType.DataReader, State = Entities.ObjectState.Added };
            var pluginContentFieldMapping = new ContentFieldMapping() { Id = 2, Value = "Plugin", ValueCode = ContentFieldMappingType.Plugin, State = Entities.ObjectState.Added };
            var stringContentFieldMapping = new ContentFieldMapping() { Id = 3, Value = "String", ValueCode = ContentFieldMappingType.String, State = Entities.ObjectState.Added };
            context.Set<ContentFieldMapping>().AddOrUpdate(activeDirectoryContentFieldMapping, pluginContentFieldMapping, stringContentFieldMapping);

            //RuleActionType Mapping
            context.Set<RuleAction>().AddOrUpdate(
                new RuleAction { Id = 1, Value = "Replace", ValueCode = RuleActionType.Replace, State = Entities.ObjectState.Added },
                new RuleAction { Id = 2, Value = "Replace with match", ValueCode = RuleActionType.ReplaceWithMatch, State = Entities.ObjectState.Added },
                new RuleAction { Id = 3, Value = "Delete", ValueCode = RuleActionType.Delete, State = Entities.ObjectState.Added }
                );


            //Add StructureFields
            int x = 1;
            List<Structure> structureList = new List<Structure>()
            {
                new Structure { Id = x+=1, Order = x, Field = "English Acronym", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 8, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French Acronym", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 8, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Description English", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "name", Skip = false, MaxLength = 96, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Description French", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "street", Skip = false, MaxLength = 96, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Organisational Unit Key", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "objectGUID", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Parent Organisational Unit Key", ContentFieldMapping = pluginContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Telephone Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Secure Telephone number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Fax Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Secure Fax Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "TDD Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "O/R (X.400) Address", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 128, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "SMTP Address (RFC 2822)", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 128, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Mail Stop", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 10, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "English Post Office Box", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French Post Office Box", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "English Street Address", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 128, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French Street Address", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 128, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "English City", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French City", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "English Province", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French Province", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Postal Code", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 16, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "English Country", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French Country", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Admin Assistant Name", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Admin Assistant Telephone Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Inclusion in Paper Directory", ContentFieldMapping = stringContentFieldMapping, Source = "N", Skip = false, MaxLength = 6, Mandatory = true, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "Organisation List Sequence Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 9, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "English URL (RFC 1738)", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 255, Mandatory = false, Default = "" },
                new Structure { Id = x+=1, Order = x, Field = "French URL (RFC 1738)", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 255, Mandatory = false, Default = "" }
            };

            structureList.ForEach(f => f.State = Entities.ObjectState.Added);
            context.Set<Structure>().AddOrUpdate(
                structureList.ToArray()
                );

            //add Component Fields
            var y = 1;
            List<Component> componentList = new List<Component>()
            {
                new Component { Id = y += 1, Order = y, Field = "Record Type", ContentFieldMapping = stringContentFieldMapping, Source = "01", Skip = false, MaxLength = 2, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Surname", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "sn", Skip = false, MaxLength = 64, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Given Name", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "givenName", Skip = false, MaxLength = 64, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Initials", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 5, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "English Prefix", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 16, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "French Prefix", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 16, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "English Suffix", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 16, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "French Suffix", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 16, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "English Title", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "title", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "French Title", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "extensionAttribute6", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Parent Organisational Unit Key", ContentFieldMapping = pluginContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Person List Sequence Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 9, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Telephone Number", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "telephoneNumber", Skip = false, MaxLength = 32, Mandatory = true, Default = "(613) 991-3896" },
                new Component { Id = y += 1, Order = y, Field = "Secure Telephone Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Fax Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Secure Fax Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Alternate Telephone Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "O/R (X.400) Address", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 128, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "RFC-822 (SMTP) Address", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 128, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Mail Stop", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 10, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Room Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 10, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Floor or Zone", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 10, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "English Building Name", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "French Building Name", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Local Messaging Type", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Word Processing Type", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Spreadsheet Type", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Terminal Type", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Admin Assistant Name", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Admin Assistant Telephone Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Executive Assistant Name", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 64, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Executive Assistant Phone No", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Show in GEDS Public", ContentFieldMapping = stringContentFieldMapping, Source = "Y", Skip = false, MaxLength = 1, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Show in GEDS Internal", ContentFieldMapping = stringContentFieldMapping, Source = "Y", Skip = false, MaxLength = 2, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "Inclusion in Paper Directory", ContentFieldMapping = stringContentFieldMapping, Source = "N", Skip = false, MaxLength = 6, Mandatory = true, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "TDD Number", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "English Post Office Box", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "French Post Office Box", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = false, Default = "" },
                new Component { Id = y += 1, Order = y, Field = "English Street Address", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "streetAddress", Skip = false, MaxLength = 128, Mandatory = true, Default = "99 Bank Street" },
                new Component { Id = y += 1, Order = y, Field = "French Street Address", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "extensionAttribute2", Skip = false, MaxLength = 128, Mandatory = true, Default = "99, rue Bank" },
                new Component { Id = y += 1, Order = y, Field = "English City", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "l", Skip = false, MaxLength = 32, Mandatory = true, Default = "Ottawa" },
                new Component { Id = y += 1, Order = y, Field = "French City", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "l", Skip = false, MaxLength = 32, Mandatory = true, Default = "Ottawa" },
                new Component { Id = y += 1, Order = y, Field = "English Province", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "st", Skip = false, MaxLength = 32, Mandatory = true, Default = "Ontario" },
                new Component { Id = y += 1, Order = y, Field = "French Province", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "extensionAttribute3", Skip = false, MaxLength = 32, Mandatory = true, Default = "Ontario" },
                new Component { Id = y += 1, Order = y, Field = "Postal Code", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "postalCode", Skip = false, MaxLength = 16, Mandatory = true, Default = "K1P 6B9" },
                new Component { Id = y += 1, Order = y, Field = "English Country", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "Canada" },
                new Component { Id = y += 1, Order = y, Field = "French Country", ContentFieldMapping = stringContentFieldMapping, Source = "", Skip = false, MaxLength = 32, Mandatory = true, Default = "Canada" },
                new Component { Id = y += 1, Order = y, Field = "ExtraDescription", ContentFieldMapping = activeDirectoryContentFieldMapping, Source = "description", Skip = true, MaxLength = 255, Mandatory = false, Default = "" }
            };
            componentList.ForEach(c => c.State = Entities.ObjectState.Added);
            context.Set<Component>().AddOrUpdate(
                componentList.ToArray()
            );


            //add structure order
            context.Set<StructureOrderPriority>().AddOrUpdate(
                new StructureOrderPriority { Id = 1, Order = 1, Value = "Flying Hippo", State = Entities.ObjectState.Added },
                new StructureOrderPriority { Id = 2, Order = 2, Value = "Executives", State = Entities.ObjectState.Added },
                new StructureOrderPriority { Id = 3, Order = 3, Value = "Programmers", State = Entities.ObjectState.Added }
                );

            //add component order
            context.Set<ComponentOrderPriority>().AddOrUpdate(
                new ComponentOrderPriority { Id = 1, Order = 1, Value = "CEO", State = Entities.ObjectState.Added },
                new ComponentOrderPriority { Id = 2, Order = 2, Value = "Vice President", State = Entities.ObjectState.Added },
                new ComponentOrderPriority { Id = 3, Order = 3, Value = "Director", State = Entities.ObjectState.Added }
                );
             
        }
    }
}
