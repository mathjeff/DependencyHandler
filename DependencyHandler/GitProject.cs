using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//#define FAKE_NETWORK

namespace DependencyHandling
{
    class GitRepo : ProjectSourceHistoryRepository
    {
        public GitRepo()
        {
        }
        public ValueProvider<string> name { get; set; }
        public FileLocation location
        {
            get
            {
                return new FileLocation(this._location);
            }
            set
            {
                this.validateLocation(value);
                this._location = new FileLocation(value);
            }
        }
        private void validateLocation(FileLocation location)
        {
            if (location != null && location.server == null)
            {
                string gitDirectory = location.path + "\\.git";
                if (!Directory.Exists(gitDirectory))
                    throw new Exception("Not a git directory: '" + gitDirectory + "'");
            }
        }
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
            this.validateLocation(this.location);
            string command = "checkout " + version.ToString();
            ShellUtils.RunCommandAndGetOutput("git", command, this.location.path);
        }
        public Version GetVersion()
        {
            string versionText = ShellUtils.RunCommandAndGetOutput("git", "git log -n 1 --oneline --format=%H", this.location.path);
            return new Version(versionText);
        }
        private FileLocation _location;
    
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

        public FileLocation origin
        {
            get
            {
                return this.remoteRepo.location;
            }
            set
            {
                this.remoteRepo.location = value;
            }
        }


        public void pull(FileLocation localPath, Version version)
        {
            FileLocation remoteUrl = this.remoteGitRepo.location;
            string remotePath = remoteUrl.server + "/" + remoteUrl.path;
            string command;
            string gitDirectory = localPath.path + "\\.git";
            if (Directory.Exists(gitDirectory))
            {
                command = "pull --rebase";
#if FAKE_NETWORK
                ShellUtils.RunCommandAndGetOutput("echo", "git " + command, localPath);
#else
                ShellUtils.RunCommandAndGetOutput("git", command, localPath.path);
#endif
            }
            else
            {
                //Logger.Message("Git directory not found: " + gitDirectory);
                string parent = Directory.GetParent(localPath.path).FullName;
                Directory.CreateDirectory(parent);
                command = "clone " + remotePath + " " + (new DirectoryInfo(localPath.path)).Name;
#if FAKE_NETWORK
                ShellUtils.RunCommandAndGetOutput("echo", "git " + command, parent);
#else
                ShellUtils.RunCommandAndGetOutput("git", command, Directory.GetParent(localPath.path).FullName);
#endif
                this.localGitRepo.location = localPath;
            }
            if (version != null)
                this.localGitRepo.Checkout(version);
        }

        public void push(FileLocation localLocation, Version version)
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

    
}
