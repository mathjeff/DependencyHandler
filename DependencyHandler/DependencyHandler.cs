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
        public DependencyHandler(XmlObjectParser objectParser, ProjectDatabase projectDatabase)
        {
            this.objectParser = objectParser;
            this.projectDatabase = projectDatabase;
        }
        public void PrepareCommit(string directory)
        {
            Logger.Message("Preparing commit in directory " + directory);
            throw new NotImplementedException();
        }

        public void Checkout(string projectFilePath, String versionString)
        {
            String directory = Directory.GetParent(projectFilePath).FullName;
            FileLocation destination = new FileLocation(directory);
            Project project = this.objectParser.OpenProject(projectFilePath);

            Version version = new Version(versionString);
            Logger.Message("checking out version " + version + " in directory " + directory);
            project.FetchAll(version, this.projectDatabase);
        }

        public void ShowStatus(string projectFilePath)
        {
            String directory = Directory.GetParent(projectFilePath).FullName;
            FileLocation destination = new FileLocation(directory);
            Project project = this.objectParser.OpenProject(projectFilePath);
            Dictionary<Project, string> statuses = project.CheckStatus(this.projectDatabase);
            if (statuses.Count > 0)
            {
                foreach (Project childProject in statuses.Keys)
                {
                    Logger.Message(childProject);
                    Logger.Message(statuses[childProject]);
                }
            }
            else
            {
                Logger.Message("No changes (working directory clean)");
            }
        }

        public void UpdateDependencies(string projectFilePath)
        {
            // update the dependencies in memory
            Project mainProject = this.objectParser.OpenProject(projectFilePath);
            bool updated = false;
            IEnumerable<Project> projects = mainProject.GetCachedVersionOfDependencies(this.projectDatabase);
            foreach (Project project in projects)
            {
                foreach (ProjectDescriptor dependency in project.dependencies)
                {
                    Project dependencyProject = this.projectDatabase.TryGetDownloadedProject(dependency);

                    if (dependencyProject != null)
                    {
                        Version newVersion = dependencyProject.GetVersion();
                        Version oldVersion = new Version(dependency.version.GetValue());
                        if (!oldVersion.Equals(newVersion))
                        {
                            Logger.Message("Updating " + dependency.name.GetValue() + " dependency version from " + oldVersion + " to " + newVersion);
                            updated = true;
                            dependency.version.SetValue(newVersion.ToString());
                        }
                    }
                }
                if (updated)
                {
                    // save the dependencies back to disk
                    ProjectLoader loader = new ProjectLoader(project.source.GetValue().location.path);
                    loader.SetValue(project);
                }

            }

        }

        public void TestParse(string projectFilePath)
        {
            String directory = Directory.GetParent(projectFilePath).FullName;
            Project project = this.objectParser.OpenProject(projectFilePath);
            if (project == null)
            {
                throw new Exception("Parsed project was null");
            }

        }
        private XmlObjectParser objectParser;
        private ProjectDatabase projectDatabase;
    }
}
