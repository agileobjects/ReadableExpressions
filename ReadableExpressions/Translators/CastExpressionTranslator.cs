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
        private static readonly Dictionary<ExpressionType, Translator> _translatorsByType =
            new Dictionary<ExpressionType, Translator>
            {
                [ExpressionType.Convert] = TranslateCast,
                [ExpressionType.ConvertChecked] = TranslateCast,
                [ExpressionType.TypeAs] = TranslateTypeAs,
                [ExpressionType.TypeIs] = TranslateTypeIs,
                [ExpressionType.Unbox] = TranslateCastCore,
            };

        internal CastExpressionTranslator()
            : base(_translatorsByType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            return _translatorsByType[expression.NodeType].Invoke(expression, context);
        }

        private static string TranslateCast(Expression expression, TranslationContext context)
        {
            var operand = ((UnaryExpression)expression).Operand;

            if (expression.Type == typeof(object))
            {
                // Don't bother showing a boxing operation:
                return context.GetTranslation(operand);
            }

            MethodCallExpression methodCall;

            if ((operand.NodeType == ExpressionType.Call) &&
                (operand.Type == typeof(Delegate)) &&
                ((methodCall = ((MethodCallExpression)operand)).Method.Name == "CreateDelegate"))
            {
                // ReSharper disable once PossibleNullReferenceException
                var subjectMethod = (MethodInfo)((ConstantExpression)methodCall.Object).Value;

                var methodSubject = subjectMethod.IsStatic
                    ? subjectMethod.DeclaringType.GetFriendlyName()
                    : context.GetTranslation(methodCall.Arguments.ElementAtOrDefault(1));

                return methodSubject + "." + subjectMethod.Name;
            }

            return TranslateCastCore(expression, context);
        }

        private static string TranslateCastCore(Expression expression, TranslationContext context)
        {
            var cast = (UnaryExpression)expression;
            var typeName = cast.Type.GetFriendlyName();
            var subject = context.GetTranslation(cast.Operand);

            if (cast.Operand.NodeType == ExpressionType.Assign)
            {
                subject = subject.WithSurroundingParentheses();
            }

            return $"(({typeName}){subject})";
        }

        private static string TranslateTypeAs(Expression expression, TranslationContext context)
        {
            var typeAs = (UnaryExpression)expression;
            var typeName = typeAs.Type.GetFriendlyName();
            var subject = context.GetTranslation(typeAs.Operand);

            return $"({subject} as {typeName})";
        }

        private static string TranslateTypeIs(Expression expression, TranslationContext context)
        {
            var typeIs = (TypeBinaryExpression)expression;
            var typeName = typeIs.TypeOperand.GetFriendlyName();
            var subject = context.GetTranslation(typeIs.Expression);

            return $"({subject} is {typeName})";
        }
    }
}