As well as `Expression.ToReadableString()`, the **ReadableExpressions** NuGet package provides the
following extension methods in the `AgileObjects.ReadableExpressions` namespace:

**Type**

To retrieve the definition of a `Type`, use:

```cs
string readable = myType.ToReadableString();
// Returns, e.g.
// public sealed class MyType
```

**ConstructorInfo**

To retrieve the definition of a `ConstructorInfo`, use:

```cs
string readable = myConstructorInfo.ToReadableString();
// Returns, e.g.
// protected MyClass
// (
//     int value,
//     TimeSpan value2
// )
```

**FieldInfo**

To retrieve the definition of a `FieldInfo`, use:

```cs
string readable = myFieldInfo.ToReadableString();
// Returns, e.g.
// private readonly string _myField;
```

**PropertyInfo**

To retrieve the definition of a `PropertyInfo`, use:

```cs
string readable = myPropertyInfo.ToReadableString();
// Returns, e.g.
// internal string MyProperty { get; private set; }
```

**MethodInfo**

To retrieve the definition of a `MethodInfo`, use:

```cs
string readable = myMethodInfo.ToReadableString();
// Returns, e.g.
// public Task<string> MyStreamMethod<TStream>
// (
//     TStream stream
// )
//     where TStream : Stream
```

### Type Names

To retrieve a readable Type name, use:

```cs
using AgileObjects.ReadableExpressions.Extensions

var type = typeof(Dictionary<int, List<string>>);

var readable = type.GetFriendlyName();
// Returns Dictionary<int, List<string>>, as opposed to:

var typeName = type.Name;
// Returns Dictionary`2

var typeFullName = type.FullName;
// Returns, e.g.:
// System.Collections.Generic.Dictionary`2[
//   [System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],
//   [System.Collections.Generic.List`1[
//     [System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]
//   ], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
```
