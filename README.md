# Compilify.net

Compilify makes the .NET compiler completely portable and accessible through a fast, simple interface that fosters sharing and collaboration.

Based on a [blog post](https://github.com/fekberg/Roslyn-Hosted-Execution) by Filip Ekberg.

## For Future Visitors

Thanks to everyone who took interest in this repository. The code here helped introduce me to problems that I had never experienced before. I learned a lot from this project. I met a lot of amazing people because of this project. And in so many ways I became a better me because of this project.

If you have an idea: Build it. Publicize it early and frequently. Learn from it as fast and as frequently as you can.

You can't be wrong, you can only be better.

## Running Compilify locally

Debugging this project requires that [MongoDB](http://www.mongodb.org/display/DOCS/Home) and [RabbitMQ](http://www.rabbitmq.com/) be running locally.

Nuget packages will be missing the first time the solution is opened. They will be retrieved from Nuget the first time you attempt to build. 

The solution contains the web application and the background worker. Visual Studio can be configured to start both simultaneously while debugging.

In Visual Studio:

1.  Right-click the Solution node in the Solution Explorer window.
2.  Select "Set StartUp Projects".
3.  Choose "Multiple startup projects".
4.  Set the "Action" for "Web" and "Worker" to "Start".
