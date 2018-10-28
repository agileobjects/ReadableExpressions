﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class TypeNameTranslation : ITranslation
    {
        private const string _object = "object";

        private readonly bool _isObject;
        private readonly string _typeName;

        public TypeNameTranslation(Type type, ITranslationContext context)
        {
            _isObject = type == typeof(object);

            if (_isObject)
            {
                EstimatedSize = _object.Length;
                return;
            }

            _typeName = type.GetFriendlyName(context.Settings);
            EstimatedSize = (int)(_typeName.Length * 1.1);
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_isObject)
            {
                context.WriteToTranslation(_object);
                return;
            }

            context.WriteToTranslation(_typeName);
        }
    }
}