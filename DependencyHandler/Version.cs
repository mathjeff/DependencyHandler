using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class Version
    {
        public Version(string content)
        {
            this.content = content;
        }
        public string content { get; set; }

        public override string ToString()
        {
            return this.content;
        }

        public override bool Equals(object obj)
        {
            Version other = obj as Version;
            if (other == null)
                return false;
            return (this.ToString() == other.ToString());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    interface VersionDTO : ValueProvider<RepoVersionProvider>
    {

    }


}
