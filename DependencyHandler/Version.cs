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
    }
}
