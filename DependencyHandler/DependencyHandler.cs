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

        public void ListProjects(string projectFilePath)
        {
            Project mainProject = this.objectParser.OpenProject(projectFilePath);
            IEnumerable<Project> projects = this.GetAllDependenciesInOrder(mainProject);
            Logger.Message(projects.Count() + " Projects:");
            foreach (Project project in projects)
            {
                Logger.Message(project.location.GetValue().path);
            }
        }

        public void UpdateDependencies(string projectFilePath, string commitMessage)
        {
            // update the dependencies in memory
            Project mainProject = this.objectParser.OpenProject(projectFilePath);
            List<Project> projectOrder = this.GetAllDependenciesInOrder(mainProject);
            projectOrder.Reverse();
            foreach (Project project in projectOrder)
            {
                this.UpdateProjectDependenciesNonRecursive(project);
                if (commitMessage != null)
                {
                    ProjectSourceHistoryRepository repo = project.source.GetValue();
                    if (repo.HasUncommittedChanges())
                    {
                        repo.Commit(commitMessage);
                    }
                    else
                    {
                        if (project == mainProject)
                        {
                            throw new InvalidOperationException("No changes to commit for project " + project);
                        }
                    }
                }
            }
        }

        public void UpdateProjectDependenciesNonRecursive(Project project)
        {
            bool updated = false;
            foreach (ProjectDescriptor dependency in project.dependencies)
            {
                Project dependencyProject = this.projectDatabase.TryGetDownloadedProject(dependency);

                if (dependencyProject != null)
                {
                    Version newVersion = dependencyProject.GetVersion();
                    Version oldVersion = new Version(dependency.version.GetValue());
                    if (!oldVersion.Equals(newVersion))
                    {
                        Logger.Message("Updating " + dependency.name.GetValue() + " dependency version in " + project.name.GetValue() + " from " + oldVersion + " to " + newVersion);
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

        public void Commit(string projectFilePath, string message)
        {
            if (message == null)
            {
                throw new ArgumentException("Commit message cannot be null");
            }
            this.UpdateDependencies(projectFilePath, message);
        }

        public List<Project> GetAllDependenciesInOrder(Project rootProject)
        {
            // returns a list of self and all projects that this project depends on
            LinkedList<Project> pendingProjects = new LinkedList<Project>();
            pendingProjects.AddLast(rootProject);
            List<Project> results = new List<Project>();
            while (pendingProjects.Count > 0)
            {
                Project project = pendingProjects.Last();
                pendingProjects.RemoveLast();
                IEnumerable<Project> dependencies = project.GetCachedVersionOfDirectDependencies(this.projectDatabase);
                LinkedList<Project> newDependencies = new LinkedList<Project>();
                foreach (Project dependency in dependencies)
                {
                    if (dependency != null)
                    {
                        if (!results.Contains(dependency))
                        {
                            if (!pendingProjects.Contains(dependency))
                                newDependencies.AddLast(dependency);
                        }
                    }
                }
                if (newDependencies.Count == 0)
                {
                    results.Add(project);
                }
                else
                {
                    pendingProjects.AddLast(project);
                    foreach (Project dependency in newDependencies)
                    {
                        pendingProjects.AddLast(dependency);
                    }
                }
            }
            return results;

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
