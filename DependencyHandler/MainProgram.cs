#define DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class MainProgram
    {
        static void Main(string[] arguments)
        {
            string currentDirectory  = Environment.CurrentDirectory;
#if DEBUG
            if (arguments.Count() == 0)
                arguments = new string[] {"checkout", "a" };
            currentDirectory = Directory.GetParent(currentDirectory).Parent.FullName + "\\test";
#endif
            XmlObjectParser objectParser = XmlObjectParser.Default;
            objectParser.RegisterClass("Project", (new Project()).GetType());
            objectParser.RegisterClass("String", (new ConstantValue_Provider<string>(null)).GetType());
            objectParser.RegisterClass("Url", (new FileLocation()).GetType());
            objectParser.RegisterClass("DependencyList", (new ParseableList<ProjectDescriptor>()).GetType());
            objectParser.RegisterClass("Git", (new GitSyncher()).GetType());
            objectParser.RegisterClass("GitRepo", (new GitSyncher()).GetType());
            //objectParser.RegisterClass("GitProject", (new GitRepoToProjectConverter()).GetType());
            
            ArgumentParser argumentParser = new ArgumentParser(new DependencyHandler(objectParser));
            argumentParser.ProcessArguments(currentDirectory, arguments);
            Logger.Message("done");
        }
    }
}
