namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
#if NET35
    using Extensions;
#endif
    using static Formatting.TokenType;

    internal class MethodGroupTranslation : ITranslation
    {
        private readonly ITranslation _subjectTranslation;
        private readonly string _subjectMethodName;

        public MethodGroupTranslation(
            ExpressionType nodeType,
            ITranslation subjectTranslation,
            MethodInfo subjectMethodInfo,
            ITranslationContext context)
        {
            NodeType = nodeType;
            Type = subjectMethodInfo.ReturnType;
            _subjectTranslation = subjectTranslation;
            _subjectMethodName = subjectMethodInfo.Name;
            TranslationSize = _subjectTranslation.TranslationSize + ".".Length + _subjectMethodName.Length;
            FormattingSize = _subjectTranslation.FormattingSize + context.GetFormattingSize(MethodName);
        }

        public static ITranslation ForCreateDelegateCall(
            ExpressionType nodeType,
            MethodCallExpression createDelegateCall,
            ITranslationContext context)
        {
#if NET35
            var subjectMethod = (MethodInfo)((ConstantExpression)createDelegateCall.Arguments.Last()).Value;
#else
            // ReSharper disable once PossibleNullReferenceException
            var subjectMethod = (MethodInfo)((ConstantExpression)createDelegateCall.Object).Value;
#endif
            var subjectTranslation = subjectMethod.IsStatic
                ? context.GetTranslationFor(subjectMethod.DeclaringType)
                : context.GetTranslationFor(createDelegateCall.Arguments.ElementAtOrDefault(1));

            return new MethodGroupTranslation(nodeType, subjectTranslation, subjectMethod, context);
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _subjectTranslation.GetIndentSize();

        public int GetLineCount() => _subjectTranslation.GetLineCount();

        public void WriteTo(TranslationWriter writer)
        {
            _subjectTranslation.WriteTo(writer);
            writer.WriteDotToTranslation();
            writer.WriteToTranslation(_subjectMethodName, MethodName);
        }
    }
}