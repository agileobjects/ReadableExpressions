The following options are available when translating an Expression.

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
```

To discard unused output parameter variables or lambda parameters, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.DiscardUnusedParameters);
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

To output a source code comment when a lambda is 
'[quoted](https://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do)', use:

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

To include the values of captured variable, field and property values, use:

```csharp
string readable = myExpression
    .ToReadableString(c => c.ShowCapturedValues);
```
