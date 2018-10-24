namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal abstract class InitialisationTranslationBase<TInitializer> : ITranslation
    {
        private readonly NewingTranslation _newingTranslation;

        protected InitialisationTranslationBase(
            ExpressionType initType,
            NewExpression newing,
            ICollection<TInitializer> initializers,
            ITranslationContext context)
        {
            NodeType = initType;
            _newingTranslation = new NewingTranslation(newing, context);
            HasInitializers = initializers.Count != 0;

            if (HasNoInitializers)
            {
                EstimatedSize = _newingTranslation.EstimatedSize;
                return;
            }

            _newingTranslation = _newingTranslation.WithoutParenthesesIfParameterless();
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; protected set; }

        protected ITranslation NewingTranslation => _newingTranslation;

        protected bool HasInitializers { get; }

        protected bool HasNoInitializers => !HasInitializers;

        public void WriteTo(ITranslationContext context)
        {
            _newingTranslation.WriteTo(context);

            if (HasNoInitializers)
            {
                return;
            }

            context.WriteOpeningBraceToTranslation();
            WriteInitializers(context);
            context.WriteClosingBraceToTranslation();
        }

        protected abstract void WriteInitializers(ITranslationContext context);
    }
}