namespace PrintsCapture.QuickCaptureControl.Class
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    // -----------------------------------------------------------------------
    // <copyright file="DynamicPluginLoader.cs" company="">
    // Loads all
    // </copyright>
    // -----------------------------------------------------------------------   

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PluginLoader
    {
        private readonly CompositionContainer container;

        private readonly string pluginPath;

        public PluginLoader(string pluginPath)
        {
            this.pluginPath = pluginPath;
            var baseCatalog = new AggregateCatalog();
            //baseCatalog.Catalogs.Add(new DirectoryCatalog("."));

            // load plugins directories
            //var pluginFolderName = Path.GetFileName(this.pluginPath.TrimEnd(Path.DirectorySeparatorChar));

            if (!Directory.Exists(this.pluginPath))
            {
                throw new PluginPathNotFoundException(this.pluginPath);                
            }

            string paths = string.Empty;
            var folders = Directory.GetDirectories(this.pluginPath).ToList();
            //PrintCaptureAppLog.Logger.Info("Found {0} plugins folder. ", folders.Count);
            
            foreach (string folder in folders)
            {                
                baseCatalog.Catalogs.Add(new DirectoryCatalog(folder));
                paths = (string.IsNullOrEmpty(paths) ? "" : paths + ";") + folder;
            }

            // make sure that referred native dlls can be loaded from plugins folder
            Environment.SetEnvironmentVariable("PATH", "%PATH%;" + paths, EnvironmentVariableTarget.Process);

            //this.container = new CompositionContainer(baseCatalog, CompositionOptions.DisableSilentRejection, null);
            this.container = new CompositionContainer(baseCatalog);
        }

        public void ResolvePlugins(object annotatedClass)
        {
            if (this.container == null)
            {
                return;
            }

            this.container.ComposeParts(annotatedClass);
        }

        internal TDynamicLoad ManualLoad<TDynamicLoad>(string assemblyFileName, params object[] constructArgsArray)
        {

            const BindingFlags BitMask = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance;

            // assemblyFileName = "CaptureApplication.Livescan.DriverEssential.dll";
            var typeName = typeof(TDynamicLoad).Name;

            var completeName = Path.Combine(this.pluginPath, assemblyFileName);

            return
                (TDynamicLoad)
                    Activator.CreateInstance(
                        completeName,
                        typeName,
                        false,
                        BitMask,
                        null,
                        constructArgsArray,
                        null,
                        null).Unwrap();

            /*
                 Activator.CreateInstanceFrom(
                    sPath + assFile,
                    typeName,
                    false,
                    bitMask,
                    null,
                    constructArgsArray,
                    null, null, null).Unwrap()
                 */
        }
    }
    public class PluginPathNotFoundException : ApplicationException
    {
        public PluginPathNotFoundException(string message) : base(message)
        { }
    }
}



