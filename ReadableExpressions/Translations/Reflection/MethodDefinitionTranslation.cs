namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a method signature, including accessibility, scope,
    /// generic arguments and constraints, and method parameters.
    /// </summary>
    public class MethodDefinitionTranslation : ITranslatable
    {
        private readonly bool _writeModifiers;
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly TypeNameTranslation _returnTypeTranslation;
        private readonly TypeNameTranslation _declaringTypeNameTranslation;
        private readonly string _methodName;
        private readonly ITranslatable _genericParametersTranslation;
        private readonly ITranslatable _genericParameterConstraintsTranslation;
        private readonly ITranslatable _parametersTranslation;

        private MethodDefinitionTranslation(
            IMethod method,
            TranslationSettings settings)
            : this(
                method,
                ParameterSetDefinitionTranslation.For(method, settings),
                includeDeclaringType: true,
                settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionTranslation"/> class for
        /// the given <paramref name="method"/>.
        /// </summary>
        /// <param name="method">
        /// The <see cref="IMethod"/> for which to create the <see cref="MethodDefinitionTranslation"/>.
        /// </param>
        /// <param name="parametersTranslation">
        /// The <see cref="ITranslatable"/> to use to translate the <paramref name="method"/>'s
        /// parameters.
        /// </param>
        /// <param name="includeDeclaringType">
        /// Whether to include the name of the <paramref name="method"/>'s declaring type in the
        /// <see cref="MethodDefinitionTranslation"/>.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public MethodDefinitionTranslation(
            IMethod method,
            ITranslatable parametersTranslation,
            bool includeDeclaringType,
            TranslationSettings settings)
        {
            var translationSize = 0;
            var formattingSize = 0;

            _writeModifiers = !method.IsInterfaceMember();

            if (_writeModifiers)
            {
                _accessibility = method.GetAccessibilityForTranslation();
                _modifiers = method.GetModifiersForTranslation();

                translationSize += _accessibility.Length + _modifiers.Length;
                formattingSize += settings.GetKeywordFormattingSize();
            }

            _returnTypeTranslation =
                new TypeNameTranslation(method.ReturnType, settings);

            _methodName = method.Name;

            translationSize +=
                _returnTypeTranslation.TranslationSize +
                _methodName.Length;

            formattingSize +=
                _returnTypeTranslation.FormattingSize;

            if (includeDeclaringType && method.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(method.DeclaringType, settings);

                translationSize += _declaringTypeNameTranslation.TranslationSize;
                formattingSize += _declaringTypeNameTranslation.FormattingSize;
            }

            if (method.IsGenericMethod)
            {
                var genericArguments = method.GetGenericArguments();

                _genericParametersTranslation =
                    new GenericParameterSetDefinitionTranslation(genericArguments, settings);

                _genericParameterConstraintsTranslation =
                    new GenericParameterSetConstraintsTranslation(genericArguments, settings);

                translationSize +=
                    _genericParametersTranslation.TranslationSize +
                    _genericParameterConstraintsTranslation.TranslationSize;

                formattingSize +=
                    _genericParametersTranslation.FormattingSize +
                    _genericParameterConstraintsTranslation.FormattingSize;
            }

            _parametersTranslation = parametersTranslation;

            TranslationSize = translationSize + _parametersTranslation.TranslationSize;
            FormattingSize = formattingSize + _parametersTranslation.FormattingSize;
        }

        /// <summary>
        /// Creates an <see cref="ITranslatable"/> for the given <paramref name="method"/>, handling
        /// properties and operators as well as regular methods. Includes the declaring type name in
        /// the translation.
        /// </summary>
        /// <param name="method">The MethodInfo for which to create the <see cref="ITranslatable"/>.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>An <see cref="ITranslatable"/> for the given <paramref name="method"/>.</returns>
        public static ITranslatable For(MethodInfo method, TranslationSettings settings)
        {
            if (method.IsAccessor(out var property))
            {
                return new PropertyDefinitionTranslation(property, method, settings);
            }

            if (method.IsImplicitOperator())
            {
                return new OperatorDefinitionTranslation(method, "implicit", settings);
            }

            if (method.IsExplicitOperator())
            {
                return new OperatorDefinitionTranslation(method, "explicit", settings);
            }

            return new MethodDefinitionTranslation(
                new ClrMethodWrapper(method, settings),
                settings);
        }

        /// <inheritdoc />
        public int TranslationSize { get; }

        /// <inheritdoc />
        public int FormattingSize { get; }

        /// <inheritdoc />
        public int GetIndentSize()
        {
            return _parametersTranslation.GetIndentSize() +
                   _genericParameterConstraintsTranslation?.GetIndentSize() ?? 0;
        }

        /// <inheritdoc />
        public int GetLineCount()
        {
            return _parametersTranslation.GetLineCount() +
                   _genericParameterConstraintsTranslation?.GetLineCount() ?? 0;
        }

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            if (_writeModifiers)
            {
                writer.WriteKeywordToTranslation(_accessibility + _modifiers);
            }

            _returnTypeTranslation.WriteTo(writer);
            writer.WriteSpaceToTranslation();

            if (_declaringTypeNameTranslation != null)
            {
                _declaringTypeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_methodName);

            _genericParametersTranslation?.WriteTo(writer);
            _parametersTranslation.WriteTo(writer);
            _genericParameterConstraintsTranslation?.WriteTo(writer);
        }
    }
}