using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Service.Plugin;
using Service.DataSets;
using Service.DataReader;

using Repository;
using Entities.Models;
using Data;
using ExternalPlugin = Plugin;
using System.IO;
using System.Net.Mail;
using Service.EventLog;
using System.Text.RegularExpressions;
using System.Threading;

namespace Service
{
    public partial class FH_Geds_Service : ServiceBase
    {
        private IGedsRepository repository;
        private ExternalPlugin.IDataReader dataReader;
        private System.Threading.Timer jobTimer;
        private IUnitOfWork unitOfWork;
        private GedsSort sortingPriority;
        private DateTime jobStarted;
        private PluginHandler plugins;
        private string GuidSession;
        private List<string> pluginLogged;
        int _timerLock;
        private Service.EventLog.Log logger;

        private struct FileOutputData
        {
            public string StructureLocation;
            public string ComponentLocation;
            public string RootLocation;
        }

        public FH_Geds_Service()
        {
            InitializeComponent();
        }

        #region START / STOP Service

        protected override void OnStart(string[] args)
        {
            jobTimer = new System.Threading.Timer(jobTimer_Elapsed, null, 300000, Timeout.Infinite);
            // 1 min = 60000, 5 mins = 300000
        }

        void jobTimer_Elapsed(object state)
        {
            if (Interlocked.CompareExchange(ref _timerLock, 1, 0) != 0)
                return;

            try
            {
                StartGedsDataGathering();
            }
            catch (Exception e)
            {
            }
            finally
            {
                Interlocked.Exchange(ref _timerLock, 0);
            }
        }

        protected override void OnStop()
        {
            jobTimer.Dispose();
            jobTimer = null;
        }

        #endregion 

        public void ConsoleDebugStart()
        {
            StartGedsDataGathering();
        }

        private void StartGedsDataGathering()
        {
            GuidSession = Guid.NewGuid().ToString("N");
            jobStarted = DateTime.Now;

            //Initialize 
            LogManager.Instantiate(GuidSession);
            unitOfWork = new UnitOfWork(new GedsContext());
            repository = new GedsRepository();
            dataReader = new ActiveDirectoryConnector();
            sortingPriority = new GedsSort();
            plugins = new PluginHandler();
            pluginLogged = new List<string>();
            logger = LogManager.GetClassLogger(this.GetType());
            
            BuildDataReader();
            plugins.LoadPlugins(unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.APP_PLUGIN_LOCATION));

            ImportGedsFieldMappingsStructure();
            ImportGedsFieldMappingsComponent();
            ImportSortPriority();
            
            BuildDataReaderList();
            EnumerateDataReaderList();
            ApplyCustomRulesToDataReaderList();
            //Apply Rules

            //Build Record for database
            FileOutputData outputData;
            Job currentJob = new Job();
            currentJob.JobStarted = jobStarted;
            

            if (GeneratOutputFiles(out outputData)) //Will also apply the GEDS maxlength RULE for each field
            {
                currentJob.StructureFileLocation = outputData.StructureLocation;
                currentJob.ComponentFileLocation = outputData.ComponentLocation;

                //If AUTO mail, mail completed files, this is disabled currently as it is not a safe idea
                if (1 == 0 && unitOfWork.Repository<Setting>().GetSettingValueBoolean(SettingsRepository.SettingTypes.APP_EMAIL_AUTOSEND_GEDS))
                {
                    string emailTo = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_EMAIL_TO);
                    string[] emailCCs = unitOfWork.Repository<Setting>().GetSettingValueCommaDelimited(SettingsRepository.SettingTypes.GEDS_EMAIL_CC);
                    string emailSubject = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_EMAIL_SUBJECT);
                    string emailFrom = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_EMAIL_FROM);
                    string emailBody = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_EMAIL_BODY);

                    if (!string.IsNullOrEmpty(emailTo) && !string.IsNullOrEmpty(emailFrom))
                    {
                        if (SendMail(emailTo, emailFrom, emailCCs, emailSubject, emailBody, new string[] { outputData.StructureLocation, outputData.ComponentLocation }))
                        {
                            if (emailCCs != null)
                                currentJob.SentTo = emailTo + "," + string.Join(",", emailCCs);
                            else currentJob.SentTo = emailTo;

                            currentJob.DataSentOn = DateTime.Now;
                        }
                    }
                }
                currentJob.Status = true;
            }
            else
            {
                currentJob.Status = false;
            }

            try
            {
                //Insert New Built record to Database
                currentJob.JobCompleted = DateTime.Now;
                currentJob.Guid = GuidSession;
                currentJob.State = Entities.ObjectState.Added;
                unitOfWork.Repository<Job>().InsertGraph(currentJob);
                unitOfWork.Save();                
            }
            catch (Exception ex)
            {
                logger.Write("Saving job issue", ex.ToString(), LogSeverity.Error, LogType.General);
                currentJob.Status = false;
            }
            

            //send mail on job failed?
            if (!currentJob.Status &&
                unitOfWork.Repository<Setting>().GetSettingValueBoolean(SettingsRepository.SettingTypes.APP_EMAIL_CRITICAL_ISSUE))
            {
                SendMail(
                    unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.APP_EMAIL_CRITICAL_ISSUE),
                    unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.APP_EMAIL_CRITICAL_ISSUE),
                    null,
                    "FH.GEDS.Service Critical Errors",
                    String.Format("{0} - FH.GEDS.Service encountered error(s)."
                        , DateTime.Now.ToShortDateString()),
                    null);
            }

            UpdateTimerSettings();
            
            unitOfWork.Dispose();
            dataReader.Dispose();
            repository.Dispose();
            plugins.Dispose();
        }

        private bool SendMail(string to, string from, string[] cc, string subject, string body, string[] files)
        {
            return false;

            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
            {
                string smtpServer = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.APP_SMTP_SERVER);
                if (string.IsNullOrEmpty(smtpServer))
                    return false;

                MailMessage mail = new MailMessage();
                SmtpClient stmp = new SmtpClient(smtpServer);

                try
                {
                    mail.To.Add(new MailAddress(to));
                    mail.From = new MailAddress(from);
                    if (cc != null)
                        cc.ToList().ForEach((s) => mail.CC.Add(s));
                    if (!string.IsNullOrEmpty(subject))
                        mail.Subject = subject;
                    if (!string.IsNullOrEmpty(body))
                        mail.Body = body;

                    if (files != null)
                    {
                        foreach (string file in files)
                        {
                            if (!File.Exists(file))
                            {
                                //file not found but tried to include, error do not send email
                                logger.Write("Mail warning",
                                    String.Format("Couldn't find file to attach to mail. File = {0}", file),
                                    LogSeverity.Warning, methodName: "SendMail");
                                return false;
                            }

                            mail.Attachments.Add(new Attachment(new MemoryStream(File.ReadAllBytes(file)), Path.GetFileName(file)));
                        }
                    }

                    stmp.Send(mail);
                }
                catch (Exception ex)
                {
                    logger.Write("Mail error", ex.ToString(), LogSeverity.Error, LogType.General, "SendMail");
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private void BuildDataReader()
        {
            var settingEntity = unitOfWork.Repository<Setting>();

            string connectionString = settingEntity.GetSettingValue(SettingsRepository.SettingTypes.CONNECTION_STRING);

            if (connectionString != null)
            {
                dataReader.Connect(connectionString);
            }
        }


        private void ImportGedsFieldMappingsStructure()
        {
            var structureFields = unitOfWork.Repository<Structure>()
                                    .Query()
                                    .AsNoTracking()
                                    .Include(s=>s.ContentFieldMapping)
                                    .Get()
                                    .ToList();

            foreach (var structureField in structureFields)
            {
                if (repository.Structures.Properties.ContainsKey(structureField.Field))
                    continue;

                ExternalPlugin.IGedUnit gedUnit = new GedUnit()
                {
                    DefaultValue = structureField.Default,
                    FieldName = structureField.Field,
                    Ignore = structureField.Skip,
                    Mandatory = structureField.Mandatory,
                    MaxLength = structureField.MaxLength,
                    Order = structureField.Order,
                    Type = structureField.ContentFieldMapping.ValueCode,
                    Source = structureField.Source,
                    Validation = structureField.Validation
                };

                repository.Structures.Properties.Add(gedUnit.FieldName, gedUnit);
            }
        }

        private void ImportGedsFieldMappingsComponent()
        {
            var componentFields = unitOfWork.Repository<Entities.Models.Component>()
                                    .Query()
                                    .AsNoTracking()
                                    .Include(i=>i.ContentFieldMapping)
                                    .Get()
                                    .ToList();

            foreach (var componentField in componentFields)
            {
                if (repository.Components.Properties.ContainsKey(componentField.Field))
                    continue;

                ExternalPlugin.IGedUnit gedUnit = new GedUnit()
                {
                    DefaultValue = componentField.Default,
                    FieldName = componentField.Field,
                    Ignore = componentField.Skip,
                    Mandatory = componentField.Mandatory,
                    MaxLength = componentField.MaxLength,
                    Order = componentField.Order,
                    Type = componentField.ContentFieldMapping.ValueCode,
                    Source = componentField.Source,
                    Validation = componentField.Validation
                };

                repository.Components.Properties.Add(gedUnit.FieldName, gedUnit);
            }
        }


        private void ImportSortPriority()
        {
            sortingPriority.StructureSort = unitOfWork.Repository<StructureOrderPriority>()
                                        .Query()
                                        .AsNoTracking()
                                        .OrderBy(q => q.OrderBy(c => c.Order))
                                        .Get()
                                        .Select(s => new SortData { Order = s.Order, Value = s.Value })
                                        .ToList();

            sortingPriority.StructureSortCompareAgainstFieldName = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_SORTING_LOOKUP_COMPARE_STRUCTURE);
            sortingPriority.StructureSortFieldName = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_SORTING_LOOKUP_COLUMN_STRUCTURE);

            sortingPriority.ComponentSort = unitOfWork.Repository<ComponentOrderPriority>()
                                        .Query()
                                        .AsNoTracking()
                                        .OrderBy(q => q.OrderBy(c => c.Order))
                                        .Get()
                                        .Select(s => new SortData { Order = s.Order, Value = s.Value })
                                        .ToList();

            sortingPriority.ComponentSortCompareAgainstFieldName = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_SORTING_LOOKUP_COMPARE_COMPONENT);
            sortingPriority.ComponentSortFieldName = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_SORTING_LOOKUP_COLUMN_COMPONENT);
        }

        private void BuildDataReaderList()
        {
            dataReader.BuildDataList();

            //on fail raise exception
        }

        private void EnumerateDataReaderList()
        {
            foreach (var dataReaderIndex in dataReader.DataList())
            {
                ExternalPlugin.DataReaderType dataReaderEntityType = dataReader.GetType(dataReaderIndex);

                if (dataReaderEntityType == ExternalPlugin.DataReaderType.Component)
                {
                    AddGedEntity(dataReaderIndex, repository.Components, repository.ComponentEntities, dataReaderEntityType);
                }
                else if (dataReaderEntityType == ExternalPlugin.DataReaderType.Structure)
                {
                    AddGedEntity(dataReaderIndex, repository.Structures, repository.StructureEntities, dataReaderEntityType);
                }
            }
        }


        private void AddGedEntity(int dataReaderIndex, 
            IGedsProperty gedsProperty,
            List<Dictionary<string, ExternalPlugin.IGedsEntity>> sortedList,
            ExternalPlugin.DataReaderType dataReaderEntityType)
        {

            Dictionary<string, ExternalPlugin.IGedsEntity> gedEntities = new Dictionary<string, ExternalPlugin.IGedsEntity>();
            List<string> pluginJobs = new List<string>();
            
            foreach (var kvp in gedsProperty.Properties)
            {
                ExternalPlugin.IGedUnit gedUnit = kvp.Value;
                ExternalPlugin.IGedsEntity gedEntity = new GedsEntity();
                string dataReaderValue = String.Empty;

                switch (gedUnit.Type)
                {
                    case ExternalPlugin.ContentFieldMappingType.DataReader:
                        dataReaderValue  = dataReader.Fetch(dataReaderIndex, gedUnit.Source);
                        break;
                    case ExternalPlugin.ContentFieldMappingType.String:
                        dataReaderValue = gedUnit.Source;
                        break;
                    case ExternalPlugin.ContentFieldMappingType.Plugin:
                        pluginJobs.Add(kvp.Key);
                        dataReaderValue = string.Empty;
                        break;                        
                }

                if (String.IsNullOrEmpty(dataReaderValue))
                {
                    dataReaderValue = gedUnit.DefaultValue;
                }

                gedEntity.Value = dataReaderValue;
                gedEntity.GedsUnit = gedUnit;
                
                if(!gedEntities.ContainsKey(gedUnit.FieldName))
                    gedEntities.Add(gedUnit.FieldName, gedEntity);
            }

            //perform Plugin execution
            ExecutePlugin(pluginJobs, gedEntities);

            if (gedEntities.Count > 0)
            {
                // update sorting on entity
                GenerateSortIndexForEntity(dataReaderEntityType, gedEntities);
                sortedList.Add(gedEntities);
            }
        }

        private void ExecutePlugin(List<string> pluginJobs, Dictionary<string, ExternalPlugin.IGedsEntity> gedEntities)
        {
            if (pluginJobs != null && gedEntities != null)
            {
                foreach (var entityKey in pluginJobs)
                {
                    if (!gedEntities.ContainsKey(entityKey))
                        continue;

                    ExternalPlugin.IGedsEntity entity = gedEntities[entityKey];
                    ExternalPlugin.IGedsPlugin plugin = plugins.GetPlugin(entity.GedsUnit.Source);

                    if (plugin != null)
                    {
                        string pluginExecResult = String.Empty;
                        plugin.LogInstance = logger;

                        try
                        {
                            pluginExecResult = plugin.Execute(entityKey, gedEntities);
                        }
                        catch (Exception ex)
                        {
                            logger.Write("Plugin Issue", ex.ToString(), LogSeverity.Error, LogType.Plugin, "ExecutePlugin");
                            continue;
                        }

                        if (pluginExecResult == null)
                            continue;

                        entity.Value = pluginExecResult;
                    }
                    else
                    {
                        if (!pluginLogged.Any(a => a.Equals(entity.GedsUnit.Source)))
                        {
                            logger.Write("Plugin Issue",
                                String.Format(
                                "Plugin: {0}\nInfo: Plugin not found for field name '{1}'.\n\nPlugin List:\n{2}",
                                entity.GedsUnit.Source,
                                entityKey,
                                plugins.ToString()
                                ), LogSeverity.Error, LogType.Plugin, "ExecutePlugin");
                            pluginLogged.Add(entity.GedsUnit.Source);
                        }
                    }
                }
            }
        }

        private void ApplyCustomRulesToDataReaderList()
        {
            var structRules = unitOfWork.Repository<StructureRule>()
                                .Query()
                                .Include(i => i.RuleAction).Include(i => i.LookupColumn).Include(i => i.ActionColumn)
                                .AsNoTracking()
                                .OrderBy(o=>o.OrderBy(ob => ob.Order))
                                .Get()
                                .Select(sr => new CustomRule
                                {
                                    LookupColumn = sr.LookupColumn.Field,
                                    RegularExpression = sr.RegularExpression,
                                    RuleAction = sr.RuleAction.ValueCode,
                                    ActionColumn = sr.ActionColumn.Field,
                                    ActionReplaceValue = sr.ReplaceValue,
                                    ActionReplaceColumn = sr.ReplaceColumn
                                })
                                .ToList();

            ApplyCustomRulesToDataReaderList(structRules, repository.StructureEntities);

            structRules = null;

            var compRules = unitOfWork.Repository<ComponentRule>()
                                .Query()
                                .Include(i => i.RuleAction).Include(i => i.LookupColumn).Include(i => i.ActionColumn)
                                .AsNoTracking()
                                .OrderBy(o => o.OrderBy(ob => ob.Order))
                                .Get()
                                .Select(cr => new CustomRule
                                {
                                    LookupColumn = cr.LookupColumn.Field,
                                    RegularExpression = cr.RegularExpression,
                                    RuleAction = cr.RuleAction.ValueCode,
                                    ActionColumn = cr.ActionColumn.Field,
                                    ActionReplaceValue = cr.ReplaceValue,
                                    ActionReplaceColumn = cr.ReplaceColumn
                                })
                                .ToList();

            ApplyCustomRulesToDataReaderList(compRules, repository.ComponentEntities);
        }

        private void ApplyCustomRulesToDataReaderList(List<CustomRule> rules, List<Dictionary<string, ExternalPlugin.IGedsEntity>> list)
        {
            if (rules == null)
                return;
            if (list == null)
                return;

            foreach (var rule in rules)
            {
                Regex lookupExpression = new Regex(rule.RegularExpression);

                for (int x = list.Count-1; x >= 0; x--)
                {
                    var entity = list[x];

                    if (!entity.ContainsKey(rule.LookupColumn) && entity[rule.LookupColumn] != null)
                        continue;
                    
                    string entityColumnValue = entity[rule.LookupColumn].Value;

                    if (entityColumnValue != null && lookupExpression.IsMatch(entityColumnValue))
                    {
                        if (rule.RuleAction == RuleActionType.Delete)
                        {
                            list.RemoveAt(x);
                            continue;
                        }
                        else if (rule.RuleAction == RuleActionType.Replace) //replace action column  with string
                        {
                            string replaceValue = null;
                            if (!string.IsNullOrEmpty(rule.ActionReplaceColumn)
                                && entity.ContainsKey(rule.ActionReplaceColumn)
                                && entity[rule.ActionReplaceColumn] != null)
                            {
                                replaceValue = entity[rule.ActionReplaceColumn].Value;
                            }
                            else
                            {
                                replaceValue = rule.ActionReplaceValue;
                            }

                            if (replaceValue != null && entity.ContainsKey(rule.ActionColumn) && entity[rule.ActionColumn] != null)
                                entity[rule.ActionColumn].Value = replaceValue;
                        }
                        else if (rule.RuleAction == RuleActionType.ReplaceWithMatch) //replace matches with value
                        {
                            string replaceValue = null;

                            var lookupMatches = lookupExpression.Matches(entityColumnValue);

                            if (!string.IsNullOrEmpty(rule.ActionReplaceColumn)
                                && entity.ContainsKey(rule.ActionReplaceColumn)
                                && entity[rule.ActionReplaceColumn] != null) //simple column replacement
                            {
                                foreach (Match match in lookupMatches)
                                {

                                    replaceValue = entity[rule.ActionReplaceColumn].Value;
                                }
                            }
                            else //need to determine if replacement string contains match clauses
                            {
                                RuleMatch[] replaceExpMatches = RuleMatcher.GroupsInRegexSet(rule.ActionReplaceValue);

                                if (replaceExpMatches != null)
                                {
                                    replaceValue = rule.ActionReplaceValue;
                                }
                            }

                            if (replaceValue != null && entity.ContainsKey(rule.ActionColumn) && entity[rule.ActionColumn] != null)
                                entity[rule.ActionColumn].Value = replaceValue;
                        }
                    }
                    
                }
            }
        }

        private void GenerateSortIndexForEntity(ExternalPlugin.DataReaderType dataReaderEntityType, Dictionary<string, ExternalPlugin.IGedsEntity> gedEntities)
        {
            if (gedEntities == null)
                return;

            if (gedEntities.ContainsKey(sortingPriority.GetFieldNameForCompare(dataReaderEntityType))
                    && gedEntities.ContainsKey(sortingPriority.GetFieldNameForSort(dataReaderEntityType)))
            {
                ExternalPlugin.IGedsEntity entity = gedEntities[sortingPriority.GetFieldNameForCompare(dataReaderEntityType)];

                if (entity != null && entity.GedsUnit != null)
                {
                    int sortIndexValue = sortingPriority.GetSortIndex(
                        dataReaderEntityType,
                        entity.GedsUnit.FieldName,
                        entity.Value);

                    var sortFieldEntity = gedEntities[sortingPriority.GetFieldNameForSort(dataReaderEntityType)];
                    if(sortFieldEntity != null)
                        sortFieldEntity.Value = sortIndexValue.ToString();
                }
            }
        }

        #region Generate CSV Files

        private bool GeneratOutputFiles(out FileOutputData fileOutputData)
        {
            fileOutputData = new FileOutputData();

            string fileOutputLocation = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.APP_GEDS_OUTPUT_LOCATION);

            if(string.IsNullOrEmpty(fileOutputLocation.Trim()))
                return false;

            fileOutputLocation = fileOutputLocation.TrimEnd(new char[] { '\\', '/'});

            if (!Directory.Exists(fileOutputLocation))
            {
                logger.Write("Directory Issue", "APP_GEDS_OUTPUT_LOCATION setting location does not exist.", LogSeverity.Error, methodName:"GenerateOutputFiles");
                return false;
            }

            //check if new instance directory already exists
            string currentInstanceOutputLocation = fileOutputLocation + '\\' + DateTime.Today.ToString("yyyy-MM-dd");
            int retryCount = 0;
            while(Directory.Exists(currentInstanceOutputLocation))
            {
                if(retryCount >= 100)
                {
                    logger.Write("Directory Issue", "Exceeded the limit of currentInstanceOutputLocation directories to create. currentInstanceOutputLocation = " + currentInstanceOutputLocation, LogSeverity.Error, methodName: "GenerateOutputFiles");
                    return false;
                }

                currentInstanceOutputLocation = fileOutputLocation + '\\' + DateTime.Today.ToString("yyyy-MM-dd") + '-' + retryCount.ToString();
                retryCount += 1;
            }

            //Create Storage Folder
            DirectoryInfo currentInstanceStorageInfo;
            try
            {
                currentInstanceStorageInfo = Directory.CreateDirectory(currentInstanceOutputLocation);
            }
            catch (Exception ex)
            {
                logger.Write("Directory Issue", ex.ToString(), LogSeverity.Error, LogType.General);
                return false;
            }

            FileStream fs;
            StreamWriter sw;

            try
            {
                fs = new FileStream(currentInstanceStorageInfo.FullName + "\\geds.ce1", FileMode.Create);
                sw = new StreamWriter(fs, Encoding.GetEncoding("ISO-8859-1"));
            }
            catch (Exception ex)
            {
                logger.Write("File Issue", ex.ToString(), LogSeverity.Error, LogType.General);
                return false;
            }

            //Create Structure File
            List<ExternalPlugin.IGedUnit> orderedGedUnits = repository.Structures.Properties
                .Where((o) => o.Value.Ignore == false)
                .OrderBy((o) => o.Value.Order)
                .Select((o) => o.Value)
                .ToList();

            sw.WriteLine(GenerateGEDStructureHeader());
            int added = GenerateOutputFileForList(orderedGedUnits, repository.StructureEntities, sw);
            sw.WriteLine(GenerateGEDStructureFooter(added.ToString()));

            sw.Close();
            sw.Dispose();
            fs.Close();
            fs.Dispose();

            try
            {
                fs = new FileStream(currentInstanceStorageInfo.FullName + "\\geds.ce2", FileMode.Create);
                sw = new StreamWriter(fs, Encoding.GetEncoding("ISO-8859-1"));
            }
            catch (Exception ex)
            {
                logger.Write("File Issue", ex.ToString(), LogSeverity.Error, LogType.General);
                return false;
            }

            //Create Component File
            orderedGedUnits = repository.Components.Properties
                .Where((o) => o.Value.Ignore == false)
                .OrderBy((o) => o.Value.Order)
                .Select((o) => o.Value)
                .ToList();

            sw.WriteLine(GenerateGEDComponentHeader());
            added = GenerateOutputFileForList(orderedGedUnits, repository.ComponentEntities, sw);
            sw.WriteLine(GenerateGEDComponentFooter(added.ToString()));

            sw.Close();
            sw.Dispose();
            fs.Close();
            fs.Dispose();

            fileOutputData.RootLocation = currentInstanceStorageInfo.FullName;
            fileOutputData.StructureLocation = currentInstanceStorageInfo.FullName + "\\geds.ce1";
            fileOutputData.ComponentLocation = currentInstanceStorageInfo.FullName + "\\geds.ce2";

            return true;
        }

        private int GenerateOutputFileForList(List<ExternalPlugin.IGedUnit> orderedGedUnits, List<Dictionary<string, ExternalPlugin.IGedsEntity>> list, StreamWriter sw)
        {
            int addedCount = 0;

            if (orderedGedUnits != null)
            {
                foreach (var entity in list)
                {
                    List<string> outputValues = new List<string>();

                    for (int x = 0; x < orderedGedUnits.Count; x++)
                    {
                        if (!entity.ContainsKey(orderedGedUnits[x].FieldName)) //log here
                            break;

                        ExternalPlugin.IGedsEntity entityValue = entity[orderedGedUnits[x].FieldName];

                        if (entityValue.ValidateEntity() == false) //log here
                            break;

                        outputValues.Add(entityValue.Value);
                    }

                    if (outputValues.Count == orderedGedUnits.Count)
                    {
                        addedCount += 1;
                        sw.WriteLine(String.Join(",", outputValues.Select(e => "\"" + e + "\"").ToArray()));
                    }
                }
            }

            return addedCount;
        }

        private string GenerateGEDComponentHeader()
        {
            string headerRecordIdentifier = "00";
            string fileType = "2";
            string cdfVersion = "2.2";
            string creationDate = DateTime.Today.ToString("yyyy.MM.dd");
            string departmentalAcronym = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_HEADER_DEPARTMENTAL_ACRONYM);
            string directoryType = string.Empty;
            string sourceAgentName = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_HEADER_SOURCE_AGENT_NAME);

            string[] header = new string[] { headerRecordIdentifier, fileType, cdfVersion, creationDate, departmentalAcronym, directoryType, sourceAgentName };

            return string.Join(",", header.Select(x => "\"" + x + "\"").ToArray());
        }

        private string GenerateGEDComponentFooter(string numberOfRecords)
        {
            string footerRecordIdentifier = "99";

            string[] footer = new string[] { footerRecordIdentifier, numberOfRecords };
            return string.Join(",", footer.Select(x => "\"" + x + "\"").ToArray());
        }

        private string GenerateGEDStructureHeader()
        {
            string headerRecordIdentifier = "00";
            string fileType = "1";
            string cdfVersion = "2.2";
            string creationDate = DateTime.Today.ToString("yyyy.MM.dd");
            string departmentalAcronym = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_HEADER_DEPARTMENTAL_ACRONYM);
            string directoryType = string.Empty;
            string sourceAgentName = unitOfWork.Repository<Setting>().GetSettingValue(SettingsRepository.SettingTypes.GEDS_HEADER_SOURCE_AGENT_NAME);

            string[] header = new string[] { headerRecordIdentifier, fileType, cdfVersion, creationDate, departmentalAcronym, directoryType, sourceAgentName };

            return string.Join(",", header.Select(x => "\"" + x + "\"").ToArray()); 
        }

        private string GenerateGEDStructureFooter(string numberOfRecords)
        {
            string footerRecordIdentifier = "99";

            string[] footer = new string[] { footerRecordIdentifier, numberOfRecords };
            return string.Join(",", footer.Select(x => "\"" + x + "\"").ToArray());
        }

        #endregion

        private void UpdateTimerSettings()
        {
            if (jobTimer == null)
                return;

            var settingEntity = unitOfWork.Repository<Setting>();

            string newJobTimer = settingEntity.GetSettingValue(SettingsRepository.SettingTypes.GEDS_AUTO_JOB_SECS);

            if (newJobTimer != null)
            {
                long seconds;
                if(long.TryParse(newJobTimer, out seconds))
                {
                    jobTimer.Change(seconds, Timeout.Infinite);
                }
            }
        }
    }
}
