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
        public GitRepo()
        {
            this.location = new ConstantValue_Provider<FileLocation>();
        }
        public ValueProvider<string> name { get; set; }
        public ValueProvider<Version> version
        {
            get
            {
                GitVersionProvider versionProvider = new GitVersionProvider();
                versionProvider.Repo = this;
                return versionProvider;
            }
        }
        public ValueProvider<FileLocation> location { get; set; }
        public Project project { get; set; }
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
            string command = "checkout " + version.ToString();
            ShellUtils.RunCommandAndGetOutput("git", command, this.location.GetValue().path.GetValue());
        }
        public Version GetVersion()
        {
            string versionText = ShellUtils.RunCommandAndGetOutput("git", "git log -n 1 --oneline --format=%H", this.location.GetValue().path.GetValue());
            return new Version(versionText);
        }
    
    }


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
                this.remoteRepo.location.SetValue(value);
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
                command = "pull --rebase";
            }
            else
            {
                command = "clone " + remotePath;
            }
            ShellUtils.RunCommandAndGetOutput("git", command, localPath);
            if (version != null)
                this.localGitRepo.Checkout(version);
            this.localGitRepo.project = new ProjectLoader(this.localGitRepo.location.GetValue().path.GetValue() + "\\project.xml").GetValue();
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

    class GitUtils
    {

    }

    // allows reading the git repository's version and checking out another version
    class GitVersionProvider : ValueConverter<GitRepo, Version>
    {
        public GitVersionProvider()
        {

        }

        public Version GetValue()
        {
            return this.repo.GetVersion();
        }

        public void SetValue(Version newValue)
        {
            this.repo.Checkout(newValue);
        }

        public void SetInput(GitRepo repo)
        {
            this.Repo = repo;
        }
        public GitRepo Repo
        {
            set
            {
                this.repo = value;
            }
        }


        GitRepo repo;
    }
}
