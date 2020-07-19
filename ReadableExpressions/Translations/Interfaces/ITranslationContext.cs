namespace AgileObjects.ReadableExpressions.Translations.Interfaces
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ReadableExpressions.SourceCode;

    internal interface ITranslationContext
    {
        /// <summary>
        /// Gets the <see cref="TranslationSettings"/> to use for translation in this context.
        /// </summary>
        TranslationSettings Settings { get; }

        /// <summary>
        /// Gets the namespaces required by the translated Expression.
        /// </summary>
        IList<string> RequiredNamespaces { get; }

        /// <summary>
        /// Gets the variables in the translated Expression which are first used as an output
        /// parameter argument.
        /// </summary>
        ICollection<ParameterExpression> InlineOutputVariables { get; }

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="parameter"/> is an output
        /// parameter that should be declared inline.
        /// </summary>
        /// <param name="parameter">The parameter for which to make the determination.</param>
        /// <returns>
        /// True if the given <paramref name="parameter"/> is an output parameter that should be
        /// declared inline, otherwise false.
        /// </returns>
        bool ShouldBeDeclaredInline(ParameterExpression parameter);

        /// <summary>
        /// Gets the variables in the translated Expression which should be declared in the same
        /// statement in which they are assigned.
        /// </summary>
        ICollection<ParameterExpression> JoinedAssignmentVariables { get; }

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="expression"/> represents an
        /// assignment where the assigned variable is declared as part of the assignment statement.
        /// </summary>
        /// <param name="expression">The Expression to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="expression"/> represents an assignment where the assigned
        /// variable is declared as part of the assignment statement, otherwise false.
        /// </returns>
        bool IsJoinedAssignment(Expression expression);

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="expression"/> is the Exception
        /// variable in a Catch block.
        /// </summary>
        /// <param name="expression">The expression for which to make the determination.</param>
        /// <returns>
        /// True if the given <paramref name="expression"/> is the Exception variable in a Catch block,
        /// otherwise false.
        /// </returns>
        bool IsCatchBlockVariable(Expression expression);

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="labelTarget"/> is referenced by a
        /// <see cref="GotoExpression"/>.
        /// </summary>
        /// <param name="labelTarget">The <see cref="LabelTarget"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="labelTarget"/> is referenced by a <see cref="GotoExpression"/>,
        /// otherwise false.
        /// </returns>
        bool IsReferencedByGoto(LabelTarget labelTarget);

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="goto"/> goes to the 
        /// final statement in a block, and so should be rendered as a return statement.
        /// </summary>
        /// <param name="goto">The GotoExpression for which to make the determination.</param>
        /// <returns>
        /// True if the given <paramref name="goto"/> goes to the final statement in a block,
        /// otherwise false.
        /// </returns>
        bool GoesToReturnLabel(GotoExpression @goto);

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="methodCall"/> is part of a chain
        /// of multiple method calls.
        /// </summary>
        /// <param name="methodCall">The Expression to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="methodCall"/> is part of a chain of multiple method calls,
        /// otherwise false.
        /// </returns>
        bool IsPartOfMethodCallChain(MethodCallExpression methodCall);

        /// <summary>
        /// Gets the 1-based index of the given <paramref name="variable"/> in the set of unnamed,
        /// accessed variables of its Type.
        /// </summary>
        /// <param name="variable">The variable for which to get the 1-based index.</param>
        /// <returns>
        /// The 1-based index of the given <paramref name="variable"/>, or null if only variable of
        /// this Type is declared.
        /// </returns>
        int? GetUnnamedVariableNumberOrNull(ParameterExpression variable);

        IList<ParameterExpression> GetUnscopedVariablesFor(MethodExpression method);

        ITranslation GetTranslationFor(Expression expression);
    }
}