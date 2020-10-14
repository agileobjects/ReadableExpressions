namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Formatting;

    /// <summary>
    /// Provides a factory method which creates an appropriate <see cref="ITranslation"/> for an
    /// <see cref="IGenericArgument"/>.
    /// </summary>
    public static class GenericArgumentTranslation
    {
        /// <summary>
        /// Creates an <see cref="ITranslation"/> for the given <paramref name="argument"/>, using
        /// the given <paramref name="settings"/>.
        /// </summary>
        /// <param name="argument">
        /// The <see cref="IGenericArgument"/> for which to return an <see cref="ITranslation"/>.
        /// </param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>An <see cref="ITranslation"/> for the given <paramref name="argument"/></returns>
        public static ITranslation For(
            IGenericArgument argument,
            TranslationSettings settings)
        {
            return argument.IsClosed
                ? (ITranslation)new TypeNameTranslation(argument.Type, settings)
                : new FixedValueTranslation(
                    ExpressionType.Constant,
                    argument.TypeName,
                    typeof(Type),
                    TokenType.TypeName,
                    settings);
        }
    }
}