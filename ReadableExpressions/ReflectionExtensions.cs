namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Reflection;
    using Translations;
    using Translations.Reflection;

    /// <summary>
    /// Provides reflection translation extension methods.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Translates this <paramref name="type"/> into a readable string.
        /// </summary>
        /// <param name="type">The Type to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="type"/>.</returns>
        public static string ToReadableString(
            this Type type,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            if (type == null)
            {
                return "[Type not found]";
            }

            var settings = configuration.GetTranslationSettings();
            var translation = new TypeDefinitionTranslation(type, settings);
            var writer = new TranslationWriter(settings, translation);

            return writer.GetContent();
        }

        /// <summary>
        /// Translates this <paramref name="ctor"/> into a readable string.
        /// </summary>
        /// <param name="ctor">The ConstructorInfo to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="ctor"/>.</returns>
        public static string ToReadableString(
            this ConstructorInfo ctor,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            if (ctor == null)
            {
                return "[Constructor not found]";
            }

            var settings = configuration.GetTranslationSettings();
            var translation = new ConstructorDefinitionTranslation(ctor, settings);
            var writer = new TranslationWriter(settings, translation);

            return writer.GetContent();
        }

        /// <summary>
        /// Translates this <paramref name="method"/> into a readable string.
        /// </summary>
        /// <param name="method">The MethodInfo to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="method"/>.</returns>
        public static string ToReadableString(
            this MethodInfo method,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            if (method == null)
            {
                return "[Method not found]";
            }

            var settings = configuration.GetTranslationSettings();
            var translation = MethodDefinitionTranslation.For(method, settings);
            var writer = new TranslationWriter(settings, translation);

            return writer.GetContent();
        }

        /// <summary>
        /// Translates this <paramref name="property"/> into a readable string.
        /// </summary>
        /// <param name="property">The PropertyInfo to translate.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A readable string version of this <paramref name="property"/>.</returns>
        public static string ToReadableString(
            this PropertyInfo property,
            Func<ITranslationSettings, ITranslationSettings> configuration = null)
        {
            if (property == null)
            {
                return "[Property not found]";
            }

            var settings = configuration.GetTranslationSettings();
            var translation = new PropertyDefinitionTranslation(property, settings);
            var writer = new TranslationWriter(settings, translation);

            return writer.GetContent();
        }
    }
}
