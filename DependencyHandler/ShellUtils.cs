using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class ShellUtils
    {
        public static string RunCommandAndGetOutput(string command, string arguments, string workingDirectory)
        {
            Logger.Message("Running command '" + command + " " + arguments + "' in dir '" + workingDirectory + "'");

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.CreateNoWindow = true;
            processInfo.FileName = command;
            processInfo.WorkingDirectory = workingDirectory;
            processInfo.Arguments = arguments;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;

            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            if (process.ExitCode != 0)
            {
                Logger.Message("stdout = " + output + " stderr = " + process.StandardError.ReadToEnd());
                throw new InvalidOperationException("Exit code " + process.ExitCode + " from command '" + command + " " + arguments + "' in dir " + workingDirectory);
            }
            process.Close();
            return output;
        }
    }
}
