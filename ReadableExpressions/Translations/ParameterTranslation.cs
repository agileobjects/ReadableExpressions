namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using static TokenType;

    internal static class ParameterTranslation
    {
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
                _useLiteralPrefix = IsKeyword(parameter.Name);
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
                    buffer.WriteToTranslation('@', Variable);
                }

                buffer.WriteToTranslation(_parameter.Name, Variable);
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
                _useLiteralPrefix = (_variableNumber == null) && IsKeyword(_parameterName);
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
                    buffer.WriteToTranslation('@', Variable);
                }

                buffer.WriteToTranslation(_parameterName, Variable);

                if (_variableNumber != null)
                {
                    buffer.WriteToTranslation(_variableNumber.Value, Variable);
                }
            }
        }

        private static bool IsKeyword(string variableName)
        {
            switch (variableName)
            {
                case "typeof":
                case "default":
                case "void":
                case "readonly":
                case "do":
                case "while":
                case "switch":
                case "if":
                case "else":
                case "try":
                case "catch":
                case "finally":
                case "throw":
                case "for":
                case "foreach":
                case "goto":
                case "return":
                case "implicit":
                case "explicit":
                    return true;

                default:
                    return InternalReflectionExtensions.TypeNames.Contains(variableName);
            }
        }
    }
}