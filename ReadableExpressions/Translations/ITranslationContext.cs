namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ITranslationContext
    {
        TranslationSettings Settings { get; }

        IEnumerable<ParameterExpression> JoinedAssignmentVariables { get; }

        bool IsNotJoinedAssignment(Expression expression);

        bool IsReferencedByGoto(LabelTarget labelTarget);

        bool GoesToReturnLabel(GotoExpression @goto);

        bool IsPartOfMethodCallChain(Expression methodCall);

        int? GetUnnamedVariableNumber(ParameterExpression variable);

        TypeNameTranslation GetTranslationFor(Type type);

        ITranslation GetTranslationFor(Expression expression);

        CodeBlockTranslation GetCodeBlockTranslationFor(Expression expression);

        bool TranslationQuery(Func<ITranslationQuery, bool> predicate);

        void Indent();

        void Unindent();

        void WriteNewLineToTranslation();

        void WriteToTranslation(char character);

        void WriteToTranslation(string stringValue);

        void WriteToTranslation(int intValue);

        void WriteToTranslation(object value);
    }
}