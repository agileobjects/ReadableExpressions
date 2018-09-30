namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using ConstantExpression = Microsoft.Scripting.Ast.ConstantExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using MethodCallExpression = Microsoft.Scripting.Ast.MethodCallExpression;
    using TypeBinaryExpression = Microsoft.Scripting.Ast.TypeBinaryExpression;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#endif
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal struct CastExpressionTranslator : IExpressionTranslator
    {
        private static readonly Dictionary<ExpressionType, Func<Expression, TranslationContext, string>> _translatorsByType =
            new Dictionary<ExpressionType, Func<Expression, TranslationContext, string>>
            {
                [ExpressionType.Convert] = TranslateCast,
                [ExpressionType.ConvertChecked] = TranslateCast,
                [ExpressionType.TypeAs] = TranslateTypeAs,
                [ExpressionType.TypeIs] = TranslateTypeIs,
                [ExpressionType.Unbox] = TranslateCastCore,
            };

        public IEnumerable<ExpressionType> NodeTypes => _translatorsByType.Keys;

        public string Translate(Expression expression, TranslationContext context)
            => _translatorsByType[expression.NodeType].Invoke(expression, context);

        private static string TranslateCast(Expression expression, TranslationContext context)
        {
            var cast = (UnaryExpression)expression;
            var operand = cast.Operand;

            if ((cast.Method != null) && !cast.Method.IsExplicitOperator() && !cast.Method.IsImplicitOperator())
            {
                return TranslateMethodConversion(cast, context);
            }

            if (expression.Type == typeof(object))
            {
                // Don't bother showing a boxing operation:
                return context.Translate(operand);
            }

            MethodCallExpression methodCall;

            if ((operand.NodeType == ExpressionType.Call) &&
                (operand.Type == typeof(Delegate)) &&
                ((methodCall = ((MethodCallExpression)operand)).Method.Name == "CreateDelegate"))
            {
#if NET35
                var subjectMethod = (MethodInfo)((ConstantExpression)methodCall.Arguments.Last()).Value;
#else
                // ReSharper disable once PossibleNullReferenceException
                var subjectMethod = (MethodInfo)((ConstantExpression)methodCall.Object).Value;
#endif

                var methodSubject = subjectMethod.IsStatic
                    ? subjectMethod.DeclaringType.GetFriendlyName(context.Settings)
                    : context.Translate(methodCall.Arguments.ElementAtOrDefault(1));

                return methodSubject + "." + subjectMethod.Name;
            }

            return TranslateCastCore(expression, context);
        }

        private static string TranslateMethodConversion(UnaryExpression cast, TranslationContext context)
        {
            return MethodCallExpressionTranslator.GetMethodCall(
                cast.Method.DeclaringType.GetFriendlyName(context.Settings),
                new BclMethodInfoWrapper(cast.Method),
                new[] { cast.Operand },
                cast,
                context);
        }

        private static string TranslateCastCore(Expression expression, TranslationContext context)
        {
            var cast = (UnaryExpression)expression;

            return Translate(cast.Operand, cast.Method, cast.Type, context);
        }

        public static string Translate(
            Expression castValue,
            MethodInfo castMethod,
            Type resultType,
            TranslationContext context)
        {
            var typeName = resultType.GetFriendlyName(context.Settings);
            var subject = context.Translate(castValue);

            if (castValue.NodeType == ExpressionType.Assign)
            {
                subject = subject.WithSurroundingParentheses();
            }

            if (castMethod?.IsImplicitOperator() == true)
            {
                return subject;
            }

            return $"(({typeName}){subject})";
        }

        private static string TranslateTypeAs(Expression expression, TranslationContext context)
        {
            var typeAs = (UnaryExpression)expression;
            var typeName = typeAs.Type.GetFriendlyName(context.Settings);
            var subject = context.Translate(typeAs.Operand);

            return $"({subject} as {typeName})";
        }

        private static string TranslateTypeIs(Expression expression, TranslationContext context)
        {
            var typeIs = (TypeBinaryExpression)expression;
            var typeName = typeIs.TypeOperand.GetFriendlyName(context.Settings);
            var subject = context.Translate(typeIs.Expression);

            return $"({subject} is {typeName})";
        }
    }
}