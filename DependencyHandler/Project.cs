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
        }

        public ValueProvider<Version> version { get; set; }
        public ValueProvider<string> name { get; set; }
        public ValueProvider<Project> parser { get; set; }
        public ValueProvider<ProjectSourceHistoryRepository> source { get; set; }
        public ParseableList<ProjectDescriptor> dependencies { get; set; }
        public ValueProvider<FileLocation> location
        {
            get
            {
                return this.source.GetValue().location;
            }
        }

        public void FetchAll(Version version)
        {
            // Download this project
            //this.source = new ConstantValue_Provider<ProjectSourceHistoryRepository>(this.source.GetValue().CopyTo(this.location, version));
            this.source.GetValue().Checkout(version);

            // Download the dependency projects
            foreach (ProjectDescriptor dependency in this.dependencies)
            {
                // tell the dependency where to put itself
                FileLocation childDestination = new FileLocation();
                childDestination.server = location.GetValue().server;
                childDestination.path = new ConstantValue_Provider<string>(location.GetValue().path.GetValue() + "\\" + dependency.name.GetValue());
                dependency.cacheLocation.SetValue(childDestination);

                // have the dependency fetch itself
                Project childProject = dependency.GetValue();
                childProject.FetchAll(new Version(dependency.version.GetValue()));
            }
        }
    }
}
