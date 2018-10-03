using AgileObjects.ReadableExpressions.Translators;

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
        private const string _openAndCloseParentheses = "()";

        private readonly IList<ITranslation> _parameterTranslations;
        private bool _forceParentheses;

        public ParameterSetTranslation(ICollection<ParameterExpression> parameters, ITranslationContext context)
#if NET35
            : this(null, parameters.Cast<Expression>(), parameters.Count, context)
#else
            : this(null, parameters, parameters.Count, context)
#endif
        {
        }

        public ParameterSetTranslation(
            IMethodInfo method,
            ICollection<Expression> parameters,
            ITranslationContext context)
            : this(method, parameters, parameters.Count, context)
        {
        }

        private ParameterSetTranslation(
            IMethodInfo method,
            IEnumerable<Expression> parameters,
            int parameterCount,
            ITranslationContext context)
        {
            if (method?.IsExtensionMethod == true)
            {
                parameters = parameters.Skip(1);
                --parameterCount;
            }

            ParameterCount = parameterCount;

            if (parameterCount == 0)
            {
                _parameterTranslations = Enumerable<ITranslation>.EmptyArray;
                EstimatedSize = _openAndCloseParentheses.Length;
                return;
            }

            var estimatedSize = 0;

            _parameterTranslations = parameters
                .Project(p =>
                {
                    var translation = context.GetTranslationFor(p);

                    estimatedSize += translation.EstimatedSize;

                    return translation;
                })
                .ToArray();


            EstimatedSize = estimatedSize + (ParameterCount * 2) + 4;
        }

        public int EstimatedSize { get; }

        private int ParameterCount { get; }

        public ParameterSetTranslation WithParentheses()
        {
            _forceParentheses = true;
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            switch (ParameterCount)
            {
                case 0:
                    context.WriteToTranslation(_openAndCloseParentheses);
                    return;

                case 1 when (_forceParentheses == false):
                    _parameterTranslations[0].WriteTo(context);
                    return;
            }

            context.WriteToTranslation('(');

            for (var i = 0; i < ParameterCount; ++i)
            {
                _parameterTranslations[i].WriteTo(context);

                if (i < ParameterCount - 1)
                {
                    context.WriteToTranslation(", ");
                }
            }

            context.WriteToTranslation(')');
        }
    }
}