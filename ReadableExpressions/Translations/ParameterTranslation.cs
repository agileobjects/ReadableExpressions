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

    internal class ParameterTranslation : ITranslation
    {
        // TODO: array-specific Combine overload
        private static readonly IEnumerable<string> _keywords = InternalTypeExtensions
            .TypeNames
            .Combine(new[]
            {
                "typeof",
                "default",
                "void",
                "readonly",
                "do",
                "while",
                "switch",
                "if",
                "else",
                "try",
                "catch",
                "finally",
                "throw",
                "for",
                "foreach",
                "goto",
                "return",
                "implicit",
                "explicit"
            })
            .ToArray();

        private readonly ParameterExpression _parameter;
        private readonly ITranslationContext _context;
        private readonly bool _isUnnamedParameter;

        public ParameterTranslation(ParameterExpression parameter, ITranslationContext context)
        {
            _parameter = parameter;
            _context = context;
            _isUnnamedParameter = parameter.Name.IsNullOrWhiteSpace();
            context.Allocate(EstimatedSize = GetEstimatedSize());
        }

        private int GetEstimatedSize()
        {
            if (_isUnnamedParameter)
            {
                return (int)(_parameter.Type.Name.Length * 1.2);
            }

            return _keywords.Contains(_parameter.Name)
                ? _parameter.Name.Length + 1
                : _parameter.Name.Length;
        }

        public int EstimatedSize { get; }

        public void Translate()
        {
            //var parameterName = _parameter.Name;

            //if (_isUnnamedParameter)
            //{
            //    var variableNumber = _context.GetUnnamedVariableNumber(_parameter);

            //    parameterName = _parameter.Type.GetVariableNameInCamelCase(_context.Settings) + variableNumber;
            //}

            //_keywords.Contains(parameterName) ? "@" + parameterName : parameterName;
        }
    }
}