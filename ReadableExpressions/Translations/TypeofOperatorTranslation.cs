namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;
    using Reflection;

    /// <summary>
    /// A <see cref="UnaryOperatorTranslationBase"/> for the typeof operator.
    /// </summary>
    public class TypeOfOperatorTranslation : UnaryOperatorTranslationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOfOperatorTranslation"/> class.
        /// </summary>
        /// <param name="type">The Type to which the typeof operator is being applied.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> describing the Expression translation.
        /// </param>
        public TypeOfOperatorTranslation(Type type, ITranslationContext context)
            : this(BclTypeWrapper.For(type), context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOfOperatorTranslation"/> class.
        /// </summary>
        /// <param name="type">The <see cref="IType"/> to which the typeof operator is being applied.</param>
        /// <param name="context">
        /// The <see cref="ITranslationContext"/> describing the Expression translation.
        /// </param>
        public TypeOfOperatorTranslation(IType type, ITranslationContext context)
            : base("typeof", context.GetTranslationFor(type), context.Settings)
        {
        }

        /// <summary>
        /// Gets the type of this <see cref="TypeOfOperatorTranslation"/>, which is 'Type'.
        /// </summary>
        public override Type Type => typeof(Type);
    }
}