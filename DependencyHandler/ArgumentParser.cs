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
            return "usage: DependencyHandler <command> [arguments]\n"
                + "\n"
                + "Commands: \n"
                + "status      view what has changed in the current repository and its dependencies\n"
                + "checkout    check out a particular commit of the current repository, and the corresponding\n"
                + "            commits of its dependencies\n"
                + "updeps      update versions of all dependencies so the versions match the currently\n"
                + "            checked-out versions without making any commit\n"
                + "commit      [-m <message>]\n"
                + "            Update all versions of all dependencies and commit all modifications"
                ;
        }

        public void ProcessArguments(string currentDirectory, string[] arguments)
        {
            IEnumerator<string> i = arguments.AsEnumerable().GetEnumerator();
            // note that we're skipping the first argument
            if (!i.MoveNext()) {
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
                        return;
                    case "status":
                        this.dependencyHandler.ShowStatus(currentDirectory);
                        return;
                    case "updeps":
                        this.dependencyHandler.UpdateDependencies(currentDirectory, null);
                        return;
                    case "commit":
                        i.MoveNext();
                        this.Commit(currentDirectory, i);
                        return;
                    case "on-commit":
                        this.dependencyHandler.PrepareCommit(currentDirectory);
                        return;
                    case "projects":
                        this.dependencyHandler.ListProjects(currentDirectory);
                        return;
                    default:
                        Logger.Message("Unrecognized command: '" + i.Current + "'");
                        return;
                }
            }

        }

        public void Commit(string currentDirectory, IEnumerator<string> argumentIterator)
        {
            IEnumerator<string> i = argumentIterator;
            string message = null;
            
            while (true)
            {
                switch (i.Current)
                {
                    case "-m":
                        try
                        {
                            i.MoveNext();
                            message = i.Current;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        break;
                    default:
                        Logger.Message("Unrecognized argument '" + i.Current + "'");
                        return;
                }
                try
                {
                    if (!i.MoveNext())
                        break;
                }
                catch (InvalidOperationException)
                {
                }
            }
            if (message == null)
            {
                Logger.Message("Error: must provide commit message. Do 'h commit -m \"message\"'");
            }
            Logger.Message("Committing in all repos, using message '" + message + "'");
            this.dependencyHandler.Commit(currentDirectory, message);
        }

        private DependencyHandler dependencyHandler;
    }
}
