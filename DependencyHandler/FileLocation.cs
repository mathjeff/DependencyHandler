using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class FileLocation
    {
        public FileLocation()
        {
        }

        public FileLocation(String filePath)
        {
            this.path = new ConstantValue_Provider<string>(filePath);
        }

        public ConstantValue_Provider<String> server { get; set; }
        public ConstantValue_Provider<String> path { get; set; }



        #region ConstantValue_Provider<FileLocation>
        
        public FileLocation GetValue()
        {
            return this;
        }

        public void SetValue(FileLocation newValue)
        {
            this.server = newValue.server;
            this.path = newValue.path;

        }

        #endregion

    }
}
