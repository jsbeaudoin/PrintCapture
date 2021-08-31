using PrintsCapture.Ui.Exceptions;

namespace PrintsCapture.Prints.Sdk
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using PrintsCapture.Ui.Class;

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
            var startTime = DateTime.Now;
            TimeSpan duration;


            this.pluginPath = pluginPath;
            var baseCatalog = new AggregateCatalog();
            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info(
                $"New Aggregation Process Time (mm:ss:ff) : {duration.ToString("mm\\:ss\\:ff")}");
            //baseCatalog.Catalogs.Add(new DirectoryCatalog("."));

            // load plugins directories
            //var pluginFolderName = Path.GetFileName(this.pluginPath.TrimEnd(Path.DirectorySeparatorChar));

            if (!Directory.Exists(this.pluginPath))
            {
                throw new PluginPathNotFoundException(this.pluginPath);                
            }

            startTime = DateTime.Now;
            string paths = string.Empty;
            var folders = Directory.GetDirectories(this.pluginPath).ToList();
            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info($"Found {folders.Count} plugins folder. Process Time (mm:ss:ff):  {duration.ToString("mm\\:ss\\:ff")}");
            
            startTime = DateTime.Now;
            foreach (string folder in folders)
            {                
                baseCatalog.Catalogs.Add(new DirectoryCatalog(folder));
                paths = (string.IsNullOrEmpty(paths) ? "" : paths + ";") + folder;
            }
            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info($"Adding folders to catalog. Process Time (mm:ss:ff):  {duration.ToString("mm\\:ss\\:ff")}");
            PrintCaptureAppLog.Logger.Trace($"Plugins Folder Path: {paths}");

            // make sure that referred native dlls can be loaded from plugins folder
            Environment.SetEnvironmentVariable("PATH", "%PATH%;" + paths, EnvironmentVariableTarget.Process);

            startTime = DateTime.Now;
            this.container = new CompositionContainer(baseCatalog);
            duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info($"new CompositionCountainer. Process Time (mm:ss:ff):  {duration.ToString("mm\\:ss\\:ff")}");

            foreach (var catalogPart in this.container.Catalog.Parts)
            {
                PrintCaptureAppLog.Logger.Info(catalogPart);
            }
        }

        public void ResolvePlugins(object annotatedClass)
        {
            if (this.container == null)
            {
                return;
            }

            var startTime = DateTime.Now;
            this.container.ComposeParts(annotatedClass);
            var duration = DateTime.Now.Subtract(startTime);
            PrintCaptureAppLog.Logger.Info($"ResolvePlugins. SDKs Count : {((PrintCaptureApp)annotatedClass).Sdks.Count} Process Time (mm:ss:ff):{duration.ToString("mm\\:ss\\:ff")}");
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
}



