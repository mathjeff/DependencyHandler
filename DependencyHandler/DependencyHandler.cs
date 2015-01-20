using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class DependencyHandler
    {
        public DependencyHandler(XmlObjectParser objectParser)
        {
            this.objectParser = objectParser;
        }
        public void PrepareCommit(string directory)
        {
            Logger.Message("Preparing commit in directory " + directory);
            throw new NotImplementedException();
        }

        public void Checkout(string projectFilePath, String versionString)
        {
            String directory = Directory.GetParent(projectFilePath).FullName;
            FileLocation destination = new FileLocation();
            destination.path = new ConstantValue_Provider<string>(directory);
            Logger.Message("parsing project at " + projectFilePath);
            Project project = this.objectParser.OpenProject(projectFilePath);

            Version version = new Version(versionString);
            Logger.Message("checking out version " + version + " in directory " + directory);
            project.FetchAll(version);
        }

        public void TestParse(string projectFilePath)
        {
            String directory = Directory.GetParent(projectFilePath).FullName;
            Logger.Message("parsing project at " + projectFilePath);
            Project project = this.objectParser.OpenProject(projectFilePath);
            if (project == null)
            {
                throw new Exception("Parsed project was null");
            }

        }
        private XmlObjectParser objectParser;
    }
}
