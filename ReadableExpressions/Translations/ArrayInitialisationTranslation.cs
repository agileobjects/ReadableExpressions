namespace AgileObjects.ReadableExpressions.Translations
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
                return new FixedValueTranslation(NewArrayInit, "new[]");
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

        protected override bool WriteLongTranslationsToMultipleLines => false;

        private class ArrayInitializerSetTranslation : InitializerSetTranslation
        {
            public ArrayInitializerSetTranslation(NewArrayExpression arrayInit, ITranslationContext context)
                : base(arrayInit.Expressions, context)
            {
            }

            protected override ITranslatable GetTranslation(Expression initializer, ITranslationContext context)
                => context.GetCodeBlockTranslationFor(initializer);

            public override bool WriteToMultipleLines => false;
        }
    }
}