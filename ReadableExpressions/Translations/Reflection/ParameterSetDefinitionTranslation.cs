namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
    using Formatting;

    internal class ParameterSetDefinitionTranslation : ITranslatable
    {
        private readonly TranslationSettings _settings;
        private readonly IList<IParameter> _parameters;
        private readonly int _parameterCount;
        private readonly ITranslatable[] _parameterTranslations;
        private readonly bool _isExtensionMethod;

        private ParameterSetDefinitionTranslation(
            IMethodBase method,
            IList<IParameter> parameters,
            TranslationSettings settings)
        {
            _settings = settings;
            _parameters = parameters;
            _parameterCount = _parameters.Count;

            _parameterTranslations = new ITranslatable[_parameterCount];
            var translationSize = 6;
            var formattingSize = 0;
            var keywordFormattingSize = settings.GetKeywordFormattingSize();
            var finalParameterIndex = _parameterCount - 1;

            for (var i = 0; ;)
            {
                var parameter = _parameters[i];
                var parameterType = parameter.Type;

                if (parameter.IsOut)
                {
                    parameterType = parameterType.ElementType;
                    formattingSize += keywordFormattingSize;
                }
                else if (parameterType.IsByRef)
                {
                    parameterType = parameterType.ElementType;
                    formattingSize += keywordFormattingSize;
                }
                else if (i == finalParameterIndex && parameter.IsParamsArray)
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
                FormattingSize += keywordFormattingSize;
            }
        }

        #region Factory Methods

        public static ITranslatable For(MethodInfo method, TranslationSettings settings)
            => For(new ClrMethodWrapper(method, settings), settings);

        public static ITranslatable For(IMethodBase method, TranslationSettings settings)
        {
            var parameters = method.GetParameters();

            return parameters.Any()
                ? new ParameterSetDefinitionTranslation(method, parameters, settings)
                : EmptyParameterSetDefinitionTranslation.Instance;
        }

        #endregion

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize() => _parameterCount * _settings.Indent.Length;

        public int GetLineCount() => _parameterCount + 2;

        public void WriteTo(TranslationWriter writer)
        {
            var finalParameterIndex = _parameterCount - 1;

            writer.WriteNewLineToTranslation();
            writer.WriteToTranslation('(');
            writer.Indent();

            for (var i = 0; ;)
            {
                var parameter = _parameters[i];
                var parameterType = parameter.Type;

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
                else if (i == finalParameterIndex && parameter.IsParamsArray)
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

        private class EmptyParameterSetDefinitionTranslation : ITranslatable
        {
            public static readonly ITranslatable Instance =
                new EmptyParameterSetDefinitionTranslation();

            public int TranslationSize => 2;

            public int FormattingSize => 0;

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
                => writer.WriteToTranslation("()");
        }
    }
}