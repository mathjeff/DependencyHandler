using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class ShellUtils
    {
        public static void RunCommand(string command, string workingDirectory)
        {
            Logger.Message("Running command '" + command + "' in dir '" + workingDirectory + "'");
        }
    }
}
