namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class DefaultValueTranslation : ITranslation, IPotentialEmptyTranslatable
    {
        private const string _default = "default";
        private const string _null = "null";
        private readonly bool _typeCanBeNull;
        private readonly ITranslation _typeNameTranslation;

        public DefaultValueTranslation(
            Expression defaultExpression,
            ITranslationContext context,
            bool allowNullKeyword = true)
        {
            Type = defaultExpression.Type;
            
            if (Type == typeof(void))
            {
                IsEmpty = true;
                return;
            }

            if (allowNullKeyword)
            {
                _typeCanBeNull = Type.CanBeNull();

                if (_typeCanBeNull)
                {
                    EstimatedSize = _null.Length;
                    return;
                }
            }

            _typeNameTranslation = context.GetTranslationFor(Type);
            EstimatedSize = _default.Length + _typeNameTranslation.EstimatedSize + 2;
        }

        public ExpressionType NodeType => ExpressionType.Default;

        public Type Type { get; }

        public int EstimatedSize { get; }

        public bool IsEmpty { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_typeCanBeNull)
            {
                context.WriteToTranslation(_null);
            }

            if (_typeNameTranslation == null)
            {
                // Translation of default(void):
                return;
            }

            context.WriteToTranslation(_default);
            _typeNameTranslation.WriteInParentheses(context);
        }
    }
}