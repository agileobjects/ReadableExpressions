# ReadableExpressions

[![NuGet version](https://badge.fury.io/nu/AgileObjects.ReadableExpressions.svg)](https://badge.fury.io/nu/AgileObjects.ReadableExpressions)

ReadableExpressions is set of [Debugger Visualizers](https://marketplace.visualstudio.com/items?itemName=vs-publisher-1232914.ReadableExpressionsVisualizers)
and [a NuGet-packaged](https://www.nuget.org/packages/AgileObjects.ReadableExpressions) extension method for 
readable, source-code versions of [Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees). 

## Debugger Visualizers

An installer for the Expression Debugger Visualizers can be downloaded from 
[the Visual Studio Gallery](https://marketplace.visualstudio.com/items?itemName=vs-publisher-1232914.ReadableExpressionsVisualizers).

The visualizer has both Light and Dark themes:

[![Visualizer themes](/docs/Themes.gif)]

...and output can be customised using various options:

[![Visualizer options](/docs/Options.gif)]

## ASP.NET Core 5 Known Issue

.NET 5 has [a breaking change](https://github.com/dotnet/runtime/issues/29976), which disables `BinaryFormatter` serialization by default.
This causes issues with the ReadableExpressions visualizers (and [elsewhere](https://github.com/nhibernate/nhibernate-core/issues/2603)) 
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

## NuGet Package

The extension method is available in [a NuGet package](https://www.nuget.org/packages/AgileObjects.ReadableExpressions) 
targeting .NET 3.5+ and [.NETStandard 1.0](https://dotnet.microsoft.com/platform/dotnet-standard)+:

```shell
PM> Install-Package AgileObjects.ReadableExpressions
```

...and is used like so:

```csharp
using AgileObjects.ReadableExpressions;

string readable = myExpression.ToReadableString();
```

...it also works on [DynamicLanguageRuntime](https://www.nuget.org/packages/DynamicLanguageRuntime) expressions.

## Options

To include namespaces when outputting type names, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.UseFullyQualifiedTypeNames);
```

To use full type names instead of `var` for local and inline-declared output parameter variables, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.UseExplicitTypeNames);
```

To declare output parameter variables inline with the method where they are first used, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.DeclareOutputParametersInline);
```

To maintain explicit generic arguments on method calls where they are implied, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.UseExplicitGenericParameters);
```

To show implicitly-typed array type names, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.ShowImplicitArrayTypes);
```

To show lambda parameter type names, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.ShowLambdaParameterTypes);
```

To output a source code comment when a lambda is '[quoted](https://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do)', use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.ShowQuotedLambdaComments);
```

To define a custom factory for naming anonymous types, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.NameAnonymousTypesUsing(
        anonType => GetAnonTypeName(anonType)));
```

To define a custom factory for translating `ConstantExpression` values, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.TranslateConstantsUsing(
        (constantType, constantValue) => GetConstantValue(constantType, constantValue)));
```

To specify a custom string for code indenting, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.IndentUsing("\t"));
```
