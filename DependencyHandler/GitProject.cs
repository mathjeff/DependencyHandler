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
            Logger.IncrementScope(1);
            string versionText = ShellUtils.RunCommandAndGetOutput("git", "log -n 1 --oneline --format=%H", this.location.path).Trim();
            Logger.DecrementScope(1);
            return new Version(versionText);
        }
        public string CheckStatus()
        {
            Logger.IncrementScope(1);
            string version = ShellUtils.RunCommandAndGetOutput("git", "status --porcelain", this.location.path).Trim();
            Logger.DecrementScope(1);
            if (version.Length == 0)
                return null;
            return version;
        }
        public bool HasUncommittedChanges()
        {
            return (this.CheckStatus() != null);
        }
        public void Commit(String message)
        {
            if (message == null)
            {
                throw new ArgumentException("Commit message");
            }
            this.validateLocation(this.location);
            string command = "add -u";
            ShellUtils.RunCommandAndGetOutput("git", command, this.location.path);
            command = "commit -m \"" + message + "\"";
            ShellUtils.RunCommandAndGetOutput("git", command, this.location.path, false);
        }
        public DTO<ProjectSourceHistoryRepository> ToDTO()
        {
            return new GitRepoDTO();
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

        public DTO<RepoSyncher> ToDTO()
        {
            GitSyncherDTO dto = new GitSyncherDTO();
            dto.origin = this.origin;
            return dto;
        }


        private ValueProvider<String> _name;
    }

    class GitSyncherDTO : DTO<RepoSyncher>
    {
        public FileLocation origin { get; set; }

        public RepoSyncher GetValue()
        {
            GitSyncher syncher = new GitSyncher();
            syncher.origin = this.origin;
            return syncher;
        }
        public void SetValue(RepoSyncher syncher)
        {
            throw new NotImplementedException();
        }
    }

    class GitRepoDTO : DTO<ProjectSourceHistoryRepository>
    {
        public GitRepoDTO()
        {
            
        }
        public ProjectSourceHistoryRepository GetValue()
        {
            return this.repo;
        }

        public void SetValue(ProjectSourceHistoryRepository repository)
        {
            // nothing to save
        }

        private GitRepo repo = new GitRepo();

    }

    // Uses the source-control version as the project's version
    class GitRepoVersionProvider : RepoVersionProvider, DTO<RepoVersionProvider>
    {
        public Version ConvertValue(Project project)
        {
            return project.source.GetValue().GetVersion();
        }

        public RepoVersionProvider GetValue()
        {
            return this;
        }

        public void SetValue(RepoVersionProvider provider)
        {
            throw new InvalidOperationException();
        }

        public DTO<RepoVersionProvider> ToDTO()
        {
            return this;
        }

    }


    class GitUtils
    {

    }

    
}
