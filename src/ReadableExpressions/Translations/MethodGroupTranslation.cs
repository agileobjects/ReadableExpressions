namespace AgileObjects.ReadableExpressions.Translations;

using System.Linq;
using System.Reflection;
using Extensions;
using static Formatting.TokenType;

internal class MethodGroupTranslation : INodeTranslation
{
    private readonly INodeTranslation _subjectTranslation;
    private readonly string _subjectMethodName;

    public MethodGroupTranslation(
        ExpressionType nodeType,
        INodeTranslation subjectTranslation,
        MethodInfo subjectMethodInfo)
    {
        NodeType = nodeType;
        _subjectTranslation = subjectTranslation;
        _subjectMethodName = subjectMethodInfo.Name;
    }

    public static INodeTranslation ForCreateDelegateCall(
        ExpressionType nodeType,
        MethodCallExpression createDelegateCall,
        ITranslationContext context)
    {
#if NET35
        var subjectMethod = (MethodInfo)
            ((ConstantExpression)createDelegateCall.Arguments.Last()).Value;
#else
        var subjectMethod = (MethodInfo)
            ((ConstantExpression)createDelegateCall.Object)!.Value;
#endif
        var subjectTranslation = subjectMethod.IsStatic
            ? context.GetTranslationFor(subjectMethod.DeclaringType)
            : context.GetTranslationFor(createDelegateCall.Arguments.ElementAtOrDefault(1));

        return new MethodGroupTranslation(nodeType, subjectTranslation, subjectMethod);
    }

    public ExpressionType NodeType { get; }

    public int TranslationLength =>
        _subjectTranslation.TranslationLength + ".".Length + _subjectMethodName.Length;

    public void WriteTo(TranslationWriter writer)
    {
        _subjectTranslation.WriteTo(writer);
        writer.WriteDotToTranslation();
        writer.WriteToTranslation(_subjectMethodName, MethodName);
    }
}