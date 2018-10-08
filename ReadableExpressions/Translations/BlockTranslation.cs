namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class BlockTranslation : ITranslation
    {
        private readonly IDictionary<ITranslation, ParameterSetTranslation> _variables;
        private readonly IList<ITranslation> _statements;

        public BlockTranslation(BlockExpression block, ITranslationContext context)
        {
            _variables = GetVariableDeclarations(block, context);
            _statements = context.GetTranslationsFor(block.Expressions);
            EstimatedSize = GetEstimatedSize();
        }

        private static IDictionary<ITranslation, ParameterSetTranslation> GetVariableDeclarations(
            BlockExpression block,
            ITranslationContext context)
        {
            var variablesByType = block
                .Variables
                .Except(context.JoinedAssignmentVariables)
                .GroupBy(v => v.Type)
                .ToArray();

            if (variablesByType.Length == 0)
            {
                return EmptyDictionary<ITranslation, ParameterSetTranslation>.Instance;
            }

            return variablesByType.ToDictionary(
                grp => context.GetTranslationFor(grp.Key),
                grp => new ParameterSetTranslation(grp, context).WithoutParentheses());
        }

        private int GetEstimatedSize()
        {
            var estimatedSize = 0;

            if (_variables.Count != 0)
            {
                foreach (var parametersByType in _variables)
                {
                    estimatedSize += parametersByType.Key.EstimatedSize;
                    estimatedSize += parametersByType.Value.EstimatedSize;
                }
            }

            for (int i = 0, l = _statements.Count; i < l; i++)
            {
                estimatedSize += _statements[i].EstimatedSize;
            }

            return estimatedSize;
        }

        public ExpressionType NodeType => ExpressionType.Block;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_variables.Count != 0)
            {
                foreach (var parametersByType in _variables)
                {
                    parametersByType.Key.WriteTo(context);
                    context.WriteToTranslation(' ');
                    parametersByType.Value.WriteTo(context);
                    context.WriteToTranslation(';');
                }
            }

            context.WriteNewLineToTranslation();

            for (int i = 0, l = _statements.Count - 1; ; ++i)
            {
                _statements[i].WriteTo(context);
                context.WriteToTranslation(';');

                if (i == l)
                {
                    break;
                }

                context.WriteNewLineToTranslation();
            }
        }
    }
}