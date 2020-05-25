namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Formatting;
    using Interfaces;

    internal static class TranslationExtensions
    {
        public static int GetLineCount<TTranslatable>(
            this IList<TTranslatable> translations,
            int translationsCount)
            where TTranslatable : ITranslatable
        {
            var lineCount = 1;

            for (var i = 0; ;)
            {
                var translationLineCount = translations[i].GetLineCount();

                if (translationLineCount > 1)
                {
                    lineCount += translationLineCount - 1;
                }

                ++i;

                if (i == translationsCount)
                {
                    return lineCount;
                }
            }
        }

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

        public static void WriteOpeningBraceToTranslation(this TranslationWriter writer, bool startOnNewLine = true)
        {
            if (startOnNewLine && writer.TranslationQuery(q => !q.TranslationEndsWith('{')))
            {
                writer.WriteNewLineToTranslation();
            }

            writer.WriteToTranslation('{');
            writer.WriteNewLineToTranslation();
            writer.Indent();
        }

        public static void WriteClosingBraceToTranslation(this TranslationWriter writer, bool startOnNewLine = true)
        {
            if (startOnNewLine)
            {
                writer.WriteNewLineToTranslation();
            }

            writer.Unindent();
            writer.WriteToTranslation('}');
        }

        public static void WriteInParentheses(this ITranslation translation, TranslationWriter writer)
        {
            writer.WriteToTranslation('(');
            translation.WriteTo(writer);
            writer.WriteToTranslation(')');
        }

        public static void WriteInParenthesesIfRequired(
            this ITranslation translation,
            TranslationWriter writer,
            ITranslationContext context)
        {
            if (ShouldWriteInParentheses(translation))
            {
                translation.WriteInParentheses(writer);
                return;
            }

            new CodeBlockTranslation(translation, context).WriteTo(writer);
        }

        public static bool ShouldWriteInParentheses(this ITranslation translation)
        {
            return (translation.NodeType == ExpressionType.Conditional) ||
                    translation.IsBinary() || translation.IsAssignment() ||
                    CastTranslation.IsCast(translation.NodeType);
        }

        public static void WriteNewToTranslation(this TranslationWriter writer)
            => writer.WriteKeywordToTranslation("new ");

        public static void WriteSpaceToTranslation(this TranslationWriter writer)
            => writer.WriteToTranslation(' ');

        public static void WriteDotToTranslation(this TranslationWriter writer)
            => writer.WriteToTranslation('.');

        public static void WriteReturnToTranslation(this TranslationWriter writer)
            => writer.WriteControlStatementToTranslation("return ");

        public static void WriteControlStatementToTranslation(this TranslationWriter writer, string statement)
            => writer.WriteToTranslation(statement, TokenType.ControlStatement);

        public static void WriteKeywordToTranslation(this TranslationWriter writer, string keyword)
            => writer.WriteToTranslation(keyword, TokenType.Keyword);

        public static void WriteTypeNameToTranslation(this TranslationWriter writer, string name)
            => writer.WriteToTranslation(name, TokenType.TypeName);
    }
}