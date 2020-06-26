namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Provides configuration options to control aspects of source-code method generation.
    /// </summary>
    /// <typeparam name="TSettings">The settings type returned by this interface's methods.</typeparam>
    public interface IMethodTranslationSettings<out TSettings> : ITranslationSettings
    {
        /// <summary>
        /// Name generated methods using the given <paramref name="nameFactory"/>.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory from which to obtain the name of a generated method. The method body
        /// LambdaExpression is supplied.
        /// </param>
        /// <returns>These settings, to support a fluent interface.</returns>
        TSettings NameMethodsUsing(Func<LambdaExpression, string> nameFactory);
    }

    /// <summary>
    /// Provides configuration options to control aspects of source-code method generation.
    /// </summary>
    public interface IMethodTranslationSettings : IMethodTranslationSettings<IMethodTranslationSettings>
    {
    }
}