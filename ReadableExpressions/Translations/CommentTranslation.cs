namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Formatting;

internal class CommentTranslation : INodeTranslation, IPotentialSelfTerminatingTranslation
{
    private readonly string _comment;

    public CommentTranslation(CommentExpression comment)
    {
        _comment = comment.Comment.Text;
    }

    public ExpressionType NodeType => ExpressionType.Constant;

    public int TranslationLength => _comment.Length;

    public bool IsTerminated => true;

    public void WriteTo(TranslationWriter writer)
        => writer.WriteToTranslation(_comment, TokenType.Comment);
}