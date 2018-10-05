namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class InitialisationTranslation : ITranslation
    {
        private readonly NewingTranslation _newingTranslation;
        private readonly IList<ElementInit> _initializers;

        public InitialisationTranslation(ListInitExpression listInit, ITranslationContext context)
        {
            _newingTranslation = new NewingTranslation(listInit.NewExpression, context);
            _initializers = listInit.Initializers;

            if (_initializers.Count != 0)
            {
                _newingTranslation = _newingTranslation.WithoutParenthesesIfParameterless();
            }
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _newingTranslation.WriteTo(context);

            if (_initializers.Count == 0)
            {
                return;
            }

            context.WriteNewLineToTranslation();
            context.WriteToTranslation('{');
            context.WriteNewLineToTranslation();
            context.Indent();

            foreach (var initializer in _initializers)
            {
                var numberOfArguments = initializer.Arguments.Count;
                var hasMultipleArguments = numberOfArguments != 0;

                if (hasMultipleArguments)
                {
                    context.WriteToTranslation("{ ");
                }

                for (var i = 0; ; ++i)
                {
                    context.GetTranslationFor(initializer.Arguments[i]).WriteTo(context);

                    if (i == (numberOfArguments - 1))
                    {
                        break;
                    }

                    context.WriteToTranslation(", ");
                }

                if (hasMultipleArguments)
                {
                    context.WriteToTranslation(" }");
                }
            
                context.WriteNewLineToTranslation();
            }
            
            context.Unindent();
            context.WriteToTranslation('}');
        }
    }
}