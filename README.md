DependencyHandler
By Jeff Gaston

The DependencyHandler was created to resolve code dependencies and to support keeping the versions of those dependencies in sync easily. It's a similar idea to Apache Maven but without the ambiguity of SNAPSHOT dependencies and without the waiting for a tag to be created.

This system is designed to be able to download dependencies from lots of different external systems (code from git/svn, built artifacts from nexus, etc) but currently the only system that is supported is git.

See https://github.com/mathjeff/ActivityRecommender-WPhone for an example project