namespace AgileObjects.ReadableExpressions.Translations
{
    using System;

    /// <summary>
    /// A <see cref="UnaryOperatorTranslationBase"/> for the typeof operator.
    /// </summary>
    public class TypeOfOperatorTranslation : UnaryOperatorTranslationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOfOperatorTranslation"/> class.
        /// </summary>
        /// <param name="operandTranslation">
        /// The <see cref="ITranslatable"/> which will write the symbol to which the typeof operator
        /// is being applied.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public TypeOfOperatorTranslation(ITranslatable operandTranslation, TranslationSettings settings)
            : base("typeof", operandTranslation, settings)
        {
        }

        /// <summary>
        /// Gets the type of this <see cref="TypeOfOperatorTranslation"/>, which is 'Type'.
        /// </summary>
        public override Type Type => typeof(Type);
    }
}