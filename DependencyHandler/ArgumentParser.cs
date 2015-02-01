using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class ArgumentParser
    {
        public ArgumentParser(DependencyHandler dependencyHandler)
        {
            this.dependencyHandler = dependencyHandler;
        }

        public String getHelp()
        {
            return "Try 'AutoDepVersioner checkout' or 'AutoDepVersioner on-commit'";
        }

        public void ProcessArguments(string currentDirectory, string[] arguments)
        {
            IEnumerator<string> i = arguments.AsEnumerable().GetEnumerator();
            // note that we're skipping the first argument
            if (!i.MoveNext()) {
                Logger.Message("No arguments provided");
                Logger.Message(this.getHelp());
                return;
            }
            while (true)
            {
                switch (i.Current)
                {
                    case "checkout":
                        String version = null;
                        try
                        {
                            i.MoveNext();
                            version = i.Current;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        this.dependencyHandler.Checkout(currentDirectory, version);
                        break;
                    case "on-commit":
                        this.dependencyHandler.PrepareCommit(currentDirectory);
                        break;
                    case "status":
                        this.dependencyHandler.ShowStatus(currentDirectory);
                        break;
                    default:
                        Logger.Message("Unrecognized argument: '" + i.Current + "'");
                        return;
                }
                try
                {
                    if (!i.MoveNext())
                        break;
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }

        }

        private DependencyHandler dependencyHandler;
    }
}
