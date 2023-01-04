## Custom Translators

Custom `Expression` translators can be configured on a per-`ExpressionType` basis.

For example, to configure custom translation of `ConstantExpression`s, use:

```cs
string readable = myExpression
    .ToReadableString(c => c
        .AddTranslatorFor(ExpressionType.Constant, (expr, _) =>
        {
            var constant = (ConstantExpression)expr;
            return GetMyTranslation(constant);
        }));
```

In this example the user-defined `GetMyTranslation` method takes a `ConstantExpression`
and returns a string representation to include in the translation.

The custom translator Func is passed the Expression to translate, and a 
`Func<Expression, string>` with which to retrieve the default translation for the given `Expression`
or a child Expression. For example, a custom `NewArrayExpression` translator could be:

```cs
string readable = myExpression
    .ToReadableString(c => c
        .AddTranslatorFor(ExpressionType.NewArrayInit, (expr, defaultTranslator) =>
        {
            var arrayInit = (NewArrayExpression)expr;

            if (UseDefaultTranslation(arrayInit))
            {
                return defaultTranslator(arrayInit);
            }

            var childExpressions = arrayInit
                .Expressions
                .Select(defaultTranslator);

            return $"{{ {string.Join(", ", childExpressions)} }}";
        }));
```

In this example the user-defined `UseDefaultTranslation` method enables a fallback to the default
translation if desired, then embeds the default translations for the array's element initializations
in curly braces, to produce (*e.g*) the syntax `{ 123, "Hello!", 456 }`.

## Custom Expression Types

Custom `Expression` types can be created and made translatable.

For example, the following `Expression` could be used to represent the use of the
[Null-Forgiving](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving) 
operator:

```cs
public class NullForgivingOperatorExpression : Expression
{
    public NullForgivingOperatorExpression(
        Expression objectExpression)
    {
        // Represents use of the Null-Forgiving operator, 
        // e.g: 'myNullableValue!'

        // The Expression to which we are applying the 
        // operator, to declaring it is not null:
        ObjectExpression = objectExpression;
    }

    public override Type Type => ObjectExpression.Type;

    public override ExpressionType NodeType => (ExpressionType)12345;

    public Expression ObjectExpression { get; }

    public override bool CanReduce => true;

    public override Expression Reduce() => ObjectExpression;
    
    protected override Expression Accept(ExpressionVisitor visitor)
        => visitor.Visit(ObjectExpression);
}
```

By implementing the `ICustomTranslationExpression` interface, **ReadableExpressions** will be able
to include this custom `Expression` type in its translations. `ICustomTranslationExpression` has a
single `GetTranslation()` method, returning an `INodeTranslation` which writes out the translated 
`Expression`:

```cs
public class NullForgivingOperatorExpression : 
    Expression,
    ICustomTranslationExpression
{
    // ...

    ITranslation ICustomTranslationExpression.GetTranslation(
        ITranslationContext context)
    {
        return new NullForgivingOperatorTranslation(this, context);
    }
}

public class NullForgivingOperatorTranslation : INodeTranslation
{
    private INodeTranslation _objectTranslation;

    public NullForgivingOperatorTranslation(
        NullForgivingOperatorExpression operatorExpression,
        ITranslationContext context)
    {
        // Get an INodeTranslation for the expression to 
        // which the operator is being applied:
        _objectTranslation = context.GetTranslationFor(
            operatorExpression.ObjectExpression);
    }

    public ExpressionType NodeType => _objectTranslation.NodeType;

    public int TranslationLength => _objectTranslation.TranslationLength + 1;

    public void WriteTo(TranslationWriter writer)
    {
        // Write out the translation of the object:
        _objectTranslation.WriteTo(writer);

        // Write out the operator:
        writer.WriteToTranslation('!');
    }
}
```