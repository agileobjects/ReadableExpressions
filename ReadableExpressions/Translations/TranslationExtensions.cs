namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Formatting;
    using Interfaces;

    internal static class TranslationExtensions
    {
        public static int GetKeywordFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.Keyword);

        public static int GetControlStatementFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.ControlStatement);

        public static int GetTypeNameFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.TypeName);

        public static int GetVariableFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.Variable);

        public static int GetNumericFormattingSize(this ITranslationContext context)
            => context.GetFormattingSize(TokenType.Numeric);

        public static int GetFormattingSize(this ITranslationContext context, TokenType tokenType)
            => context.Settings.Formatter.GetFormattingSize(tokenType);

        public static bool IsMultiStatement(this ITranslation translation)
        {
            switch (translation.NodeType)
            {
                case ExpressionType.Call:
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                    return false;
            }

            return (translation is IPotentialMultiStatementTranslatable multiStatementTranslatable) &&
                    multiStatementTranslatable.IsMultiStatement;
        }

        public static bool IsTerminated(this ITranslatable translation)
        {
            return (translation is IPotentialSelfTerminatingTranslatable selfTerminatedTranslatable) &&
                   selfTerminatedTranslatable.IsTerminated;
        }

        public static bool HasGoto(this ITranslatable translation)
        {
            return (translation is IPotentialGotoTranslatable gotoTranslatable) &&
                   gotoTranslatable.HasGoto;
        }

        public static bool ExceedsLengthThreshold(this ITranslatable translatable)
            => translatable.TranslationSize > 100;

        public static bool IsAssignment(this ITranslation translation)
            => AssignmentTranslation.IsAssignment(translation.NodeType);

        public static bool IsBinary(this ITranslation translation)
            => BinaryTranslation.IsBinary(translation.NodeType);

        public static ITranslation WithParentheses(this ITranslation translation)
            => new WrappedTranslation("(", translation, ")");

        public static ITranslation WithNodeType(this ITranslation translation, ExpressionType nodeType)
            => new ModifiedTranslation(translation, nodeType);

        public static ITranslation WithTypes(this ITranslatable translatable, ExpressionType nodeType, Type type)
            => new ModifiedTranslation(translatable, nodeType, type);

        public static void WriteOpeningBraceToTranslation(this TranslationBuffer buffer, bool startOnNewLine = true)
        {
            if (startOnNewLine && buffer.TranslationQuery(q => !q.TranslationEndsWith('{')))
            {
                buffer.WriteNewLineToTranslation();
            }

            buffer.WriteToTranslation('{');
            buffer.WriteNewLineToTranslation();
            buffer.Indent();
        }

        public static void WriteClosingBraceToTranslation(this TranslationBuffer buffer, bool startOnNewLine = true)
        {
            if (startOnNewLine)
            {
                buffer.WriteNewLineToTranslation();
            }

            buffer.Unindent();
            buffer.WriteToTranslation('}');
        }

        public static void WriteInParentheses(this ITranslation translation, TranslationBuffer buffer)
        {
            buffer.WriteToTranslation('(');
            translation.WriteTo(buffer);
            buffer.WriteToTranslation(')');
        }

        public static void WriteInParenthesesIfRequired(
            this ITranslation translation, 
            TranslationBuffer buffer,
            ITranslationContext context)
        {
            if (ShouldWriteInParentheses(translation))
            {
                translation.WriteInParentheses(buffer);
                return;
            }

            new CodeBlockTranslation(translation, context).WriteTo(buffer);
        }

        public static bool ShouldWriteInParentheses(this ITranslation translation)
        {
            return (translation.NodeType == ExpressionType.Conditional) ||
                    translation.IsBinary() || translation.IsAssignment() ||
                    CastTranslation.IsCast(translation.NodeType);
        }

        public static void WriteNewToTranslation(this TranslationBuffer buffer)
            => buffer.WriteKeywordToTranslation("new ");

        public static void WriteSpaceToTranslation(this TranslationBuffer buffer)
            => buffer.WriteToTranslation(' ');

        public static void WriteDotToTranslation(this TranslationBuffer buffer)
            => buffer.WriteToTranslation('.');

        public static void WriteReturnToTranslation(this TranslationBuffer buffer)
            => buffer.WriteControlStatementToTranslation("return ");

        public static void WriteControlStatementToTranslation(this TranslationBuffer buffer, string statement)
            => buffer.WriteToTranslation(statement, TokenType.ControlStatement);

        public static void WriteKeywordToTranslation(this TranslationBuffer buffer, string keyword)
            => buffer.WriteToTranslation(keyword, TokenType.Keyword);

        public static void WriteTypeNameToTranslation(this TranslationBuffer buffer, string name)
            => buffer.WriteToTranslation(name, TokenType.TypeName);
    }
}