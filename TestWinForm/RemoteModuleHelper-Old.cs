using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using RemoteModules;
using UniBIO.Services.Communication.BiometricService;
using XL_ID.Utilities.Log;
using XL_ID.Utilities.XML;

namespace TestWinForm
{
    public class RemoteModuleHelper
    {
        const string TakePictureModuleName = "TakePicture.Remote";

        const string PrintsCaptureModuleName = "PrintsCapture";

        public delegate void CaptureResultHandler(bool success, object resultObject, Exception ex);

        private delegate void CommandReceivedHandler(Dictionary<string, string> result);

        private CommandReceivedHandler commandReceived = null;

        private RemoteModule remoteModule;

        private bool eventTriggered = false;

        private readonly string language;
        private readonly bool isDebug;

        public RemoteModuleHelper(string language, bool isDebug)
        {
            this.language = language;
            this.isDebug = isDebug;
        }

        public void GetLivePrints()
        {
            this.commandReceived = CommandResultPrints;
            var args = this.GetPrintsCaptureArguments("live");
            args.Add("capturemode", "2");
            this.StartRemote(PrintsCaptureModuleName, args);
        }

        public void GetSinglePrint()
        {
            this.commandReceived = CommandResultPrints;
            var args = this.GetPrintsCaptureArguments("live");
            args.Add("capturemode", "8");
            this.StartRemote(PrintsCaptureModuleName, args);
        }

        /// <summary>
        /// Take an image using a camera. Event 'ImageCaptureCompleted' is triggered when done.
        /// ResultObject is a Bitmap instance.
        /// </summary>        
        public void TakePicture()
        {
            this.commandReceived = CommandResulImage;
            this.StartRemote(TakePictureModuleName, this.GetTakePictureArguments("photo"));
        }

        public event CaptureResultHandler CaptureCompleted;

        private Dictionary<string, string> GetTakePictureArguments(string cmdType)
        {
            var d = new Dictionary<string, string>();
            d.Add("cmd", cmdType);
            d.Add("lang", this.language);
            d.Add("debug", this.isDebug ? "1" : "0");

            return d;
        }

        private Dictionary<string, string> GetPrintsCaptureArguments(string cmdType)
        {
            var d = new Dictionary<string, string>();
            d.Add("mode", cmdType);
            d.Add("culture", this.language);
            d.Add("debug", this.isDebug ? "1" : "0");
            d.Add("descriptionline1", "");
            d.Add("descriptionline2", "");
            d.Add("wizard", "1");
            return d;
        }

        private void StartRemote(string remoteModuleName, Dictionary<string, string> arguments)
        {
            this.eventTriggered = false;
            var manager = new RemoteModulesHost();
            manager.ServiceHost.Start();

            // Trouve tous les modules dans le dossier des modules
            var baseFolder =
                (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\").Replace(@"file:\",
                    String.Empty);


            var foundModules = RemoteModulesFinder.InDirectory(Path.Combine(baseFolder, "Modules"));
            var seekModule = foundModules?.FirstOrDefault(x => x.Name == remoteModuleName);

            if (foundModules == null || foundModules.Count == 0 || seekModule == null)
            {
                this.HandleError(remoteModuleName + " module not found !");
                //Console.WriteLine("Remote modules not found !");
                return;
            }

            remoteModule = manager.InitializeRemoteModule(seekModule);

            if (remoteModule == null)
            {
                this.HandleError(remoteModuleName + " could not be initialized !");
                //Console.WriteLine("TakePicture remote module not found !");
                return;
            }

            // start le plugin
            try
            {
                //Console.WriteLine("preparing client host");
                remoteModule.Connected += (s, e) =>
                {
                    Console.Write("Remote module host has connected to client");
                    Thread.Sleep(100);
                    remoteModule.SendCommand(arguments);
                    Console.Write("Command sent to remote client");
                };

                remoteModule.NewResult += RemoteModuleOnNewResult;
                remoteModule.Disconnected += (sender, args) =>
                {
                    Console.Write("Remote module disconnected");
                    if (!this.eventTriggered)
                    {
                        this.TriggerEvent(false, null, new ApplicationException("Module disconnected without event"));
                    }
                };

                remoteModule.Exited += exitcode =>
                {
                    Console.Write($"Remote Module exited with code : {exitcode}");
                    this.Close();
                };
                remoteModule.Started += (sender, args) => Console.Write("Remote module has been started");
                remoteModule.NotResponding += (sender, args) =>
                    Console.Write("Client not responding");
                remoteModule.Start();

            }
            catch (Exception e)
            {
                Console.WriteLine("Start Error : " + e.Message);
                this.HandleError("Could not start Module", e);
            }
        }

        private void TriggerEvent(bool success, object resultObject, Exception ex)
        {
            this.eventTriggered = true;
            this.CaptureCompleted?.Invoke(success, resultObject, ex);
        }

        private void HandleError(string errorMessage, Exception ex = null)
        {
            this.TriggerEvent(false, null, ex ?? new ApplicationException(errorMessage));
            this.Close();
        }



        private Bitmap Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
                imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Bitmap image = (Bitmap)Image.FromStream(ms, true);
            return image;
        }

        private void Close()
        {
            try
            {
                if (!this.eventTriggered)
                {
                    this.TriggerEvent(false, null, new ApplicationException("Capture stopped without result"));
                }
            }
            catch (Exception ex)
            {
                Console.Write("RemoteModuleHandler Close TriggerEvent failed : " + ex.Message);
            }

            try
            {
                this.CaptureCompleted = null;
                this.remoteModule?.Close();
                this.remoteModule = null;
            }
            catch (Exception ex)
            {
                Console.Write("Closing remoteModule produced an error : " + ex.Message);
            }
        }

        private void RemoteModuleOnNewResult(Dictionary<string, string> result)
        {

            if (!result.ContainsKey("success"))
            {
                this.HandleError("Wrong version of module. Results must include an entry : 'success'");
                return;
            }


            if (result["success"] != "1" && result["success"] != "true")
            {

                var msg = result.ContainsKey("message") ? result["message"] : "Unknown error in module";
                this.HandleError(msg);
                return;
            }

            this.commandReceived(result);
        }

        private void CommandResultPrints(Dictionary<string, string> result)
        {
            if (!result.ContainsKey("data"))
            {
                this.HandleError("Wrong version of module. Results must include entry : 'data' ");
                return;
            }

            CapturedPrintData resultData = null;
            try
            {
                var serialized = result["data"];
                resultData = XmlSerializer.Deserialize<CapturedPrintData>(serialized);
            }
            catch (Exception ex)
            {
                this.HandleError("Could not process prints result : " + ex.Message, ex);
                return;
            }

            this.TriggerEvent(true, resultData, null);
        }

        private void CommandResulImage(Dictionary<string, string> result)
        {
            if (!result.ContainsKey("imagecount") || !result.ContainsKey("image0"))
            {
                this.HandleError("Wrong version of module. Results must include entries : 'imagecount', 'image0' ");
                return;
            }

            Bitmap bmp = null;
            try
            {
                var imgText = result["image0"];
                if (!string.IsNullOrEmpty(imgText))
                {
                    bmp = this.Base64ToImage(result["image0"]);
                    if (bmp == null)
                    {
                        throw new ApplicationException("Converted image cannot be processed");
                    }
                }

            }
            catch (Exception ex)
            {
                this.HandleError("Could not read image : " + ex.Message);
                return;
            }


            this.TriggerEvent(true, bmp, null);
        }

        //private void CommandResultPerson(Dictionary<string, string> result)
        //{
        //    object resultObject = null;
        //    try
        //    {
        //        if (result.ContainsKey("person"))
        //        {
        //            resultObject = XmlSerializer.Deserialize<PersonInformation>(result["person"]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.HandleError("Could not read person : " + ex.Message);
        //        return;
        //    }

        //    this.TriggerEvent(true, resultObject, null);
        //}

    }
}
