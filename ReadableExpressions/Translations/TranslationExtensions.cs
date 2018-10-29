namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class TranslationExtensions
    {
        public static bool IsMultiStatement(this ITranslatable translation)
        {
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

        public static void WriteOpeningBraceToTranslation(this ITranslationContext context, bool startOnNewLine = true)
        {
            if (startOnNewLine && context.TranslationQuery(q => !q.TranslationEndsWith('{')))
            {
                context.WriteNewLineToTranslation();
            }

            context.WriteToTranslation('{');
            context.WriteNewLineToTranslation();
            context.Indent();
        }

        public static void WriteClosingBraceToTranslation(this ITranslationContext context, bool startOnNewLine = true)
        {
            if (startOnNewLine)
            {
                context.WriteNewLineToTranslation();
            }

            context.Unindent();
            context.WriteToTranslation('}');
        }

        public static void WriteInParentheses(this ITranslation translation, ITranslationContext context)
        {
            context.WriteToTranslation('(');
            translation.WriteTo(context);
            context.WriteToTranslation(')');
        }

        public static void WriteInParenthesesIfRequired(this ITranslation translation, ITranslationContext context)
        {
            if ((translation.NodeType == ExpressionType.Conditional) ||
                 BinaryTranslation.IsBinary(translation.NodeType) ||
                 AssignmentTranslation.IsAssignment(translation.NodeType))
            {
                translation.WriteInParentheses(context);
                return;
            }

            new CodeBlockTranslation(translation).WriteTo(context);
        }

        public static void WriteSpaceToTranslation(this ITranslationContext context)
        {
            context.WriteToTranslation(' ');
        }
    }
}