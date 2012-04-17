# Compilify.net

Compilify makes the .NET compiler completely portable and accessible through a fast, simple interface that fosters sharing and collaboration.

Based on a [blog post](https://github.com/fekberg/Roslyn-Hosted-Execution) by Filip Ekberg.

## Running Compilify locally

Debugging this project requires that [MongoDB](http://www.mongodb.org/display/DOCS/Home) and [Redis](http://redis.io/download) be running locally.

Nuget packages will be missing the first time the solution is opened. They will be retrieved from Nuget the first time you attempt to build. 

The solution contains the web application and the background worker. Visual Studio can be configured to start both simultaneously while debugging.

In Visual Studio 2010:

1.  Right-click the Solution node in the Solution Explorer window.
2.  Select "Set StartUp Projects".
3.  Choose "Multiple startup projects".
4.  Set the "Action" for "Web" and "Worker" to "Start".

## Contributing

Please keep the master branch deployable at all times. It's recommended that all but the most minor changes be implemented in their own branch and merged back into master when complete.

### Coding Guidelines
* Use spaces, not tabs
* Follow existing conventions
* To make it easier to look at your code through the Github interface, wrap at column 80 when possible. Try not to go past 120.
