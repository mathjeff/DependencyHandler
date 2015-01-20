using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class GitRepo : ProjectSourceHistoryRepository
    {
        public ValueProvider<string> name { get; set; }
        public ValueProvider<string> version { get; set; }
        public ValueProvider<FileLocation> location { get; set; }
        public ValueProvider<Project> project { get; set; }
        public ProjectSourceHistoryRepository CopyTo(FileLocation destination, Version version)
        {
            throw new NotImplementedException();
        }
        public void Checkout(Version version)
        {
            if (version == null)
            {
                throw new ArgumentException("Version to checkout must be specified");
            }
            string command = "git checkout " + version.ToString();
            ShellUtils.RunCommand(command, this.location.GetValue().path.GetValue());
        }
    
    }

    /* class GitRepoToProjectConverter : ValueProvider<Project>
    {
        public GitRepoToProjectConverter()
        {
            this.initialize(new GitRepo());
        }

        private void initialize(GitRepo repo)
        {
            this.gitRepo = repo;
            this.name = gitRepo.name;
            this.version = gitRepo.version;
            this.location = gitRepo.location;
        }

        public ValueProvider<string> name
        {
            get
            {
                return this.gitRepo.name; 
            }
            set 
            {
                this.gitRepo.name = value; 
            }
        }
        public ValueProvider<string> version
        {
            get
            {
                return this.gitRepo.version;
            }
            set
            {
                this.gitRepo.version = value;
            }
        }
        public ValueProvider<FileLocation> location 
        {
            get
            {
                return this.gitRepo.location;
            }
            set
            {
                this.gitRepo.location = value;
            }
        }


        public Project GetValue()
        {
            return this.gitRepo.project.GetValue();
        }
        public void SetValue(Project project)
        {
            this.gitRepo.project.SetValue(project);
        }

        private GitRepo gitRepo;

    }*/



    class GitSyncher : RepoSyncher, ValueProvider<ProjectSourceHistoryRepository>
    {
        public GitSyncher()
        {
            this.localGitRepo = new GitRepo();
            this.remoteGitRepo = new GitRepo();
            this.name = null;
        }
        
        public ProjectSourceHistoryRepository localRepo
        {
            get
            { 
                return this.localGitRepo;
            }
            set 
            {
                this.localGitRepo = (GitRepo)value;
            }
        }
        GitRepo localGitRepo { get; set; }

        public ProjectSourceHistoryRepository remoteRepo
        {
            get
            {
                return this.remoteGitRepo;
            }
            set 
            {
                this.remoteGitRepo = (GitRepo)value;
            }
        }
        public ProjectSourceHistoryRepository GetValue()
        {
            return this.localGitRepo;
        }
        public void SetValue(ProjectSourceHistoryRepository repo)
        {
            this.localRepo = repo;
        }

        GitRepo remoteGitRepo { get; set; }

        public FileLocation remoteLocation
        {
            get
            {
                return this.remoteRepo.location.GetValue();
            }
            set
            {
                this.remoteRepo.location = new ConstantValue_Provider<FileLocation>(value);
            }
        }


        public void pull(Version version)
        {
            string localPath = this.localGitRepo.location.GetValue().path.GetValue();
            FileLocation remoteUrl = this.remoteGitRepo.location.GetValue();
            string remotePath = remoteUrl.server.GetValue() + "/" + remoteUrl.path.GetValue();
            string command;
            if (Directory.Exists(localPath))
            {
                command = "git pull";
            }
            else
            {
                command = "git clone " + remotePath;
            }
            ShellUtils.RunCommand(command, localPath);
            if (version != null)
                this.localGitRepo.Checkout(version);
            this.localGitRepo.project = new ProjectLoader(this.localGitRepo.location.GetValue().path.GetValue() + "\\project.xml");
        }

        public void push(Version version)
        {
            throw new NotImplementedException("pushing to a git repo isn't implemented yet");
        }

        public ValueProvider<String> name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                this.localGitRepo.name = value;
                this.remoteGitRepo.name = value;
            }
        }


        private ValueProvider<String> _name;
    }
}
