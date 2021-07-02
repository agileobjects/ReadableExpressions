Custom Expression types can be created and made translatable.

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
single `GetTranslation()` method, returning an `ITranslation` which writes out the translated 
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

public class NullForgivingOperatorTranslation : ITranslation
{
    private ITranslation _objectTranslation;

    public NullForgivingOperatorTranslation(
        NullForgivingOperatorExpression operatorExpression,
        ITranslationContext context)
    {
        // Get an ITranslation for the expression to 
        // which the operator is being applied:
        _objectTranslation = context.GetTranslationFor(
            operatorExpression.ObjectExpression);
    }

    public ExpressionType NodeType
        => _objectTranslation.NodeType;

    public Type Type => _objectTranslation.Type;

    public int TranslationSize
        => _objectTranslation.TranslationSize + 1;

    public int FormattingSize
        => _objectTranslation.FormattingSize;

    public int GetIndentSize()
        => _objectTranslation.GetIndentSize();

    public int GetLineCount()
        => _objectTranslation.GetLineCount();

    public void WriteTo(TranslationWriter writer)
    {
        // Write out the translation of the object:
        _objectTranslation.WriteTo(writer);

        // Write out the operator:
        writer.WriteToTranslation('!');
    }
}
```