using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class ProjectDatabase
    {
        public ProjectDatabase(XmlObjectParser projectParser)
        {
            this.projectsByPath = new Dictionary<string, Project>();
            this.projectParser = projectParser;
        }
        public Project TryGetDownloadedProject(ProjectDescriptor dependency)
        {
            // check whether we've already downloaded the project
            FileLocation cacheLocation = dependency.cacheLocation;
            if (cacheLocation.server == null && this.projectsByPath.ContainsKey(cacheLocation.path))
                return this.projectsByPath[cacheLocation.path];
            string filePath = cacheLocation.path;
            // check whether the project exists
            if (this.projectParser.ProjectExists(filePath))
            {
                // parse the project
                Project project = this.projectParser.OpenProject(filePath);
                // save the project and return it
                this.projectsByPath[cacheLocation.path] = project;
                return project;
            }
            return null;
        }
        public Project GetProject(ProjectDescriptor dependency)
        {
            // check whether we've already downloaded the project
            Project cachedProject = this.TryGetDownloadedProject(dependency);
            if (cachedProject != null)
                return cachedProject;

            // download the project
            FileLocation cacheLocation = dependency.cacheLocation;
            string filePath = cacheLocation.path;
            dependency.syncher.pull(dependency.cacheLocation, new Version(dependency.version.GetValue()));
            // parse the project
            Project project = this.projectParser.OpenProject(filePath);
            // save the project and return it
            this.projectsByPath[cacheLocation.path] = project;
            return project;
        }
        public void PutProject(FileLocation location, Project project)
        {
            this.projectsByPath[location.path] = project;
        }

        private Dictionary<string, Project> projectsByPath;
        private XmlObjectParser projectParser;
    }
}
