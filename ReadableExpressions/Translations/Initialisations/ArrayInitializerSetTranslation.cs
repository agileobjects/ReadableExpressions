namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class ArrayInitializerSetTranslation : InitializerSetTranslationBase<Expression>
    {
        public ArrayInitializerSetTranslation(NewArrayExpression arrayInit, ITranslationContext context)
            : base(arrayInit.Expressions, context)
        {
        }

        protected override ITranslatable GetTranslation(Expression initializer, ITranslationContext context)
            => context.GetCodeBlockTranslationFor(initializer);

        public override bool ForceWriteToMultipleLines => false;
    }
}