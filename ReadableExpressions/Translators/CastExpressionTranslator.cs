namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    internal class CastExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<ExpressionType, Translator> _translatorsByType;

        internal CastExpressionTranslator(Translator globalTranslator)
            : base(
                globalTranslator,
                ExpressionType.Convert,
                ExpressionType.ConvertChecked,
                ExpressionType.TypeAs,
                ExpressionType.TypeIs,
                ExpressionType.Unbox)
        {
            _translatorsByType = new Dictionary<ExpressionType, Translator>
            {
                [ExpressionType.Convert] = TranslateCast,
                [ExpressionType.ConvertChecked] = TranslateCast,
                [ExpressionType.TypeAs] = TranslateTypeAs,
                [ExpressionType.TypeIs] = TranslateTypeIs,
                [ExpressionType.Unbox] = TranslateCastCore,
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            return _translatorsByType[expression.NodeType].Invoke(expression, context);
        }

        private string TranslateCast(Expression expression, TranslationContext context)
        {
            var operand = ((UnaryExpression)expression).Operand;

            if (expression.Type == typeof(object))
            {
                // Don't bother showing a boxing operation:
                return GetTranslation(operand, context);
            }

            if ((operand.NodeType == ExpressionType.Call) && (operand.Type == typeof(Delegate)))
            {
                var methodCall = ((MethodCallExpression)operand);

                if (methodCall.Method.Name == "CreateDelegate")
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var subjectMethod = (MethodInfo)((ConstantExpression)methodCall.Object).Value;

                    var methodSubject = subjectMethod.IsStatic
                        ? subjectMethod.DeclaringType.GetFriendlyName()
                        : GetTranslation(methodCall.Arguments.ElementAtOrDefault(1), context);

                    return methodSubject + "." + subjectMethod.Name;
                }
            }

            return TranslateCastCore(expression, context);
        }

        private string TranslateCastCore(Expression expression, TranslationContext context)
        {
            var cast = (UnaryExpression)expression;
            var typeName = cast.Type.GetFriendlyName();
            var subject = GetTranslation(cast.Operand, context);

            return $"(({typeName}){subject})";
        }

        private string TranslateTypeAs(Expression expression, TranslationContext context)
        {
            var typeAs = (UnaryExpression)expression;
            var typeName = typeAs.Type.GetFriendlyName();
            var subject = GetTranslation(typeAs.Operand, context);

            return $"({subject} as {typeName})";
        }

        private string TranslateTypeIs(Expression expression, TranslationContext context)
        {
            var typeIs = (TypeBinaryExpression)expression;
            var typeName = typeIs.TypeOperand.GetFriendlyName();
            var subject = GetTranslation(typeIs.Expression, context);

            return $"({subject} is {typeName})";
        }
    }
}