using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class Logger
    {
        public static Logger Default = new Logger();
        public static void Message(string text)
        {
            Logger.Default.Write(text);
        }


        private void Write(string text)
        {
            Console.WriteLine(text);
            System.Diagnostics.Debug.WriteLine(text);
        }

    }
}
