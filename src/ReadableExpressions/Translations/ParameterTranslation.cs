namespace AgileObjects.ReadableExpressions.Translations;

#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;
using static System.StringComparison;
using static Formatting.TokenType;

internal static class ParameterTranslation
{
    public static INodeTranslation For(
        ParameterExpression parameter,
        ITranslationContext context)
    {
        if (!parameter.IsNamed())
        {
            return new UnnamedParameterTranslation(parameter, context);
        }

        if (parameter.Name.StartsWith("<>", Ordinal))
        {
            return new FixedValueTranslation(
                ExpressionType.Parameter,
                "_",
                Variable);
        }

        return new StandardParameterTranslation(parameter);
    }

    private abstract class ParameterTranslationBase : IParameterTranslation
    {
        private readonly ParameterExpression _parameter;
        private INodeTranslation _typeNameTranslation;

        protected ParameterTranslationBase(
            ParameterExpression parameter,
            string parameterName)
        {
            _parameter = parameter;
            Name = parameterName;
        }

        public ExpressionType NodeType => ExpressionType.Parameter;

        public int TranslationLength
        {
            get
            {
                var translationLength = Name.Length;

                if (_typeNameTranslation != null)
                {
                    translationLength += _typeNameTranslation.TranslationLength;
                }

                return translationLength;
            }
        }

        public string Name { get; }

        public INodeTranslation WithTypeNames(ITranslationContext context)
            => _typeNameTranslation ??= context.GetTranslationFor(_parameter.Type);

        public void WithoutTypeNames(ITranslationContext context)
            => _typeNameTranslation = null;

        public void WriteTo(TranslationWriter writer)
        {
            if (_typeNameTranslation != null)
            {
                _typeNameTranslation.WriteTo(writer);
                writer.WriteSpaceToTranslation();
            }

            writer.WriteToTranslation(Name, Variable);
        }
    }

    private class StandardParameterTranslation : ParameterTranslationBase
    {
        public StandardParameterTranslation(ParameterExpression parameter) :
            base(parameter, GetParameterName(parameter))
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
            ITranslationContext context) :
            base(parameter, GetParameterName(parameter, context))
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
            case "const":
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