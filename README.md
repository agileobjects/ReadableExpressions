## ReadableExpressions

[![NuGet](http://img.shields.io/nuget/v/AgileObjects.ReadableExpressions.svg)](https://www.nuget.org/packages/AgileObjects.ReadableExpressions)

ReadableExpressions is an extension method for the [Expression](https://msdn.microsoft.com/en-us/library/system.linq.expressions.expression.aspx) class and set of Debugger Visualizers to produce readable, source-code string versions of [Expression Trees](https://msdn.microsoft.com/en-us/library/bb397951.aspx). It targets [.NETStandard 1.0](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard) and .NET 4.0.

### Usage
The extension method (in the namespace `AgileObjects.ReadableExpressions`) is used like so:

    string readable = myExpression.ToReadableString();

You can optionally maintain explicit generic arguments on method calls where they are implied like so:

    string readable = myExpression.ToReadableString(c => c.UseExplicitGenericParameters);

### Debugger Visualizers
An installer for a set of Debugger Visualizers which use the extension method for Expressions can be downloaded from 
[the Visual Studio Gallery](https://marketplace.visualstudio.com/items?itemName=vs-publisher-1232914.ReadableExpressionsVisualizers).

### Download
You can download and install using [the NuGet package](https://www.nuget.org/packages/AgileObjects.ReadableExpressions), or 
clone the repository [on GitHub](https://github.com/AgileObjects/ReadableExpressions).
