using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.RemoteModuleForApi
{
    using CommandLine;

    public class CommandLineOptions
    {
        [Option('t', "token", Required = true, HelpText = "Identification token for service bus")]
        public string Token { get; set; }

        [Option('a', "address", Required = true, HelpText = "Address of service bus")]
        public string EndPointAddress { get; set; }

        [Option('l', "login", Required = true, HelpText = "Login name for service bus")]
        public string User { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password for service bus")]
        public string Password { get; set; }

        [Option('s', "shared-accesskey", Required = false, HelpText = "Is the password a shared access key ?")]
        public bool IsSharedAccessKey { get; set; }

        [Option('c', "culture", Required = true, HelpText = "Culture name for user display")]
        public string CultureIdentifier { get; set; }

        [Option('r', "reference", Required = true, HelpText = "External Reference for printset")]
        public string ReferenceId { get; set; }

        [Option('u', "url", Required = true, HelpText = "Url where results are returned")]
        public string CallbackUrl { get; set; }

        [Option('1', "desc1", Required = false, HelpText = "Line 1 of description shown in top right corner")]
        public string Description1 { get; set; }

        [Option('2', "desc2", Required = false, HelpText = "Line 2 of description shown in top right corner")]
        public string Description2 { get; set; }

        [Option('i', "simple-ui", Required = false, HelpText = "Is the Ui the simple Ui for flat only ?")]
        public bool IsSimpleUi { get; set; }

        [Option('l', "login", Required = false, HelpText = "Is the Ui the simple Ui for login mode ?")]
        public bool IsLoginMode { get; set; }

        //[Option('n', "named-pipe", Required = false, HelpText = "Named pipe where results are returned")]
        //public string NamedPipe { get; set; }

        //public bool IsNamedPipeUsed { get; private set; }

        //public bool IsUrlUsed { get; private set; }

        public bool Validate()
        {
            //if (string.IsNullOrEmpty(this.CallbackUrl) && string.IsNullOrEmpty(this.NamedPipe))
            //{
            //    Console.WriteLine("Url or namedpipe must be provided");
            //    return false;                
            //}

            //if (!string.IsNullOrEmpty(this.CallbackUrl))
            //{
            //    this.IsUrlUsed = true;
            //}
            //else
            //{
            //    this.IsNamedPipeUsed = true;
            //}

            return true;
        }

    }
}
