# Overview

ReadableExpressions provides extension methods and a 
[Debugger Visualizer](https://marketplace.visualstudio.com/items?itemName=vs-publisher-1232914.ReadableExpressionsVisualizers) 
for readable, source-code versions of 
[Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees),
as well as reflection objects like `Type`, `FieldInfo`, `PropertyInfo`, etc.

## Debugger Visualizer

The Debugger Visualizer installer can be downloaded from 
[the Visual Studio Gallery](https://marketplace.visualstudio.com/items?itemName=vs-publisher-1232914.ReadableExpressionsVisualizers).

The visualizer has both Light and Dark themes:

[![Visualizer themes](/assets/images/Themes.gif)]

...and output can be customised using various options:

[![Visualizer options](/assets/images/Options.gif)]

## ASP.NET Core 5 Known Issue

.NET 5 had [a breaking change](https://github.com/dotnet/runtime/issues/29976), which disables `BinaryFormatter` serialization by default.
This has caused issues with the ReadableExpressions visualizers (and [elsewhere](https://github.com/nhibernate/nhibernate-core/issues/2603)) 
when debugging ASP.NET Core apps as the VS debugger [uses](https://wrightfully.com/writing-a-readonly-debugger-visualizer) `BinaryFormatter` 
to serialize objects before sending them to the visualizer.

[The solution](https://developercommunity2.visualstudio.com/t/visual-studio-debugger-visualizers-and-binaryforma/1278642) is to enable the 
`BinaryFormatter` in Debug only by adding the following to your ASP.NET Core csproj:

```xml
<PropertyGroup>
  <TargetFramework>net5.0</TargetFramework>
  <EnableUnsafeBinaryFormatterSerialization Condition=" '$(Configuration)' == 'Debug' ">
    true
  </EnableUnsafeBinaryFormatterSerialization>
</PropertyGroup>
```

## Extension Methods

The extension methods are available [on NuGet](https://www.nuget.org/packages/AgileObjects.ReadableExpressions), 
targeting .NET 3.5+ and [.NETStandard 1.0](https://dotnet.microsoft.com/platform/dotnet-standard)+:

```shell
PM> Install-Package AgileObjects.ReadableExpressions
```
[![NuGet version](https://badge.fury.io/nu/AgileObjects.ReadableExpressions.svg)](https://badge.fury.io/nu/AgileObjects.ReadableExpressions)

...and are used like so:

```csharp
using AgileObjects.ReadableExpressions;

string readable = myExpression.ToReadableString();
```

...it also works on [DynamicLanguageRuntime](https://www.nuget.org/packages/DynamicLanguageRuntime) expressions.