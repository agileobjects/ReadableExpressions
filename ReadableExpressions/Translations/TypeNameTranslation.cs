﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class TypeNameTranslation : ITranslation
    {
        private const string _object = "object";
        private readonly bool _isObject;
        private readonly string _typeName;
        private bool _writeObjectTypeName;

        public TypeNameTranslation(Type type, ITranslationContext context)
        {
            Type = type;
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
        
        public Type Type { get; }

        public int EstimatedSize { get; }

        public TypeNameTranslation WithObjectTypeName()
        {
            if (_isObject)
            {
                _writeObjectTypeName = true;
            }

            return this;
        }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_isObject)
            {
                buffer.WriteToTranslation(_writeObjectTypeName ? "Object" : _object);
                return;
            }

            buffer.WriteToTranslation(_typeName);
        }
    }
}