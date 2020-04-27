namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal static class TranslationExtensions
    {
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
            => translatable.EstimatedSize > 100;

        public static bool IsAssignment(this ITranslation translation)
            => AssignmentTranslation.IsAssignment(translation.NodeType);

        public static bool IsBinary(this ITranslation translation)
            => BinaryTranslation.IsBinary(translation.NodeType);

        public static TranslationWrapper WithParentheses(this ITranslation translation)
            => new TranslationWrapper(translation).WrappedWith("(", ")");

        public static TranslationWrapper WithNodeType(this ITranslation translation, ExpressionType nodeType)
            => new TranslationWrapper(nodeType, translation, translation.Type);

        public static TranslationWrapper WithTypes(this ITranslatable translatable, ExpressionType nodeType, Type type)
            => new TranslationWrapper(nodeType, translatable, type);

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

        public static void WriteInParenthesesIfRequired(this ITranslation translation, TranslationBuffer buffer)
        {
            if (ShouldWriteInParentheses(translation))
            {
                translation.WriteInParentheses(buffer);
                return;
            }

            new CodeBlockTranslation(translation).WriteTo(buffer);
        }

        public static bool ShouldWriteInParentheses(this ITranslation translation)
        {
            return (translation.NodeType == ExpressionType.Conditional) ||
                    translation.IsBinary() || translation.IsAssignment() ||
                    CastTranslation.IsCast(translation.NodeType);
        }

        public static void WriteNewToTranslation(this TranslationBuffer buffer)
            => buffer.WriteToTranslation("new ", TokenType.Keyword);

        public static void WriteSpaceToTranslation(this TranslationBuffer buffer)
            => buffer.WriteToTranslation(' ');

        public static void WriteDotToTranslation(this TranslationBuffer buffer)
            => buffer.WriteToTranslation('.');
    }
}