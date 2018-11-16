namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using Extensions;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Interfaces;

    internal class MethodGroupTranslation : ITranslation
    {
        private readonly ITranslation _subjectTranslation;
        private readonly string _subjectMethodName;

        public MethodGroupTranslation(
            ExpressionType nodeType,
            ITranslation subjectTranslation,
            MethodInfo subjectMethodInfo)
        {
            NodeType = nodeType;
            Type = subjectMethodInfo.ReturnType;
            _subjectTranslation = subjectTranslation;
            _subjectMethodName = subjectMethodInfo.Name;
            EstimatedSize = _subjectTranslation.EstimatedSize + ".".Length + _subjectMethodName.Length;
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

            return new MethodGroupTranslation(nodeType, subjectTranslation, subjectMethod);
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            _subjectTranslation.WriteTo(buffer);
            buffer.WriteToTranslation('.');
            buffer.WriteToTranslation(_subjectMethodName);
        }
    }
}