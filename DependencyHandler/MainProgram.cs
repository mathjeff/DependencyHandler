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
            {
                arguments = new string[] { "checkout", "a" };
                currentDirectory = Directory.GetParent(currentDirectory).Parent.Parent.Parent.Parent.FullName + "\\test";
            }
#endif
            XmlObjectParser objectParser = XmlObjectParser.Default;
            objectParser.RegisterClass("Project", new Project());
            objectParser.RegisterClass("GitRepo", new GitSyncher());
            objectParser.RegisterClass("GitVersion", new RepoVersionProvider());
            objectParser.RegisterClass("Url", new FileLocation());
            objectParser.RegisterClass("DependencyList", new ParseableList<ProjectDescriptor>());
            objectParser.RegisterClass("Git", new GitSyncher());
            objectParser.RegisterClass("String", new ConstantValue_Provider<string>(null));

            ProjectDatabase database = new ProjectDatabase(objectParser);
            ArgumentParser argumentParser = new ArgumentParser(new DependencyHandler(objectParser, database));
            argumentParser.ProcessArguments(currentDirectory, arguments);
            Logger.Message("done");
        }
    }
}
