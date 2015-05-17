# TalBot

I'm using this project as a way to experiment with and learn F#. I started with the FSharp [ProjectScaffold](http://fsprojects.github.io/ProjectScaffold/index.html).
It is currently functional with one exception. I'm having trouble loading F# assemblies that implement my IPlugin interface. I get an InvalidCastException when trying to downcast to the interface.
For some reason this isn't a problem with C# plugins. If anyone has a suggestion for a fix, I'd be interested.