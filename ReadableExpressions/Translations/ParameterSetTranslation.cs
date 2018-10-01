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

    internal class ParameterSetTranslation
    {
        private readonly IList<ParameterTranslation> _parameterTranslations;

        public ParameterSetTranslation(IEnumerable<ParameterExpression> parameters)
        {
            _parameterTranslations = parameters
                .Project(p => new ParameterTranslation(p))
                .ToArray();
        }
    }
}