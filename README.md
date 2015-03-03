DependencyHandler

Downloads dependencies and knows how to update version numbers. This makes it easy to specify which version of each project depends on which versions of which other items.


This system is designed to be able to download dependencies from lots of different external systems (code from git/svn, built artifacts from nexus, etc) but currently the only system that is supported is git.

See https://github.com/mathjeff/ActivityRecommender-WPhone for an example project