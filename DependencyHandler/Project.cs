using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class Project
    {
        public Project()
        {
            this.dependencyCacheLocationRoot = new StringJoiner(new ProjectPathProvider(this), new ConstantValue_Provider<string>("\\deps"));
            this.dependencies = new ParseableList<ProjectDescriptor>();
        }

        public ValueConverter<Project, Version> version { set { this._version = value; } }
        public Version GetVersion()
        {
            return this._version.ConvertValue(this);
        }
        public ValueProvider<string> name { get; set; }
        public XmlObjectParser parser { get; set; }
        public ValueProvider<ProjectSourceHistoryRepository> source
        {
            get
            {
                return this._source;
            }
            set
            {
                this._source = value;
            }
        }
        public ParseableList<ProjectDescriptor> dependencies { get; set; }
        public ValueProvider<FileLocation> location
        {
            get
            {
                return new ConstantValue_Provider<FileLocation>(this.source.GetValue().location);
            }
        }
        public ValueProvider<string> dependencyCacheLocationRoot { get; set; }

        public void FetchAll(Version version, ProjectDatabase projectDatabase)
        {
            // Download the right version of the source code for this project
            this.source.GetValue().Checkout(version);
            // Update (actually implemented as by loading a new project) this project based on the code that we just downloaded
            Project project = this.parser.OpenProject(this.location.GetValue().path);
            project.dependencyCacheLocationRoot = this.dependencyCacheLocationRoot;
            projectDatabase.PutProject(project.location.GetValue(), project);
            project.FetchDependencies(version, projectDatabase);
        }
        public void FetchDependencies(Version version, ProjectDatabase projectDatabase)
        {
            // Download the dependency projects
            foreach (ProjectDescriptor dependency in this.dependencies)
            {
                // tell the dependency where to put itself
                string cacheRoot = this.dependencyCacheLocationRoot.GetValue();
                string path = cacheRoot + "\\" + dependency.name.GetValue();
                FileLocation childDestination = new FileLocation(path);
                dependency.cacheLocation = childDestination;

                // now fetch the dependency finally
                Project childProject = projectDatabase.GetProject(dependency);
                // tell the child to put any of its dependencies in the same directory as ours
                childProject.dependencyCacheLocationRoot = this.dependencyCacheLocationRoot;
                childProject.FetchAll(new Version(dependency.version.GetValue()), projectDatabase);
            }
        }

        private ValueConverter<Project, Version> _version;
        private ValueProvider<ProjectSourceHistoryRepository> _source;
    }

    class ProjectPathProvider : ValueProvider<string>
    {
        public ProjectPathProvider(Project project)
        {
            this.project = project;
        }
        public string GetValue()
        {
            return this.project.location.GetValue().path;
        }
        public void SetValue(string newValue)
        {
            project.location.SetValue(new FileLocation(newValue));
        }
        private Project project;
    }

    // Uses the source-control version as the project's version
    class RepoVersionProvider : ValueConverter<Project, Version>
    {
        public Version ConvertValue(Project project)
        {
            return project.source.GetValue().GetVersion();
        }
    }

}
