//#define DEBUGGING

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
#if DEBUGGING
            currentDirectory = "c:\\Users/Jeff/Documents/Visual Studio 2012/Projects/Git/ActivityRecommender-WPhone";
            if (arguments.Count() == 0)
            {
                arguments = new string[] { "updeps" };
                //currentDirectory = Directory.GetParent(currentDirectory).Parent.Parent.Parent.Parent.FullName + "\\test";
            }
#endif
            XmlObjectParser objectParser = XmlObjectParser.Default;
            objectParser.RegisterClass("Project", new ProjectDTO());
            objectParser.RegisterClass("GitRepo", new GitRepoDTO());
            objectParser.RegisterClass("GitVersion", new GitRepoVersionProvider());
            objectParser.RegisterClass("Url", new FileLocation());
            objectParser.RegisterClass("DependencyList", new ParseableList<ProjectDescriptorDTO>());
            objectParser.RegisterClass("Git", new GitSyncherDTO());
            objectParser.RegisterClass("String", new ConstantValue_Provider<string>(null));

            ProjectDatabase database = new ProjectDatabase(objectParser);
            ArgumentParser argumentParser = new ArgumentParser(new DependencyHandler(objectParser, database));
            argumentParser.ProcessArguments(currentDirectory, arguments);
        }
    }
}
