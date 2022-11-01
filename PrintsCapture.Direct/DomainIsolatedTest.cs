using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniBIO.Services.Communication.BiometricService;

namespace PrintsCapture.Direct
{
    //[Serializable]
    //public class CapturedPrintDataWrapper
    //{
    //    public CapturedPrintData Data { get; set; }
    //}

    public class DomainIsolatedClass : MarshalByRefObject, IDomainIsolatedClass
    {
        PrintCaptureDllApp2 test;

        public CapturedPrintData GetResults()
        {
            return test.GetResults();
            
        }

        public void StartCapture(Dictionary<string, string> arguments)
        {
            Thread t = new Thread(new ThreadStart(() => {
                test = new PrintCaptureDllApp2();
                test.StartPrintCapture(arguments);
                //test.ShutdownWPF();
            }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            t.Join(); // wait for the printcapture app to end. else will try to return values before the capture has even started   
        }
    }

    public interface IDomainIsolatedClass
    {
        CapturedPrintData GetResults();
        void StartCapture(Dictionary<string, string> arguments);
    }
    
    [Serializable]
    public class DomainPrintCapture: IDisposable 
    {
        private AppDomain childDomain;

        public CapturedPrintData CaptureData(Dictionary<string, string> arguments)
        {
            try
            {
                AppDomainSetup domainSetup = new AppDomainSetup()
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                    LoaderOptimization = LoaderOptimization.MultiDomainHost
                };

                // Create the child AppDomain used for the service tool at runtime.
                childDomain = AppDomain.CreateDomain("PrintCapture personal AppDomain", null, domainSetup);
                childDomain.UnhandledException += ChildDomain_UnhandledException;

                // Create an instance of the runtime in the second AppDomain. 
                // A proxy to the object is returned.
                IDomainIsolatedClass runtime = (IDomainIsolatedClass)childDomain.CreateInstanceAndUnwrap(
                    typeof(DomainIsolatedClass).Assembly.FullName, typeof(DomainIsolatedClass).FullName);
                //GC.SuppressFinalize(runtime);
                // start the runtime.  call will marshal into the child runtime appdomain
                runtime.StartCapture(arguments);
                Console.WriteLine("App Domain job done");
                var data = runtime.GetResults();
                runtime = null;
                return data; //  runtime.GetResults().Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("App DOmain job failed !!!!!!!!!!!!!!");
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                try
                {
                    
                    AppDomain.Unload(childDomain);
                    childDomain = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("App DOmain exception while unloading !");                    
                }
             }

            

        }

        private void ChildDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("ERROR : " + e.ExceptionObject.ToString());
        }

        //public void Dispose()
        //{
        //    AppDomain.Unload(childDomain);
        //}
        public void Dispose()
        {
            if(childDomain != null) AppDomain.Unload(this.childDomain);
            GC.SuppressFinalize(this);
            GC.Collect();
        }
    }
}
