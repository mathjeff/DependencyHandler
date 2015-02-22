using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class ProjectLoader : ValueProvider<Project>
    {
        public ProjectLoader(string filePath)
        {
            this.filePath = filePath;
        }

        public Project GetValue()
        {
            return XmlObjectParser.Default.OpenProject(this.filePath);
        }

        public void SetValue(Project project)
        {
            XmlObjectParser.Default.SaveProject(this.filePath, project);
        }

        string filePath;
    }
}
