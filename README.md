When Microsoft released the Roslyn CTP it included the C# Interactive window to Visual Studio. Through this window, users can execute C# statements against their solution with little effort. This is extremely useful for doing things like playing with regexes, comparing the results of different method overloads, or quickly testing portions of of code, all without requiring the entire project or solution to be rebuilt.

The idea behind Compilify is to bring that same interactive environment to the web. My vision is for this site to turn into the go-to place for .NET developers to paste, share, and collaborate on their code.

Compilify is also a great site for people who are interested in learning C#. It provides a fast and simple way to experiment with basic constructs and techniques without requiring the user to download and install any additional software. This site lets you skip all of that overhead and just dive in!

Based on a blog post by Filip Ekberg (https://github.com/fekberg/Roslyn-Hosted-Execution).

Debugging this project requires a MongoDB and Redis server running on localhost.
