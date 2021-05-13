namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
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

        private abstract class ParameterTranslationBase : IParameterTranslation
        {
            private readonly ParameterExpression _parameter;
            private readonly string _parameterName;
            private ITranslation _typeNameTranslation;

            protected ParameterTranslationBase(
                ParameterExpression parameter,
                string parameterName,
                ITranslationContext context)
            {
                _parameter = parameter;
                _parameterName = parameterName;

                TranslationSize = _parameterName.Length;
                FormattingSize = context.GetFormattingSize(Variable);
            }

            public ExpressionType NodeType => ExpressionType.Parameter;

            public Type Type => _parameter.Type;

            public int TranslationSize { get; private set; }

            public int FormattingSize { get; private set; }

            public void WithTypeNames(ITranslationContext context)
            {
                _typeNameTranslation = context.GetTranslationFor(Type);

                TranslationSize += _typeNameTranslation.TranslationSize;
                FormattingSize += _typeNameTranslation.FormattingSize;
            }

            public void WithoutTypeNames(ITranslationContext context)
            {
                if (_typeNameTranslation != null)
                {
                    TranslationSize -= _typeNameTranslation.TranslationSize;
                    FormattingSize -= _typeNameTranslation.FormattingSize;
                    _typeNameTranslation = null;
                }
            }

            public int GetIndentSize() => _typeNameTranslation?.GetIndentSize() ?? 0;

            public int GetLineCount() => _typeNameTranslation?.GetLineCount() ?? 1;

            public void WriteTo(TranslationWriter writer)
            {
                if (_typeNameTranslation != null)
                {
                    _typeNameTranslation.WriteTo(writer);
                    writer.WriteSpaceToTranslation();
                }

                writer.WriteToTranslation(_parameterName, Variable);
            }
        }

        private class StandardParameterTranslation : ParameterTranslationBase
        {
            public StandardParameterTranslation(
                ParameterExpression parameter,
                ITranslationContext context)
                : base(parameter, GetParameterName(parameter), context)
            {
            }

            private static string GetParameterName(ParameterExpression parameter)
            {
                var parameterName = parameter.Name;

                return IsKeyword(parameterName) ? "@" + parameterName : parameterName;
            }
        }

        private class UnnamedParameterTranslation : ParameterTranslationBase
        {
            public UnnamedParameterTranslation(
                ParameterExpression parameter,
                ITranslationContext context)
                : base(parameter, GetParameterName(parameter, context), context)
            {
            }

            private static string GetParameterName(
                ParameterExpression parameter,
                ITranslationContext context)
            {
                var parameterName = parameter.Type.GetVariableNameInCamelCase(context.Settings);
                var variableNumber = context.Analysis.GetUnnamedVariableNumberOrNull(parameter);

                if (variableNumber.HasValue)
                {
                    parameterName += variableNumber.Value;
                }
                else if (IsKeyword(parameterName))
                {
                    parameterName = "@" + parameterName;
                }

                return parameterName;
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