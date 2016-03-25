## ReadableExpressions
AgileObjects.ReadableExpressions is a Portable Class Library written in C# exposing an extension method for the 
[Expression](https://msdn.microsoft.com/en-us/library/system.linq.expressions.expression.aspx) class which converts 
Expressions or entire [Expression Trees](https://msdn.microsoft.com/en-us/library/bb397951.aspx) into a readable string format.

### Usage
The extension method (in the namespace `AgileObjects.ReadableExpressions`) is used like so:

    string readable = myExpression.ToReadableString();

### Debugger Visualizers
An installer for a set of Debugger Visualizers which use the extension method for Expressions can be downloaded from 
[the Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/2d5de770-50e9-4dcf-87e9-ea1ed1b43b68).

### Download
You can download and install using [the NuGet package](https://www.nuget.org/packages/AgileObjects.ReadableExpressions), or 
clone the repository [on GitHub](https://github.com/AgileObjects/ReadableExpressions).