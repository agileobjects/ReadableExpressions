namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Extensions;
    using Formatting;
    using Interfaces;
    using NetStandardPolyfills;

    internal class ParameterSetDefinitionTranslation : ITranslatable
    {
        private readonly TranslationSettings _settings;
        private readonly ParameterInfo[] _parameters;
        private readonly int _parameterCount;
        private readonly ITranslatable[] _parameterTranslations;
        private readonly bool _isExtensionMethod;

        public ParameterSetDefinitionTranslation(
            MethodInfo method,
            TranslationSettings settings)
            : this(new BclMethodWrapper(method), settings)
        {
        }

        public ParameterSetDefinitionTranslation(
            IMethod method,
            TranslationSettings settings)
        {
            _settings = settings;
            _parameters = method.GetParameters();
            _parameterCount = _parameters.Length;

            if (_parameterCount == 0)
            {
                _parameterTranslations = Enumerable<ITranslatable>.EmptyArray;
                TranslationSize = 2;
                return;
            }

            _parameterTranslations = new ITranslatable[_parameterCount];
            var translationSize = 6;
            var formattingSize = 0;
            var keywordFormattingSize = settings.GetKeywordFormattingSize();
            var finalParameterIndex = _parameterCount - 1;

            for (var i = 0; ;)
            {
                var parameter = _parameters[i];
                var parameterType = parameter.ParameterType;

                if (parameter.IsOut)
                {
                    parameterType = parameterType.GetElementType();
                    formattingSize += keywordFormattingSize;
                }
                else if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                    formattingSize += keywordFormattingSize;
                }
                else if (i == finalParameterIndex && parameter.IsParamsArray())
                {
                    formattingSize += keywordFormattingSize;
                }

                var typeNameTranslation = new TypeNameTranslation(parameterType, settings);

                translationSize += typeNameTranslation.TranslationSize;
                formattingSize += typeNameTranslation.FormattingSize;

                _parameterTranslations[i] = typeNameTranslation;

                ++i;

                if (i == _parameterCount)
                {
                    break;
                }

                translationSize += 3;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;

            if (method.IsExtensionMethod)
            {
                _isExtensionMethod = true;
                TranslationSize += "this ".Length;
                FormattingSize += settings.GetKeywordFormattingSize();
            }
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _parameterCount * _settings.Indent.Length;

        public int GetLineCount() => _parameterCount + 2;

        public void WriteTo(TranslationWriter writer)
        {
            if (_parameterCount == 0)
            {
                writer.WriteToTranslation("()");
                return;
            }

            var finalParameterIndex = _parameterCount - 1;

            writer.WriteNewLineToTranslation();
            writer.WriteToTranslation('(');
            writer.Indent();

            for (var i = 0; ;)
            {
                var parameter = _parameters[i];
                var parameterType = parameter.ParameterType;

                writer.WriteNewLineToTranslation();

                if ((i == 0) && _isExtensionMethod)
                {
                    writer.WriteKeywordToTranslation("this ");
                }
                else if (parameter.IsOut)
                {
                    writer.WriteKeywordToTranslation("out ");
                }
                else if (parameterType.IsByRef)
                {
                    writer.WriteKeywordToTranslation("ref ");
                }
                else if (i == finalParameterIndex && parameter.IsParamsArray())
                {
                    writer.WriteKeywordToTranslation("params ");
                }

                _parameterTranslations[i].WriteTo(writer);
                writer.WriteSpaceToTranslation();
                writer.WriteToTranslation(parameter.Name, TokenType.Variable);

                ++i;

                if (i == _parameterCount)
                {
                    break;
                }

                writer.WriteToTranslation(',');
            }

            writer.Unindent();
            writer.WriteNewLineToTranslation();
            writer.WriteToTranslation(')');
        }
    }
}