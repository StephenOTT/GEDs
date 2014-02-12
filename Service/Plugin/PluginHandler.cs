using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Plugin;
using Entities.Models;

namespace Service.Plugin
{
    internal struct Plugin
    {
        public string Name;
        public IGedsPlugin Instance;
        public AppDomain InstanceDomain;
    }

    internal class PluginHandler
    {
        public Dictionary<string, Plugin> plugins;
        private EventLog.Log logger;

        public PluginHandler()
        {
            plugins = new Dictionary<string, Plugin>();
            logger = EventLog.LogManager.GetClassLogger(this.GetType());
        }

        public IGedsPlugin GetPlugin(string pluginName)
        {
            if (plugins == null)
                return null;

            if (!plugins.ContainsKey(pluginName))
                return null;

            Plugin p = plugins[pluginName];
            return p.Instance;
        }

        public void LoadPlugins(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
                return;

            var pluginType = typeof(IGedsPlugin);

            foreach (string file in Directory.GetFiles(pluginDirectory, "*.dll"))
            {
                string pluginName = Path.GetFileNameWithoutExtension(file);
                //var domain = AppDomain.CreateDomain(pluginName);

                IGedsPlugin plugin = null;

                try
                {
                    foreach (Type assemblyType in Assembly.LoadFrom(file).GetTypes())
                    {
                        Type interfaceType = assemblyType.GetInterface(pluginType.FullName);

                        if (interfaceType != null)// && !(interfaceType.IsAbstract || interfaceType.IsInterface))
                        {
                            //plugin = domain.CreateInstanceFromAndUnwrap(file, pluginType.FullName) as IGedsPlugin;
                            plugin = Activator.CreateInstance(assemblyType) as IGedsPlugin;
                            break;
                        }
                    }
                    //plugin = domain.CreateInstanceFromAndUnwrap(file, pluginType.Name) as IGedsPlugin;
                    /*var asm = Assembly.LoadFile(@file);
                    var type = asm.GetType("FH.Geds.Parent.Guid.Lookup.GuidLookup");
                    var p2 = Activator.CreateInstance(type) as IGedsEntity;
                    plugin = Activator.CreateInstance(type) as IGedsPlugin;*/
                }
                catch (Exception ex)
                {
                    logger.Write("Plugin Handler Issue",
                        String.Format("File: {0}\n\n{1}", file, ex.ToString()),
                        LogSeverity.Error,
                        LogType.Plugin,
                        "LoadPlugins");
                    continue;
                }

                if (plugin == null)
                {
                    continue;
                }

                Plugin p = new Plugin();
                p.Name = pluginName;
                p.Instance = plugin;
                //p.InstanceDomain = domain;

                plugins.Add(p.Name, p);
            }
        }

        public override string ToString()
        {
            if (plugins == null)
                return String.Empty;

            return String.Join("\n", plugins.Keys.ToArray());
        }

        public void Dispose()
        {
            foreach (var kvp in plugins)
            {
                Plugin p = kvp.Value;
                p.Instance.Dispose();
                p.Instance = null;

                p.InstanceDomain = null;
            }
        }
    }
}
