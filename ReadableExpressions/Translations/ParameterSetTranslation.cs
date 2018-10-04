using AgileObjects.ReadableExpressions.Translators;

namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;

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
            var methodProvided = method != null;

            if (methodProvided && method.IsExtensionMethod)
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

            ParameterInfo[] methodParameters;

            if (methodProvided)
            {
                methodParameters = method.GetParameters();
                parameters = GetAllParameters(parameters, methodParameters);
            }
            else
            {
                methodParameters = null;
            }

            var estimatedSize = 0;

            _parameterTranslations = parameters
                .Project((p, index) =>
                {
                    ITranslation translation;

                    if (methodProvided)
                    {
                        // If a parameter is a params array then index will increase
                        // past parameterCount, so adjust here:
                        var parameterIndex = (ParameterCount != parameterCount)
                            ? ParameterCount - parameterCount - index
                            : index;

                        // ReSharper disable once PossibleNullReferenceException
                        translation = GetParameterTranslation(p, methodParameters[parameterIndex], context);
                    }
                    else
                    {
                        translation = context.GetTranslationFor(p);
                    }

                    estimatedSize += translation.EstimatedSize;

                    return translation;
                })
                .ToArray();


            EstimatedSize = estimatedSize + (ParameterCount * 2) + 4;
        }

        private IEnumerable<Expression> GetAllParameters(
            IEnumerable<Expression> parameters,
            IList<ParameterInfo> methodParameters)
        {
            var i = 0;

            foreach (var parameter in parameters)
            {
                if (methodParameters[i].IsParamsArray())
                {
                    var paramsArray = (NewArrayExpression)parameter;

                    if (paramsArray.Expressions.Count > 0)
                    {
                        foreach (var paramsArrayValue in paramsArray.Expressions)
                        {
                            yield return paramsArrayValue;
                            ++ParameterCount;
                        }
                    }

                    --ParameterCount;
                    continue;
                }

                yield return parameter;
                ++i;
            }
        }

        private ITranslation GetParameterTranslation(
            Expression parameter,
            ParameterInfo info,
            ITranslationContext context)
        {
            var translation = context.GetTranslationFor(parameter);

            return translation;
        }

        public int EstimatedSize { get; }

        private int ParameterCount { get; set; }

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

            for (var i = 0; ; ++i)
            {
                _parameterTranslations[i].WriteTo(context);

                if (i == (ParameterCount - 1))
                {
                    break;
                }

                context.WriteToTranslation(", ");
            }

            context.WriteToTranslation(')');
        }
    }
}