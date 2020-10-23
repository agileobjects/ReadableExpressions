namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using static MethodTranslationHelpers;

    /// <summary>
    /// An <see cref="ITranslatable"/> for a method signature, including accessibility, scope, generic
    /// arguments and constraints, and method arguments.
    /// </summary>
    public class MethodDefinitionTranslation : ITranslatable
    {
        private readonly TranslationSettings _settings;
        private readonly string _accessibility;
        private readonly string _modifiers;
        private readonly TypeNameTranslation _returnTypeTranslation;
        private readonly TypeNameTranslation _declaringTypeNameTranslation;
        private readonly string _methodName;
        private readonly ITranslatable[] _genericArgumentTranslations;
        private readonly ITranslatable[] _constraintTranslations;
        private readonly int _genericArgumentCount;
        private readonly int _constraintsCount;
        private readonly ITranslatable _parametersTranslation;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinitionTranslation"/> class for
        /// the given <paramref name="method"/>.
        /// </summary>
        /// <param name="method">
        /// The <see cref="IMethod"/> for which to create the <see cref="MethodDefinitionTranslation"/>.
        /// </param>
        /// <param name="includeDeclaringType">
        /// Whether to include the name of the <paramref name="method"/>'s declaring type in the
        /// <see cref="MethodDefinitionTranslation"/>.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public MethodDefinitionTranslation(
            IMethod method,
            bool includeDeclaringType,
            TranslationSettings settings)
        {
            _settings = settings;
            _accessibility = GetAccessibility(method);
            _modifiers = GetModifiers(method);

            _returnTypeTranslation =
                new TypeNameTranslation(method.ReturnType, settings);

            if (includeDeclaringType && method.DeclaringType != null)
            {
                _declaringTypeNameTranslation =
                    new TypeNameTranslation(method.DeclaringType, settings);
            }

            _methodName = method.Name;

            var translationSize =
                _accessibility.Length +
                _modifiers.Length +
                _returnTypeTranslation.TranslationSize +
                _methodName.Length;

            var keywordFormattingSize = settings.GetKeywordFormattingSize();

            var formattingSize =
                 keywordFormattingSize + // <- For modifiers
                _returnTypeTranslation.FormattingSize;

            if (method.IsGenericMethod)
            {
                translationSize += 2;

                var genericArguments = method.GetGenericArguments();
                _genericArgumentCount = genericArguments.Count;

                _genericArgumentTranslations = new ITranslatable[_genericArgumentCount];
                _constraintTranslations = new ITranslatable[_genericArgumentCount];

                for (var i = 0; ;)
                {
                    var argument = genericArguments[i];

                    var argumentTranslation = new TypeNameTranslation(argument.Type, settings);
                    var constraintsTranslation = GenericConstraintsTranslation.For(argument, settings);

                    translationSize +=
                        argumentTranslation.TranslationSize +
                        constraintsTranslation.TranslationSize;

                    formattingSize +=
                        argumentTranslation.FormattingSize +
                        constraintsTranslation.FormattingSize;

                    _genericArgumentTranslations[i] = argumentTranslation;
                    _constraintTranslations[i] = constraintsTranslation;

                    if (constraintsTranslation.TranslationSize > 0)
                    {
                        ++_constraintsCount;
                    }

                    ++i;

                    if (i == _genericArgumentCount)
                    {
                        break;
                    }

                    translationSize += ", ".Length;
                }
            }
            else
            {
                _genericArgumentTranslations = Enumerable<ITranslatable>.EmptyArray;
                _constraintTranslations = Enumerable<ITranslatable>.EmptyArray;
            }

            _parametersTranslation = new ParameterSetDefinitionTranslation(method, settings);

            if (_declaringTypeNameTranslation != null)
            {
                translationSize += _declaringTypeNameTranslation.TranslationSize + 1;
                formattingSize += _declaringTypeNameTranslation.FormattingSize;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
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
            if (method.IsPropertyGetterOrSetterCall(out var property))
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
                new BclMethodWrapper(method, settings),
                includeDeclaringType: true,
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
                   _constraintsCount * _settings.Indent.Length;
        }

        /// <inheritdoc />
        public int GetLineCount()
        {
            return _parametersTranslation.GetLineCount() +
                   _constraintsCount + 1;
        }

        /// <inheritdoc />
        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);

            _returnTypeTranslation.WriteTo(writer);
            writer.WriteSpaceToTranslation();

            if (_declaringTypeNameTranslation != null)
            {
                _declaringTypeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
            }

            writer.WriteToTranslation(_methodName);

            if (_genericArgumentCount != 0)
            {
                writer.WriteToTranslation('<');

                for (var i = 0; ;)
                {
                    _genericArgumentTranslations[i].WriteTo(writer);

                    ++i;

                    if (i == _genericArgumentCount)
                    {
                        break;
                    }

                    writer.WriteToTranslation(", ");
                }

                writer.WriteToTranslation('>');
            }

            _parametersTranslation.WriteTo(writer);

            if (_constraintsCount == 0)
            {
                return;
            }

            writer.WriteNewLineToTranslation();
            writer.Indent();

            for (var i = 0; ;)
            {
                _constraintTranslations[i].WriteTo(writer);
                ++i;

                if (i == _constraintsCount)
                {
                    break;
                }

                writer.WriteNewLineToTranslation();
            }

            writer.Unindent();
        }
    }
}