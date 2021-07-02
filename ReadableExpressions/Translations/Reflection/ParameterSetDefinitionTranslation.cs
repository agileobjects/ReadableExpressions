namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;
    using Formatting;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a declared set of parameters.
    /// </summary>
    public class ParameterSetDefinitionTranslation : ITranslatable
    {
        private const string _this = "this ";
        private const string _out = "out ";
        private const string _ref = "ref ";
        private const string _params = "params ";

        /// <summary>
        /// Gets a singleton <see cref="ITranslatable"/> for an empty set of parameters.
        /// </summary>
        public static readonly ITranslatable Empty = new EmptyParameterSetDefinitionTranslation();

        private readonly TranslationSettings _settings;
        private readonly IList<IParameter> _parameters;
        private readonly int _parameterCount;
        private readonly ITranslatable[] _parameterTypeTranslations;
        private readonly bool _isExtensionMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetDefinitionTranslation"/> class.
        /// </summary>
        /// <param name="method">
        /// The <see cref="IMethodBase"/> describing the method which declares the parameters to
        /// which this <see cref="ParameterSetDefinitionTranslation"/> relate.
        /// </param>
        /// <param name="parameters">One or more <see cref="IParameter"/>s describing the
        /// <paramref name="method"/>'s parameters.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        protected ParameterSetDefinitionTranslation(
            IMethodBase method,
            IList<IParameter> parameters,
            TranslationSettings settings)
        {
            _settings = settings;
            _parameters = parameters;
            _parameterCount = _parameters.Count;

            _parameterTypeTranslations = new ITranslatable[_parameterCount];
            var translationSize = 6;
            var formattingSize = 0;
            var keywordFormattingSize = settings.GetKeywordFormattingSize();
            var finalParameterIndex = _parameterCount - 1;

            for (var i = 0; ;)
            {
                var parameter = _parameters[i];
                var parameterType = parameter.Type;

                if (parameter.IsRef || parameter.IsOut)
                {
                    parameterType = parameterType.ElementType;
                    translationSize += (parameter.IsOut ? _out : _ref).Length;
                    formattingSize += keywordFormattingSize;
                }
                else if (i == finalParameterIndex && parameter.IsParamsArray)
                {
                    translationSize += _params.Length;
                    formattingSize += keywordFormattingSize;
                }

                var typeNameTranslation = new TypeNameTranslation(parameterType, settings);

                translationSize += typeNameTranslation.TranslationSize;
                formattingSize += typeNameTranslation.FormattingSize;

                _parameterTypeTranslations[i] = typeNameTranslation;

                ++i;

                if (i == _parameterCount)
                {
                    break;
                }

                translationSize += 3;
            }

            if (method.IsExtensionMethod)
            {
                _isExtensionMethod = true;
                translationSize += _this.Length;
                formattingSize += keywordFormattingSize;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        #region Factory Methods

        /// <summary>
        /// Creates a new <see cref="ParameterSetDefinitionTranslation"/> for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The MethodInfo describing the method to which the parameters belong.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>
        /// A new <see cref="ParameterSetDefinitionTranslation"/> for the given
        /// <paramref name="method"/>.
        /// </returns>
        public static ITranslatable For(MethodInfo method, TranslationSettings settings)
            => For(new ClrMethodWrapper(method, settings), settings);

        /// <summary>
        /// Creates a new <see cref="ParameterSetDefinitionTranslation"/> for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">
        /// The <see cref="IMethodBase"/> describing the method to which the parameters belong.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>
        /// A new <see cref="ParameterSetDefinitionTranslation"/> for the given
        /// <paramref name="method"/>.
        /// </returns>
        public static ITranslatable For(IMethodBase method, TranslationSettings settings)
        {
            var parameters = method.GetParameters();

            return parameters.Any()
                ? new ParameterSetDefinitionTranslation(method, parameters, settings)
                : Empty;
        }

        #endregion

        /// <inheritdoc />
        public virtual int TranslationSize { get; }

        /// <inheritdoc />
        public virtual int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize() => _parameterCount * _settings.Indent.Length;

        /// <inheritdoc />
        public int GetLineCount() => _parameterCount + 2;

        /// <inheritdoc />
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
                WriteParameterStartTo(writer, i);

                if ((i == 0) && _isExtensionMethod)
                {
                    writer.WriteKeywordToTranslation(_this);
                }
                else if (parameter.IsOut)
                {
                    writer.WriteKeywordToTranslation(_out);
                }
                else if (parameterType.IsByRef)
                {
                    writer.WriteKeywordToTranslation(_ref);
                }
                else if (i == finalParameterIndex && parameter.IsParamsArray)
                {
                    writer.WriteKeywordToTranslation(_params);
                }

                _parameterTypeTranslations[i].WriteTo(writer);
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

        /// <summary>
        /// Write characters to the given <paramref name="writer"/> to begin translation of the
        /// parameter with the given <paramref name="parameterIndex"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the characters.</param>
        /// <param name="parameterIndex">The index of the <see cref="IParameter"/> currently being translated.</param>
        protected virtual void WriteParameterStartTo(TranslationWriter writer, int parameterIndex)
        {
        }

        private class EmptyParameterSetDefinitionTranslation : ITranslatable
        {
            public int TranslationSize => 2;

            public int FormattingSize => 0;

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
                => writer.WriteToTranslation("()");
        }
    }
}