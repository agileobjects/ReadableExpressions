namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Extensions;
    using Interfaces;

    internal class ArrayInitialisationTranslation : InitialisationTranslationBase<Expression>
    {
        private ArrayInitialisationTranslation(NewArrayExpression arrayInit, ITranslationContext context)
            : base(
                NewArrayInit,
                GetNewArrayTranslation(arrayInit, context),
                new ArrayInitializerSetTranslation(arrayInit, context))
        {
        }

        private static ITranslation GetNewArrayTranslation(NewArrayExpression arrayInit, ITranslationContext context)
        {
            var expressionTypes = arrayInit
                .Expressions
                .Project(exp => exp.Type)
                .Distinct()
                .ToArray();

            if (expressionTypes.Length == 1)
            {
                return new FixedValueTranslation(NewArrayInit, "new[]", arrayInit.Type, TokenType.Default);
            }

            return GetNewBoundedArrayTranslation(arrayInit, context).WrappedWith("new ", "[]");
        }

        public static ITranslation For(NewArrayExpression arrayInit, ITranslationContext context)
        {
            if (arrayInit.Expressions.Count == 0)
            {
                return GetNewBoundedArrayTranslation(arrayInit, context).WrappedWith("new ", "[0]");
            }

            return new ArrayInitialisationTranslation(arrayInit, context);

        }

        private static TranslationWrapper GetNewBoundedArrayTranslation(Expression arrayInit, ITranslationContext context)
        {
            var emptyArrayNewing = context.GetTranslationFor(arrayInit.Type.GetElementType());

            return emptyArrayNewing.WithNodeType(NewArrayInit);
        }
    }
}