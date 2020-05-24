namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal abstract class NewingTranslationBase
    {
        protected NewingTranslationBase(NewExpression newing, ITranslationContext context)
        {
            Parameters = new ParameterSetTranslation(
                new CtorInfoWrapper(newing.Constructor),
                newing.Arguments,
                context);
        }

        public ExpressionType NodeType => ExpressionType.New;

        protected ParameterSetTranslation Parameters { get; }
    }
}