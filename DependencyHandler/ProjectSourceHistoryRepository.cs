using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    interface ProjectSourceHistoryRepository
    {
        ValueProvider<string> name { get; }
        FileLocation location { get; set;  }
        Project project { get; set; }

        void Checkout(Version version); // `git checkout version`
        Version GetVersion();

        string CheckStatus();

        DTO<ProjectSourceHistoryRepository> ToDTO();

    }

    interface ProjectSourceHistoryRepositoryDTO : DTO<ProjectSourceHistoryRepository>
    {
    }

    interface RepoSyncher
    {
        ProjectSourceHistoryRepository localRepo { get; set; }
        ProjectSourceHistoryRepository remoteRepo { get; set; }
        void pull(FileLocation location, Version version);
        void push(FileLocation location, Version version);
        ValueProvider<String> name { get; set; }
        DTO<RepoSyncher> ToDTO();
    }


    interface RepoVersionProvider : ValueConverter<Project, Version>
    {
        DTO<RepoVersionProvider> ToDTO();
    }

}
