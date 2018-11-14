namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
#if NET35
    using Extensions;
#endif
    using Interfaces;

    internal class MethodGroupTranslation : ITranslation
    {
        private readonly ITranslation _subjectTranslation;
        private readonly string _subjectMethodName;

        public MethodGroupTranslation(
            ExpressionType nodeType,
            MethodCallExpression methodCall,
            MemberInfo subjectMethodInfo,
            ITranslationContext context)
        {
            NodeType = nodeType;

            _subjectTranslation = MethodCallTranslation.GetSubjectTranslation(methodCall, context);

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
            return new MethodGroupTranslation(nodeType, createDelegateCall, subjectMethod, context);
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _subjectTranslation.WriteTo(context);
            context.WriteToTranslation('.');
            context.WriteToTranslation(_subjectMethodName);
        }
    }
}