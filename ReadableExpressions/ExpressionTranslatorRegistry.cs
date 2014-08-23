﻿namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Translators;

    internal partial class ExpressionTranslatorRegistry : IExpressionTranslatorRegistry
    {
        private Dictionary<ExpressionType, IExpressionTranslator> _translatorsByType;

        private Dictionary<ExpressionType, IExpressionTranslator> TranslatorsByType
        {
            get
            {
                return _translatorsByType ?? (_translatorsByType = _translators
                    .SelectMany(t => t.NodeTypes.Select(nt => new
                    {
                        NodeType = nt,
                        Translator = t
                    }))
                    .ToDictionary(t => t.NodeType, t => t.Translator));
            }
        }

        public string Translate(Expression expression)
        {
            return TranslatorsByType[expression.NodeType].Translate(expression, this);
        }
    }
}