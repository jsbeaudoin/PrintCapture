namespace PrintsCapture.LocalNeuroSeqCheck
{
    using System;
    using System.IO;
    using System.Reflection;

    public enum NeuroLicenceType
    {
        Biometrics_FingerExtraction,
        Biometrics_PalmExtraction,
        Biometrics_FaceExtraction,
        Biometrics_FingerMatching,
        Biometrics_PalmMatching,
        Biometrics_FaceMatching,
        Biometrics_FingerSegmentation,
        Biometrics_PalmSegmentation,
        Biometrics_FaceSegmentation,
        Biometrics_Standards_Fingers,
        Biometrics_Standards_FingerTemplates,
        Biometrics_Standards_Faces
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class NeuroLicence
    {
        private static string neuroServerAddress;

        private static int neuroServerPort;

        private static object locker = new object();

        /// <summary>
        /// All licenses required by the UniABIS Server
        /// </summary>
        private static readonly string[] UniAbisServerLicenses = 
            {
                "Biometrics.FingerExtraction",
                "Biometrics.FingerMatching",
                "Biometrics.FingerSegmentation",
                "Biometrics.Standards.Fingers",
                "Biometrics.Standards.FingerTemplates",
            };


        /// <summary>
        /// Client required licenses. Same as server, but without matching
        /// </summary>
        private static readonly string[] UniAbisClientLicenses = 
            {
                "Biometrics.FingerExtraction",
                "Biometrics.FingerSegmentation",
                "Biometrics.Standards.Fingers",
                "Biometrics.Standards.FingerTemplates",
            };

        /// <summary>
        /// List of activated licences on Neuro Licensing server
        /// </summary>
        private static string[] activatedLicenses;

        /// <summary>
        /// Activates the neuro licenses.
        /// </summary>        
        /// <param name="neuroLicensePort">The neuro server license port.</param>
        /// <param name="isServer">if set to <c>true</c> [is server].</param>
        /// <returns>Whether the activation was successful or not</returns>
        /// <remarks>Not Thread safe.</remarks>
        public static bool ActivateLicenses(int neuroLicensePort, bool isServer)
        {
            try
            {
                neuroServerAddress = "/local";
                neuroServerPort = neuroLicensePort;
                return ActivateLicenses(neuroServerAddress, neuroLicensePort, isServer ? UniAbisServerLicenses : UniAbisClientLicenses);
            }
            catch (Exception ex)
            {
                var errorMessage = GetStateFromException(ex);
                throw new ApplicationException(errorMessage == string.Empty ? ex.Message : errorMessage);
            }
        }

        /// <summary>
        /// Checks the licensing activation. Sometimes, after awhile, licenses are freed. Call before using Neuro.
        /// </summary>
        /// <remarks>Thread safe. Licenses must have been activated before using that method</remarks>
        public static bool CheckLicensing()
        {
            //Logging.Logger.Info(@"CheckLicensing called");
            //Logging.Logger.Debug("activatedLicenses null ? : {0}", activatedLicenses == null);
            //Logging.Logger.Debug("UniAbisServerLicenses null ? : {0}", UniAbisServerLicenses == null);
            var licenses = activatedLicenses ?? UniAbisServerLicenses;
            if (licenses == null)
            {
                throw new ApplicationException("Licenses not initialized. Call ActivateLicenses before CheckLicensing");
            }

            var activated = true;
            lock (locker)
            {                
                foreach (string license in licenses)
                {
                    activated &= Neurotec.Licensing.NLicense.IsComponentActivated(license);
                }

                if (!activated)
                {
                    activated = ActivateLicenses(neuroServerAddress, neuroServerPort, licenses);
                }
                
            }

            return activated;
        }

        /// <summary>
        /// Determines whether the specified neuro component is activated
        /// </summary>
        /// <param name="neuroComponent">The neuro component.</param>
        /// <returns>
        /// 	<c>true</c> if the specified neuro component is active; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsComponentActivated(NeuroLicenceType neuroComponent)
        {
            var compName = neuroComponent.ToString().Replace('_', '.');

            return Neurotec.Licensing.NLicense.IsComponentActivated(compName);
        }

        /// <summary>
        /// Releases the activated neuro licenses.
        /// </summary>
        public static void ReleaseLicenses()
        {
            //Logging.Logger.Info(@"ReleaseLicenses called");
            if (activatedLicenses == null)
            {
                return;
            }

            try
            {
                foreach (string license in activatedLicenses)
                {
                    Neurotec.Licensing.NLicense.ReleaseComponents(license);
                }
            }
            catch (Exception)
            {
                // nothing
            }
        }

        /// <summary>
        /// Activates the licenses of Neuro.
        /// </summary>
        /// <param name="neuroAddress">The neuro address.</param>
        /// <param name="neuroLicensePort">The neuro license port.</param>
        /// <param name="licenses">The license list to activate.</param>
        /// <returns>
        /// Whether the activation was successful or not
        /// </returns>
        private static bool ActivateLicenses(string neuroAddress, int neuroLicensePort, string[] licenses)
        {
            var currentPath = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\").Replace(@"file:\", String.Empty);
            Environment.SetEnvironmentVariable("PATH", currentPath + ";" + Environment.GetEnvironmentVariable("PATH"), EnvironmentVariableTarget.Process);
            //Logging.Logger.Info(@"ActivateLicenses v2 called with {0} licences at address:{1}", licenses.Length, neuroAddress);
            int count = 0;
            bool retry = false;

            do
            {
                try
                {
                    count++;
                    foreach (string license in licenses)
                    {
                        Neurotec.Licensing.NLicense.ObtainComponents(neuroAddress, neuroLicensePort, license);
                        //Logging.Logger.Debug(@"License obtained : {0}", license);
                    }
                }
                catch (Exception ex)
                {
                    //Logging.Logger.Debug("Licences activation failed : {0}", ex.Message);
                    string message = string.Format("Failed to obtain licenses for components.\nError message: {0}", ex.Message);
                    if (ex is System.IO.IOException)
                    {
                        message += "\n(Probably licensing service is not running. Use Activation Wizard to figure it out.)";
                    }

                    //Logging.Logger.Debug("Could not activate license. \n" + message);

                    if (count > 2)
                    {
                        return false;
                    }
                    else
                    {
                        retry = true;
                    }
                }
            }
            while (retry);

            activatedLicenses = licenses;
            //Logging.Logger.Info(@"ActivateLicenses activated {0} licences", activatedLicenses == null ? 0 : activatedLicenses.Length);
            return true;
        }

        private static string GetStateFromException(Exception ex)
        {
            string message = string.Empty;

            if (ex is AggregateException)
            {
                var aggEx = (AggregateException)ex;
                if (aggEx.InnerException != null)
                {
                    ex = aggEx.InnerException;
                }
            }

            if (ex is System.IO.IOException)
            {
                message += "Probably licensing service is not running. Use Activation Wizard to figure it out. ";
            }

            if (ex.InnerException is DllNotFoundException)
            {
                message += "Missing dll for Neuro license : " + ex.InnerException.Message + ". ";
            }

            if (ex.InnerException is BadImageFormatException)
            {
                message += "Wrong dll format for Neuro license: Could be x64 dll instead of x86 " + ". ";
            }

            return message;
        }
    }
}
