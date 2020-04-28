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
    using static Formatting.TokenType;

    internal static class ParameterTranslation
    {
        public static ITranslation For(ParameterExpression parameter, ITranslationContext context)
        {
            if (parameter.Name.IsNullOrWhiteSpace())
            {
                return new UnnamedParameterTranslation(parameter, context);
            }

            return new StandardParameterTranslation(parameter, context);
        }

        private class StandardParameterTranslation : ITranslation
        {
            private readonly ParameterExpression _parameter;
            private readonly string _parameterName;

            public StandardParameterTranslation(
                ParameterExpression parameter,
                ITranslationContext context)
            {
                _parameter = parameter;
                _parameterName = parameter.Name;

                if (IsKeyword(_parameterName))
                {
                    _parameterName = "@" + _parameterName;
                }

                TranslationSize = _parameterName.Length;
                FormattingSize = context.GetVariableFormattingSize();
            }

            public ExpressionType NodeType => ExpressionType.Parameter;

            public Type Type => _parameter.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
                => buffer.WriteToTranslation(_parameterName, Variable);
        }

        private class UnnamedParameterTranslation : ITranslation
        {
            private readonly ParameterExpression _parameter;
            private readonly string _parameterName;

            public UnnamedParameterTranslation(ParameterExpression parameter, ITranslationContext context)
            {
                _parameter = parameter;
                _parameterName = parameter.Type.GetVariableNameInCamelCase(context.Settings);

                var variableNumber = context.GetUnnamedVariableNumberOrNull(parameter);

                if (variableNumber.HasValue)
                {
                    _parameterName += variableNumber.Value;
                }
                else if (IsKeyword(_parameterName))
                {
                    _parameterName = "@" + _parameterName;
                }

                TranslationSize = _parameterName.Length;
                FormattingSize = context.GetVariableFormattingSize();
            }

            public ExpressionType NodeType => ExpressionType.Parameter;

            public Type Type => _parameter.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
                => buffer.WriteToTranslation(_parameterName, Variable);
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