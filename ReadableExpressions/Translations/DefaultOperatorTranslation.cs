namespace AgileObjects.ReadableExpressions.Translations
{
    using System;

    /// <summary>
    /// A <see cref="UnaryOperatorTranslationBase"/> for the default operator.
    /// </summary>
    public sealed class DefaultOperatorTranslation : UnaryOperatorTranslationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryOperatorTranslationBase"/> class.
        /// </summary>
        /// <param name="operandTranslation">
        /// The <see cref="ITranslation"/> which will write the symbol to which the default operator
        /// is being applied.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public DefaultOperatorTranslation(
            ITranslation operandTranslation,
            TranslationSettings settings)
            : base("default", operandTranslation, settings)
        {
            Type = operandTranslation.Type;
        }

        /// <summary>
        /// Gets the type of this <see cref="DefaultOperatorTranslation"/>, which is its operand's
        /// type.
        /// </summary>
        public override Type Type { get; }
    }
}