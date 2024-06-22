namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using Extensions;
using NetStandardPolyfills;

internal class MemberAccessTranslation : INodeTranslation
{
    private readonly ITranslationContext _context;
    private readonly INodeTranslation _subjectTranslation;
    private readonly string _memberName;

    public MemberAccessTranslation(
        INodeTranslation subjectTranslation,
        string memberName,
        ITranslationContext context)
    {
        _subjectTranslation = subjectTranslation;
        _memberName = memberName;
        _context = context;
    }

    #region Factory Methods

    public static INodeTranslation For(
        MemberExpression memberAccess,
        ITranslationContext context)
    {
        bool translateSubject;

        if (memberAccess.IsCapture(out var capture))
        {
            if (context.Settings.ShowCapturedValues &&
                ConstantTranslation.TryCreateValueTranslation(
                    capture.Object,
                    capture.Type,
                    context,
                    out var valueTranslation))
            {
                return valueTranslation;
            }

            translateSubject = capture.IsStatic;
        }
        else
        {
            translateSubject = true;
        }

        if (IsIndexedPropertyAccess(memberAccess, context, out var translation))
        {
            return translation;
        }

        var subjectTranslation = translateSubject
            ? TranslateSubject(memberAccess, context)
            : null;

        return new MemberAccessTranslation(
            subjectTranslation,
            memberAccess.Member.Name,
            context);
    }

    private static bool IsIndexedPropertyAccess(
        MemberExpression memberAccess,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        if (memberAccess.Member is not PropertyInfo property)
        {
            translation = null;
            return false;
        }

        var indexParameters = property.GetIndexParameters();

        if (indexParameters.None())
        {
            translation = null;
            return false;
        }

        var method = memberAccess.Type != typeof(void)
            ? property.GetGetter() : property.GetSetter();

        var arguments = indexParameters.ProjectToArray(p =>
        {
            if (p.DefaultValue != null)
            {
                return Expression.Constant(p.DefaultValue, p.ParameterType);
            }

            return (Expression)Expression.Default(p.ParameterType);
        });

        var indexedPropertyCall = Expression.Call(
            memberAccess.Expression,
            method,
            arguments);

        translation = MethodCallTranslation.For(indexedPropertyCall, context);
        return true;
    }

    private static INodeTranslation TranslateSubject(
        MemberExpression memberAccess,
        ITranslationContext context)
    {
        return GetSubjectTranslationOrNull(
            memberAccess.Expression,
            memberAccess.Member,
            context);
    }

    protected static INodeTranslation GetSubjectTranslationOrNull(
        Expression subject,
        MemberInfo member,
        ITranslationContext context)
    {
        return subject != null
            ? context.GetTranslationFor(subject)
            : context.GetTranslationFor(member.DeclaringType);
    }

    #endregion

    public ExpressionType NodeType => ExpressionType.MemberAccess;

    public int TranslationLength =>
        _subjectTranslation != null
            ? _subjectTranslation.TranslationLength + ".".Length + _memberName.Length
            : _memberName.Length;

    public void WriteTo(TranslationWriter writer)
    {
        if (_subjectTranslation != null)
        {
            _subjectTranslation.WriteInParenthesesIfRequired(writer, _context);
            writer.WriteDotToTranslation();
        }

        writer.WriteToTranslation(_memberName);
    }
}