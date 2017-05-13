namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Formatting;

    internal class BlockExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly ParameterExpressionTranslator _variableNameTranslator;

        public BlockExpressionTranslator(ParameterExpressionTranslator variableNameTranslator)
            : base(ExpressionType.Block)
        {
            _variableNameTranslator = variableNameTranslator;
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var block = (BlockExpression)expression;

            var variables = GetVariableDeclarations(block, context);
            var statements = GetBlockStatements(block, context).ToArray();
            var separator = GetStatementsSeparator(variables, statements);

            var blockContents = variables.Concat(separator).Concat(statements);

            return string.Join(Environment.NewLine, blockContents);
        }

        private IList<string> GetVariableDeclarations(
            BlockExpression block,
            TranslationContext context)
        {
            return block
                .Variables
                .Except(context.JoinedAssignmentVariables)
                .GroupBy(v => v.Type)
                .Select(vGrp => new
                {
                    TypeName = vGrp.Key.GetFriendlyName(),
                    VariableNames = vGrp.Select(varName => _variableNameTranslator.Translate(varName))
                })
                .Select(varData => $"{varData.TypeName} {string.Join(", ", varData.VariableNames)};")
                .ToArray();
        }

        private static IEnumerable<string> GetStatementsSeparator(
            ICollection<string> variables,
            IList<string> statements)
        {
            if ((variables.Count > 0) && LeaveBlankLineBefore(statements[0]))
            {
                yield return string.Empty;
            }
        }

        private static IEnumerable<string> GetBlockStatements(
            BlockExpression block,
            TranslationContext context)
        {
            var lines = GetBlockLines(block, context);

            var finalLineIndex = lines.Count - 1;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var isNotFirstLine = (i > 0);
                var previousLine = isNotFirstLine ? lines[i - 1] : null;

                if (isNotFirstLine && LeaveBlankLineBefore(line, previousLine))
                {
                    yield return string.Empty;
                }

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

                if (isNotFirstLine && LeaveBlankLineBeforeReturn(line, previousLine))
                {
                    yield return string.Empty;
                }

                if (CodeBlock.IsSingleStatement(line.SplitToLines()))
                {
                    yield return "return " + line;
                    yield break;
                }

                yield return CodeBlock.InsertReturnKeyword(line);
            }
        }

        private static IList<string> GetBlockLines(BlockExpression block, TranslationContext context)
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
                .Select(d => d.Translation)
                .ToArray();
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

        private static bool LeaveBlankLineBefore(string line, string previousLine = null)
        {
            if ((previousLine != null) && LeaveBlankLineBefore(previousLine))
            {
                return false;
            }

            return line.StartsWith("if (", StringComparison.Ordinal) ||
                line.StartsWith("switch ", StringComparison.Ordinal);
        }

        private static bool LeaveBlankLineAfter(string line, string nextLine)
        {
            return (line.EndsWith('}') || IsMultiLineStatement(line)) &&
                !(string.IsNullOrEmpty(nextLine) || nextLine.StartsWithNewLine());
        }

        private static bool IsMultiLineStatement(string line)
        {
            return line.IsMultiLine() && line
                .SplitToLines(StringSplitOptions.RemoveEmptyEntries)
                .Any(l => !l.IsTerminated());
        }

        private static bool DoNotAddReturnStatement(BlockExpression block, ICollection<string> lines)
        {
            return (lines.Count <= 1) || !block.IsReturnable();
        }

        private static bool LeaveBlankLineBeforeReturn(string line, string previousLine)
        {
            if (previousLine.IsComment() || LeaveBlankLineAfter(previousLine, line))
            {
                return false;
            }

            return true;
        }
    }
}