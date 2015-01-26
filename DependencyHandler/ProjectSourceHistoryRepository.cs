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
        ValueProvider<Version> version { get; } // currently checked-out version
        ValueProvider<FileLocation> location { get; }
        Project project { get; set; }

        void Checkout(Version version); // `git checkout version`

    }

    interface RepoSyncher
    {
        ProjectSourceHistoryRepository localRepo { get; set; }
        ProjectSourceHistoryRepository remoteRepo { get; set; }
        void pull(Version version);
        void push(Version version);
        ValueProvider<String> name { get; set; }
    }
}
