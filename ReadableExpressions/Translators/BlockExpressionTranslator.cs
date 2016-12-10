namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Formatting;

    internal class BlockExpressionTranslator : ExpressionTranslatorBase
    {
        public BlockExpressionTranslator()
            : base(ExpressionType.Block)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var block = (BlockExpression)expression;

            var variables = GetVariableDeclarations(block, context);
            var lines = GetBlockLines(block, context);

            lines = ProcessBlockContents(lines.ToArray(), block);

            var blockContents = variables.Concat(lines);

            return string.Join(Environment.NewLine, blockContents);
        }

        private static IEnumerable<string> GetVariableDeclarations(
            BlockExpression block,
            TranslationContext context)
        {
            return block
                .Variables
                .Except(context.JoinedAssignmentVariables)
                .GroupBy(v => v.Type)
                .Select(vGrp => $"{vGrp.Key.GetFriendlyName()} {string.Join(", ", vGrp)};");
        }

        private static IEnumerable<string> GetBlockLines(BlockExpression block, TranslationContext context)
        {
            return block
                .Expressions
                .Where(exp => (exp == block.Result) || Include(exp))
                .Select(exp => new
                {
                    Expression = exp,
                    Translation = GetTerminatedStatementOrNull(exp, context)
                })
                .Where(d => d.Translation != null)
                .Select(d => d.Translation);
        }

        private static bool Include(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Parameter)
            {
                return false;
            }

            return (expression.NodeType != ExpressionType.Constant) || expression.IsComment();
        }

        private static string GetTerminatedStatementOrNull(Expression expression, TranslationContext context)
        {
            var translation = context.Translate(expression);

            if (string.IsNullOrEmpty(translation))
            {
                return null;
            }

            if (StatementIsTerminated(translation, expression))
            {
                return translation;
            }

            translation += ";";

            if (context.IsNotJoinedAssignment(expression))
            {
                return translation;
            }

            var typeName = GetVariableTypeName((BinaryExpression)expression);

            return typeName + " " + translation;
        }

        private static bool StatementIsTerminated(string translation, Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Block:
                case ExpressionType.Lambda:
                    return true;

                case ExpressionType.Assign:
                    return false;
            }

            return translation.IsTerminated() || expression.IsComment();
        }

        private static string GetVariableTypeName(BinaryExpression assignment)
        {
            return UseFullTypeName(assignment) ? assignment.Left.Type.GetFriendlyName() : "var";
        }

        private static bool UseFullTypeName(BinaryExpression assignment)
        {
            if ((assignment.Left.Type != assignment.Right.Type) ||
                (assignment.Right.NodeType == ExpressionType.Lambda))
            {
                return true;
            }

            if (assignment.Right.NodeType != ExpressionType.Conditional)
            {
                return false;
            }

            var conditional = (ConditionalExpression)assignment.Right;

            return conditional.IfTrue.Type != conditional.IfFalse.Type;
        }

        private static IEnumerable<string> ProcessBlockContents(IList<string> lines, BlockExpression block)
        {
            var finalLineIndex = lines.Count - 1;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (i != finalLineIndex)
                {
                    yield return line;

                    if (LeaveBlankLineAfter(line, lines[i + 1]))
                    {
                        yield return string.Empty;
                    }

                    continue;
                }

                if (DoNotAddReturnStatement(block, lines))
                {
                    yield return line;
                    yield break;
                }

                if (CodeBlock.IsSingleStatement(line.SplitToLines()))
                {
                    yield return "return " + line;
                    yield break;
                }

                yield return CodeBlock.InsertReturnKeyword(line);
            }
        }

        private static bool LeaveBlankLineAfter(string line, string nextLine)
        {
            return (line.EndsWith('}') || IsMultiLineStatement(line)) &&
                !(string.IsNullOrEmpty(nextLine) || nextLine.StartsWithNewLine());
        }

        private static bool IsMultiLineStatement(string line)
        {
            if (!line.Contains(Environment.NewLine))
            {
                return false;
            }

            return line
                .SplitToLines(StringSplitOptions.RemoveEmptyEntries)
                .Any(l => !l.IsTerminated());
        }

        private static bool DoNotAddReturnStatement(BlockExpression block, ICollection<string> lines)
        {
            return (lines.Count <= 1) || !block.IsReturnable();
        }
    }
}