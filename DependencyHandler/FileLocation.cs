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
        public FileLocation(FileLocation original)
        {
            if (original != null)
            {
                this._server = original._server;
                this._path = original._path;
            }
        }
        public FileLocation(string filePath)
        {
            this._path = filePath;
            this._server = null;
        }

        public FileLocation(string filePath, string server)
        {
            this._path = filePath;
            this._server = server;
        }

        public string server
        {
            get
            {
                return this._server;
            }
            set
            {
                this._server = value;
            }
        }
        public string path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }

        private string _server;
        private string _path;


    }
}
