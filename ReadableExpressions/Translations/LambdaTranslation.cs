namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class LambdaTranslation : INodeTranslation, IPotentialMultiStatementTranslatable
{
    private const string _fatArrow = " => ";

    private readonly ParameterSetTranslation _parameters;
    private readonly CodeBlockTranslation _bodyTranslation;

    public LambdaTranslation(LambdaExpression lambda, ITranslationContext context)
    {
        _parameters = ParameterSetTranslation.For(lambda, context);
        _bodyTranslation = context.GetCodeBlockTranslationFor(lambda.Body);

        if (_bodyTranslation.IsMultiStatement == false)
        {
            _bodyTranslation.WithoutTermination();
        }
    }

    public ExpressionType NodeType => ExpressionType.Lambda;

    public int TranslationLength =>
        _parameters.TranslationLength +
        _fatArrow.Length +
        _bodyTranslation.TranslationLength;

    public bool IsMultiStatement => _bodyTranslation.IsMultiStatement;

    public void WriteTo(TranslationWriter writer)
    {
        _parameters.WriteTo(writer);

        writer.WriteToTranslation(_bodyTranslation.HasBraces ? " =>" : _fatArrow);

        _bodyTranslation.WriteTo(writer);
    }
}