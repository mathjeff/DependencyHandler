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
        public static void Message(object item)
        {
            Logger.Message(item.ToString());
        }
        public static void IncrementScope(int depth)
        {
            Logger.Default.AdjustScope(depth);
        }
        public static void DecrementScope(int depth)
        {
            Logger.Default.AdjustScope(-depth);
        }


        private void Write(string text)
        {
            if (this.currentDepth <= this.maxDepthToWrite)
            {
                Console.WriteLine(text);
                System.Diagnostics.Debug.WriteLine(text);
            }
        }
        public void AdjustScope(int extraDepth)
        {
            this.currentDepth += extraDepth;
        }

        private int currentDepth;
        private int maxDepthToWrite = 0;

    }
}
