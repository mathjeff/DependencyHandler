using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// a ProjectDescriptor is enough of a description of a project to be used as a dependency
namespace DependencyHandling
{
    class ProjectDescriptor : ValueProvider<Project>
    {
        public ProjectDescriptor()
        {
        }

        public ValueProvider<string> name {
            get
            {
                return this._name;
            }
            set
            {
                // link the name of the repo syncher to the name of the project descriptor
                this._name = value;
                if (this.syncher != null)
                    this.syncher.name = value;
            }
        }
        public ValueProvider<string> version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }
        public ValueProvider<FileLocation> preferredCacheLocation
        {
            get
            {
                return this._preferredCacheLocation;
            }
            set
            {
                this._preferredCacheLocation = value;
                // link the preferred cache location to the local repo
                if (this._syncher != null)
                {
                    this.syncher.localRepo.location = value;
                }
            }
        }
        public RepoSyncher syncher
        {
            get
            {
                return this._syncher;
            }
            set
            {
                // link the name of the repo syncher to the name of the project descriptor
                RepoSyncher syncher = value;
                syncher.name = this._name;
                syncher.localRepo.location = this._preferredCacheLocation;
                this._syncher = syncher;
            }
        }

        public Project GetValue()
        {
            if (this.syncher.localRepo.project == null)
                this.syncher.pull(null);
            return this.syncher.localRepo.project.GetValue();
        }

        public void SetValue(Project project)
        {
            throw new NotImplementedException("Pushing via ProjectDescriptor.cs isn't supported");
        }

        private ValueProvider<string> _name;
        private ValueProvider<string> _version;
        private RepoSyncher _syncher;
        private ValueProvider<FileLocation> _preferredCacheLocation = new ConstantValue_Provider<FileLocation>(null);
    }
}
