## Overview

**BuildableExpressions** enables runtime [CLR type generation](/building-types/). It can build 
[classes](/api/Building-Classes), [structs](/api/Building-Structs), [interfaces](/api/Building-Interfaces),
[enums](/api/Building-Enums) and [attributes](/api/Building-Attributes).

To install from NuGet, use:

```shell
PM> Install-Package AgileObjects.BuildableExpressions
```
[![NuGet version](https://badge.fury.io/nu/AgileObjects.BuildableExpressions.svg)](https://badge.fury.io/nu/AgileObjects.BuildableExpressions)

**BuildableExpressions.Generator** enables build-time [C# source code generation](/generating-code/)
via a [configurable](/generating-code/Configuration) 
[MSBuild task](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-tasks). It works within
Visual Studio or from `dotnet build`, and supports 
[SDK](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview) and non-SDK projects.


To install from NuGet, use:

```shell
PM> Install-Package AgileObjects.BuildableExpressions.Generator
```
[![NuGet version](https://badge.fury.io/nu/AgileObjects.BuildableExpressions.Generator.svg)](https://badge.fury.io/nu/AgileObjects.BuildableExpressions.Generator)

Both packages generate from C# source-code strings or
[Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees), target .NET 4.6.1
and [.NETStandard 2.0](https://dotnet.microsoft.com/platform/dotnet-standard), and are available under the 
[MIT licence](https://github.com/agileobjects/BuildableExpressions/blob/master/LICENCE.md). 

