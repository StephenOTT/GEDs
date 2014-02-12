using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ServiceProcess;

using GEDs.ViewModel;
using Data;
using Entities.Models;
using Repository;
using GEDs.Helpers;
using System.IO;

namespace GEDs.Controllers
{
    public class HomeController : Controller
    {
        private IUnitOfWork unitOfWork;

        public HomeController()
        {
            unitOfWork = new UnitOfWork(new GedsContext());
        }

        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();

            model.ServiceStatus = GetServiceStatus();
            model.Jobs = unitOfWork.Repository<Job>()
                               .Query()
                               .AsNoTracking()
                               .OrderBy(o => o.OrderByDescending(ob => ob.Id))
                               .Get();

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Service(bool status)
        {
            SetServiceStatus(status);

            return RedirectToAction("Index");
        }

        public ActionResult Demo(string guid, string ou = null, string cn = null)
        {
            if (string.IsNullOrEmpty(guid))
                return HttpNotFound();

            TreeNode<Node> treeModel = GenerateDemoModel(guid, ou, cn);

            if (treeModel == null)
                return HttpNotFound();

            if (!string.IsNullOrEmpty(ou))
            {
                treeModel = treeModel.Descendants().Where(n => n.Value.Structure.GUID == ou).FirstOrDefault();
                if (treeModel == null)
                    return HttpNotFound();
            }

            DemoViewModel model = new DemoViewModel();
            model.Guid = guid;
            model.Organizations = new List<Helpers.Structure>();
            model.People = new List<Helpers.Component>();
            model.Parents = new List<Helpers.Structure>();

            //current organization
            model.Organization = treeModel.Value.Structure;
            
            //child organizations
            foreach (var child in treeModel.Children)
            {
                model.Organizations.Add(child.Value.Structure);
            }

            //current organization people
            foreach (var people in treeModel.Value.Components)
            {
                model.People.Add(people);
            }

            //build no user model
            if (!string.IsNullOrEmpty(cn))
            {
                foreach (var people in treeModel.Value.Components)
                {
                    if (people.Name.Equals(cn))
                    {
                        model.User = people;
                        break;
                    }
                }
            }

            //build navigation list
            List<Helpers.Structure> parents = new List<Helpers.Structure>();
            do
            {
                parents.Add(treeModel.Value.Structure);
                treeModel = treeModel.Parent;
            } while (treeModel != null);

            model.Parents = parents;

            return View(model);
        }

        public ActionResult Download(int id, string type)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return HttpNotFound();

            Job job = unitOfWork.Repository<Job>().FindById(id);
            if (job == null)
                return HttpNotFound();

            string file = null;
            string fileName = null;

            if (type.Equals("ce2"))
            {
                file = job.ComponentFileLocation;
                fileName = "geds.ce2";
            }
            else
            {
                file = job.StructureFileLocation;
                fileName = "geds.ce1";
            }

            if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(fileName))
                return HttpNotFound();

            return File(file, "application/text", fileName);
        }

        protected override void Dispose(bool disposing)
        {
            if (unitOfWork != null)
                unitOfWork.Dispose();

            base.Dispose(disposing);
        }


        private bool GetServiceStatus()
        {
            ServiceController sc;

            try
            {
                sc = new ServiceController("FH.Geds.Service");
                if (sc.Status == ServiceControllerStatus.Running)
                    return true;
            }
            catch (ArgumentException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            catch (System.ComponentModel.Win32Exception wex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(wex);
            }
            catch (InvalidOperationException ioe)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ioe);
            }
            
            return false;
        }

        /// <summary>
        /// Start or Stop the service
        /// </summary>
        /// <param name="status">true = start, false = stop</param>
        private void SetServiceStatus(bool status)
        {
            if (!User.IsInRole(System.Web.Configuration.WebConfigurationManager.AppSettings["AdminDomainGroup"]))
                return;

            ServiceController sc;

            try
            {
                sc = new ServiceController("FH.Geds.Service");
                if (status)
                    sc.Start();
                else sc.Stop();
            }
            catch (ArgumentException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            catch (System.ComponentModel.Win32Exception wex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(wex);
            }
            catch (InvalidOperationException ioe)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ioe);
            }
        }


        #region DEMO

        private TreeNode<Node> GenerateDemoModel(string id, string ou, string cn)
        {
            Job job = unitOfWork.Repository<Job>().Query()
                        .Filter(f => f.Guid == id)
                        .Get()
                        .FirstOrDefault();

            if (job == null)
                return null;

            List<string> structureContents = RetrieveFileContents(job.StructureFileLocation);
            List<string> componentContents = RetrieveFileContents(job.ComponentFileLocation);

            if (structureContents == null || componentContents == null)
                return null;

            if (structureContents.Count == 0 || componentContents.Count == 0)
                return null;

            TreeNode<Node> tree = BuildTree(structureContents, componentContents);

            return tree;
        }

        private TreeNode<Node> BuildTree(List<string> structureContents, List<string> componentContents)
        {
            List<GEDs.Helpers.Structure> structures = new List<GEDs.Helpers.Structure>();
            for (int x = 1; x < structureContents.Count - 1; x++) //skip header and footer
            {
                string csvLine = structureContents[x];
                string[] csvData = CsvSplit(csvLine);

                if (csvData.Length > 28) //build structure, always should be greater than 5
                {
                    GEDs.Helpers.Structure s = new GEDs.Helpers.Structure();
                    s.Name = csvData[2];
                    s.FName = csvData[3];
                    s.GUID = csvData[4];
                    s.PGUID = csvData[5];
                    s.Sequence = csvData[28];

                    structures.Add(s);
                }
            }

            //build component
            List<GEDs.Helpers.Component> components = new List<GEDs.Helpers.Component>();
            for (int x = 1; x < componentContents.Count - 1; x++)
            {
                string csvLine = componentContents[x];
                string[] csvData = CsvSplit(csvLine);

                if (csvData.Length > 45)
                {
                    GEDs.Helpers.Component c = new GEDs.Helpers.Component();
                    c.Name = csvData[1] + ", " + csvData[2];
                    c.Title = csvData[8];
                    c.PGUID = csvData[10];
                    c.Phone = csvData[12];
                    c.Sequence = csvData[11];
                    c.Address = csvData[38];
                    c.City = csvData[40];
                    c.Province = csvData[42];
                    c.PostalCode = csvData[44];
                    c.Country = csvData[45];

                    components.Add(c);
                }
            }

            //find the root structure (GUID == PGUID, cannot be more than 1)
            GEDs.Helpers.Structure parent = structures.FirstOrDefault(s => s.GUID == s.PGUID);
            if (parent == null)
                return null;

            Node parentNode = new Node();
            parentNode.Structure = parent;

            TreeNode<Node> gedsTree = new TreeNode<Node>(parentNode);

            //build remaining structures now
            AddStructureNode(parent, gedsTree, structures, components);

            return gedsTree;
        }

        private void AddStructureNode(GEDs.Helpers.Structure parent, TreeNode<Node> tree,
            List<GEDs.Helpers.Structure> structures, List<GEDs.Helpers.Component> components)
        {
            if (parent == null || tree == null)
                return;

            if (structures == null || components == null)
                return;

            List<GEDs.Helpers.Structure> siblings = structures.Where(s => s.PGUID == parent.GUID && s.GUID != s.PGUID).ToList();

            for (int x = 0; x < siblings.Count; x++)
            {
                Node siblingNode = new Node();
                siblingNode.Structure = siblings[x];

                List<GEDs.Helpers.Component> siblingComponents = components.Where(c => c.PGUID == siblingNode.Structure.GUID).ToList();
                foreach (GEDs.Helpers.Component siblingComponent in siblingComponents.OrderByDescending(c => c.Sequence).ThenBy(c => c.Name).ToList())
                {
                    siblingNode.Components.Add(siblingComponent);
                }

                TreeNode<Node> treeChild = tree.AddChild(siblingNode);

                AddStructureNode(siblings[x], treeChild, structures, components);
            }

            //tree.Children.Sort((a, b) => a.Value.Structure.Sequence.CompareTo(b.Value.Structure.Sequence));
            tree.Children.Sort(
                delegate(TreeNode<Node> a, TreeNode<Node> b)
                {
                    int sort = a.Value.Structure.Sequence.CompareTo(b.Value.Structure.Sequence);
                    return (sort == 0) ? a.Value.Structure.Name.CompareTo(b.Value.Structure.Name) : sort;
                }
            );
        }

        /// <summary>
        /// CsvSplit
        /// GEDS specific csv split. GEDS does not allow for quotes inside strings.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string[] CsvSplit(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new string[] { "" };

            List<string> strings = new List<string>();
            int startIndex = 0;
            int length = 0;
            bool skipping = true;

            for (int x = 0; x < data.Length; x++)
            {
                char @char = data[x];

                if (@char == '"') //open or close, quotes are not allowed in GEDS strings
                {
                    if (skipping == false) //we are already open and parsing, now found a close quote
                    {
                        if (length == 0)
                            strings.Add(string.Empty);
                        else strings.Add(data.Substring(startIndex, length));
                        startIndex = 0;
                        length = 0;
                        skipping = true;
                        continue;
                    }
                    else
                    {
                        skipping = false; //open quote, begin parsing data in it.
                        startIndex = x + 1;
                        length = 0;
                        continue;
                    }
                }
                else if (@char == ',' && skipping == true)
                {
                    continue;
                }
                else
                {
                    length += 1;
                }
            }

            return strings.ToArray();
        }

        private List<string> RetrieveFileContents(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;

            if (!System.IO.File.Exists(file))
                return null;

            List<string> fileContents = new List<string>();

            try
            {
                StreamReader fileStream = new StreamReader(file, System.Text.Encoding.GetEncoding("ISO-8859-1"));

                while (!fileStream.EndOfStream)
                    fileContents.Add(fileStream.ReadLine());
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return fileContents;

        }

        #endregion
    }
}
