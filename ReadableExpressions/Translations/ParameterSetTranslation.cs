namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class ParameterSetTranslation : ITranslation
    {
        private readonly IList<ParameterTranslation> _parameterTranslations;

        public ParameterSetTranslation(IEnumerable<ParameterExpression> parameters, ITranslationContext context)
        {
            var estimatedSize = 0;

            _parameterTranslations = parameters
                .Project(p =>
                {
                    var translation = new ParameterTranslation(p, context);

                    estimatedSize += translation.EstimatedSize;

                    return translation;
                })
                .ToArray();

            EstimatedSize = estimatedSize + (_parameterTranslations.Count * 2) + 4;
        }

        public int EstimatedSize { get; }

        public void Translate()
        {
            throw new System.NotImplementedException();
        }
    }
}