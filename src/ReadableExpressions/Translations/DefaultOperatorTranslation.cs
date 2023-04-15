namespace AgileObjects.ReadableExpressions.Translations;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

/// <summary>
/// A <see cref="UnaryOperatorTranslationBase"/> for the default operator.
/// </summary>
public sealed class DefaultOperatorTranslation : UnaryOperatorTranslationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultOperatorTranslation"/> class.
    /// </summary>
    /// <param name="type">The Type to which the default operator is being applied.</param>
    /// <param name="context">The <see cref="ITranslationContext"/> to use.</param>
    public DefaultOperatorTranslation(Type type, ITranslationContext context) :
        this(context.GetTranslationFor(type))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultOperatorTranslation"/> class.
    /// </summary>
    /// <param name="operandTranslation">
    /// The <see cref="ITranslation"/> which will write the symbol to which the default operator
    /// is being applied.
    /// </param>
    public DefaultOperatorTranslation(ITranslation operandTranslation) : 
        base("default", operandTranslation)
    {
    }

    /// <summary>
    /// Gets the ExpressionType of the translated default() operator use - ExpressionType.Default.
    /// </summary>
    public override ExpressionType NodeType => ExpressionType.Default;
}