namespace AgileObjects.ReadableExpressions.Translations;

/// <summary>
/// Implementing classes will translate an Expression to a source-code string and provide the
/// ExpressionType of the translated Expression.
/// </summary>
public interface INodeTranslation : ITranslation
{
    /// <summary>
    /// Gets the ExpressionType of the translated Expression.
    /// </summary>
    ExpressionType NodeType { get; }
}