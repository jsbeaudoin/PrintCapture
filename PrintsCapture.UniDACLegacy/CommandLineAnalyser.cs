using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintsCapture.UniDACLegacy
{
    public class CommandLineAnalyser
    {

        public CommandLineAnalyser(params CommandLineArgument[] args)
        {
            this.Values = new Dictionary<string, CommandLineArgument>();

            foreach (var argument in args)
            {
                this.Values.Add(argument.Name, argument);
            }

        }

        public void Analyze(params string[] commandLineArgs)
        {
            foreach (var arg in commandLineArgs)
            {
                var lowerArg = arg.ToLowerInvariant();
                foreach (var cmdArg in this.Values.Values)
                {
                    if (lowerArg.StartsWith(cmdArg.ArgumentLine))
                    {
                        var pos = arg.IndexOf(':');
                        if (pos > 0)
                        {
                            cmdArg.Value = arg.Substring(pos + 1);
                        }
                    }
                }

            }
        }

        public Dictionary<string, CommandLineArgument> Values { get; private set; }

    }

    public class CommandLineArgument
    {
        public CommandLineArgument(string name, string commandLine)
        {
            this.Name = name;
            this.ArgumentLine = commandLine.ToLowerInvariant();
        }

        public string Name { get; set; }

        public string ArgumentLine { get; set; }

        public string Value { get; set; }

        public bool IsDefined { get; set; }
    }
}
