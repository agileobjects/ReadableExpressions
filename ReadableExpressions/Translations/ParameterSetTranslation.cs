namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class ParameterSetTranslation : ITranslation
    {
        private const string _openAndCloseParentheses = "()";

        private readonly ITranslationContext _context;
        private readonly IList<ParameterTranslation> _parameterTranslations;

        public ParameterSetTranslation(ICollection<ParameterExpression> parameters, ITranslationContext context)
        {
            _context = context;

            if (parameters.Count == 0)
            {
                _parameterTranslations = Enumerable<ParameterTranslation>.EmptyArray;
                EstimatedSize = _openAndCloseParentheses.Length;
                return;
            }

            var estimatedSize = 0;

            _parameterTranslations = parameters
                .Project(p =>
                {
                    var translation = new ParameterTranslation(p, context);

                    estimatedSize += translation.EstimatedSize;

                    return translation;
                })
                .ToArray();

            ParameterCount = _parameterTranslations.Count;
            EstimatedSize = estimatedSize + (ParameterCount * 2) + 4;
        }

        public int EstimatedSize { get; }

        private int ParameterCount { get; }

        public void WriteToTranslation()
        {
            switch (ParameterCount)
            {
                case 0:
                    _context.WriteToTranslation(_openAndCloseParentheses);
                    return;
                
                case 1:
                    _parameterTranslations[0].WriteToTranslation();
                    return;
            }

            _context.WriteToTranslation('(');

            for (var i = 0; i < ParameterCount; ++i)
            {
                _parameterTranslations[i].WriteToTranslation();

                if (i < ParameterCount - 1)
                {
                    _context.WriteToTranslation(", ");
                }
            }

            _context.WriteToTranslation(')');
        }
    }
}