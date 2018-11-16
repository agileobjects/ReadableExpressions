namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal static class ParameterTranslation
    {
        // TODO: array-specific Combine overload
        private static readonly IEnumerable<string> _keywords = InternalTypeExtensions
            .TypeNames
            .Combine(new[]
            {
                "typeof",
                "default",
                "void",
                "readonly",
                "do",
                "while",
                "switch",
                "if",
                "else",
                "try",
                "catch",
                "finally",
                "throw",
                "for",
                "foreach",
                "goto",
                "return",
                "implicit",
                "explicit"
            })
            .ToArray();

        public static ITranslation For(ParameterExpression parameter, ITranslationContext context)
        {
            if (parameter.Name.IsNullOrWhiteSpace())
            {
                return new UnnamedParameterTranslation(parameter, context);
            }

            return new StandardParameterTranslation(parameter);
        }

        private class StandardParameterTranslation : ITranslation
        {
            private readonly ParameterExpression _parameter;
            private readonly bool _useLiteralPrefix;

            public StandardParameterTranslation(ParameterExpression parameter)
            {
                _parameter = parameter;
                _useLiteralPrefix = _keywords.Contains(parameter.Name);
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                var estimatedSize = _parameter.Name.Length;

                if (_useLiteralPrefix)
                {
                    ++estimatedSize;
                }

                return estimatedSize;
            }

            public ExpressionType NodeType => ExpressionType.Parameter;

            public Type Type => _parameter.Type;

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                if (_useLiteralPrefix)
                {
                    buffer.WriteToTranslation('@');
                }

                buffer.WriteToTranslation(_parameter.Name);
            }
        }

        private class UnnamedParameterTranslation : ITranslation
        {
            private readonly ParameterExpression _parameter;
            private readonly int? _variableNumber;
            private readonly string _parameterName;
            private readonly bool _useLiteralPrefix;

            public UnnamedParameterTranslation(ParameterExpression parameter, ITranslationContext context)
            {
                _parameter = parameter;
                _variableNumber = context.GetUnnamedVariableNumberOrNull(parameter);
                _parameterName = parameter.Type.GetVariableNameInCamelCase(context.Settings);
                _useLiteralPrefix = (_variableNumber == null) && _keywords.Contains(_parameterName);
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                var estimatedSize = _parameterName.Length;

                if (_useLiteralPrefix)
                {
                    ++estimatedSize;
                }

                if (_variableNumber.HasValue)
                {
                    estimatedSize += 2;
                }

                return estimatedSize;
            }

            public ExpressionType NodeType => ExpressionType.Parameter;

            public Type Type => _parameter.Type;

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                if (_useLiteralPrefix)
                {
                    buffer.WriteToTranslation('@');
                }

                buffer.WriteToTranslation(_parameterName);

                if (_variableNumber != null)
                {
                    buffer.WriteToTranslation(_variableNumber.Value.ToString());
                }
            }
        }
    }
}