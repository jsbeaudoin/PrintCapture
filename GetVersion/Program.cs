using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetVersion
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Reflection;

    namespace Version
    {
        class GetVersion
        {
            static void Main(string[] args)
            {
                if (args.Length == 0 || args.Length > 2) ShowUsage();

                string target = args[0];

                string path = Path.IsPathRooted(target)
                                    ? target
                                    : Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + Path.DirectorySeparatorChar + target;
                int fieldCount = 2;
                if (args.Length == 2)
                {
                    int.TryParse(args[1], out fieldCount);
                }
                Console.Write(Assembly.LoadFile(path).GetName().Version.ToString(fieldCount));
            }

            static void ShowUsage()
            {
                Console.WriteLine("Usage: GetVersion.exe <target> [<Field count>]");
            }
        }
    }
}
