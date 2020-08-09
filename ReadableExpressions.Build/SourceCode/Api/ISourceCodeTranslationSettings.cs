namespace AgileObjects.ReadableExpressions.Build.SourceCode.Api
{
    using System;

    /// <summary>
    /// Provides configuration options to control aspects of source-code generation.
    /// </summary>
    public interface ISourceCodeTranslationSettings : 
        IClassTranslationSettings<ISourceCodeTranslationSettings>
    {
        /// <summary>
        /// Add generated classes to the namespace of the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type the namespace of which the generated code should use.</typeparam>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings WithNamespaceOf<T>();

        /// <summary>
        /// Add generated classes to the given <paramref name="namespace"/>.
        /// </summary>
        /// <param name="namespace">The namespace in which to create generated classes.</param>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings WithNamespace(string @namespace);

        /// <summary>
        /// Name generated classes using the given <paramref name="nameFactory"/>.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory from which to obtain the name of a generated class. The parent
        /// <see cref="SourceCodeExpression"/> and a <see cref="IClassNamingContext"/> are supplied.
        /// </param>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings NameClassesUsing(
            Func<SourceCodeExpression, IClassNamingContext, string> nameFactory);

        /// <summary>
        /// Name generated methods using the given <paramref name="nameFactory"/>.
        /// </summary>
        /// <param name="nameFactory">
        /// The factory from which to obtain the name of a generated method. The parent
        /// <see cref="SourceCodeExpression"/>, <see cref="ClassExpression"/> and a
        /// <see cref="IMethodNamingContext"/> are supplied.
        /// </param>
        /// <returns>These <see cref="ISourceCodeTranslationSettings"/>, to support a fluent interface.</returns>
        ISourceCodeTranslationSettings NameMethodsUsing(
            Func<SourceCodeExpression, ClassExpression, IMethodNamingContext, string> nameFactory);

        /// <summary>
        /// Generate source code for a single class only. BlockExpressions directly containing
        /// multiple LambdaExpressions will generate a single class with one method per lambda
        /// instead of the default of multiple, single-method classes.
        /// </summary>
        ISourceCodeTranslationSettings CreateSingleClass { get; }
    }
}