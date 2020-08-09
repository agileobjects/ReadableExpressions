namespace AgileObjects.ReadableExpressions.Build.SourceCode.Api
{
    using System;

    /// <summary>
    /// Provides configuration options to control aspects of source-code class generation.
    /// </summary>
    /// <typeparam name="TSettings">The settings type returned by this interface's methods.</typeparam>
    public interface IClassTranslationSettings<out TSettings> : IMethodTranslationSettings<TSettings>
    {
        /// <summary>
        /// Name generated classes using the given <paramref name="nameFactory"/>.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory from which to obtain the name of a generated class. The
        /// <see cref="IClassNamingContext"/> is supplied.
        /// </param>
        /// <returns>These settings, to support a fluent interface.</returns>
        TSettings NameClassesUsing(Func<IClassNamingContext, string> nameFactory);

        /// <summary>
        /// Name generated methods using the given <paramref name="nameFactory"/>.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory from which to obtain the name of a generated method. The parent
        /// <see cref="ClassExpression"/> and method <see cref="IMethodNamingContext"/> are
        /// supplied.
        /// </param>
        /// <returns>These settings, to support a fluent interface.</returns>
        TSettings NameMethodsUsing(Func<ClassExpression, IMethodNamingContext, string> nameFactory);
    }

    /// <summary>
    /// Provides configuration options to control aspects of source-code class generation.
    /// </summary>
    public interface IClassTranslationSettings : IClassTranslationSettings<IClassTranslationSettings>
    {
    }
}