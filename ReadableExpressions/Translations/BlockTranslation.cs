namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using Microsoft.Scripting.Ast;
#endif

    internal class BlockTranslation : ITranslation
    {
        private readonly IDictionary<Type, ParameterSetTranslation> _variables;

        public BlockTranslation(BlockExpression block, ITranslationContext context)
        {
            _variables = GetVariableDeclarations(block, context);
        }

        private static IDictionary<Type, ParameterSetTranslation> GetVariableDeclarations(
            BlockExpression block,
            ITranslationContext context)
        {
            return block
                .Variables
                .Except(context.JoinedAssignmentVariables)
                .GroupBy(v => v.Type)
                .ToDictionary(
                    grp => grp.Key,
                    grp => new ParameterSetTranslation(grp, context).WithoutParentheses());
        }

        public ExpressionType NodeType => ExpressionType.Block;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
        }
    }
}