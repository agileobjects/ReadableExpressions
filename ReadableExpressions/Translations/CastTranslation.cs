namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;
    using Reflection;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static class CastTranslation
    {
        public static ITranslation For(UnaryExpression cast, ITranslationContext context)
        {
            var castValueTranslation = context.GetTranslationFor(cast.Operand);

            switch (cast.NodeType)
            {
                case ExpressionType.Convert:
                case ConvertChecked:
                    if (cast.Type == typeof(object) && cast.Operand.Type.IsValueType())
                    {
                        // Don't bother to show boxing:
                        return castValueTranslation;
                    }

                    if (cast.Method != null)
                    {
                        var isImplicitOperator = cast.Method.IsImplicitOperator();

                        if (isImplicitOperator)
                        {
                            return castValueTranslation.ShouldWriteInParentheses()
                                ? castValueTranslation.WithParentheses()
                                : castValueTranslation;
                        }

                        if (cast.Method.IsExplicitOperator())
                        {
                            break;
                        }

                        return MethodCallTranslation.ForCustomMethodCast(
                            context.GetTranslationFor(cast.Type),
                            new ClrMethodWrapper(cast.Method, context),
                            castValueTranslation,
                            context);
                    }

                    if (IsDelegateCast(cast, out var createDelegateCall))
                    {
                        return MethodGroupTranslation.ForCreateDelegateCall(cast.NodeType, createDelegateCall, context);
                    }

                    break;

                case TypeAs:
                    return new TypeTestedTranslation(TypeAs, castValueTranslation, " as ", cast.Type, context);
            }

            return new StandardCastTranslation(cast, castValueTranslation, context);
        }

        private static bool IsDelegateCast(UnaryExpression cast, out MethodCallExpression createDelegateCall)
        {
            if ((cast.Operand.NodeType == Call) &&
                (cast.Operand.Type == typeof(Delegate)) &&
               ((createDelegateCall = ((MethodCallExpression)cast.Operand)).Method.Name == "CreateDelegate"))
            {
                return true;
            }

            createDelegateCall = null;
            return false;
        }

        public static ITranslation For(TypeBinaryExpression typeIs, ITranslationContext context)
        {
            return new TypeTestedTranslation(
                TypeIs,
                context.GetTranslationFor(typeIs.Expression),
                " is ",
                typeIs.TypeOperand,
                context);
        }

        public static ITranslation ForExplicitOperator(
            ITranslation castValueTranslation,
            ITranslation castTypeNameTranslation)
        {
            return new StandardCastTranslation(
                Call,
                castTypeNameTranslation,
                castValueTranslation);
        }

        public static bool IsCast(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Convert:
                case ConvertChecked:
                case TypeAs:
                case TypeIs:
                case Unbox:
                    return true;
            }

            return false;
        }

        private class TypeTestedTranslation : ITranslation
        {
            private readonly ITranslation _testedValueTranslation;
            private readonly string _test;
            private readonly ITranslation _testedTypeNameTranslation;

            public TypeTestedTranslation(
                ExpressionType nodeType,
                ITranslation testedValueTranslation,
                string test,
                Type testedType,
                ITranslationContext context)
            {
                NodeType = nodeType;
                _testedValueTranslation = testedValueTranslation;
                _test = test;
                _testedTypeNameTranslation = context.GetTranslationFor(testedType);

                TranslationSize =
                    _testedValueTranslation.TranslationSize +
                    _test.Length +
                    _testedTypeNameTranslation.TranslationSize;

                FormattingSize =
                    _testedValueTranslation.FormattingSize +
                    context.GetKeywordFormattingSize() +
                    _testedTypeNameTranslation.FormattingSize;
            }

            public ExpressionType NodeType { get; }

            public Type Type => _testedTypeNameTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize()
            {
                return _testedValueTranslation.GetIndentSize() +
                       _testedTypeNameTranslation.GetIndentSize();
            }

            public int GetLineCount()
            {
                return Math.Max(
                    _testedValueTranslation.GetLineCount(),
                    _testedTypeNameTranslation.GetLineCount());
            }

            public void WriteTo(TranslationWriter writer)
            {
                _testedValueTranslation.WriteTo(writer);
                writer.WriteKeywordToTranslation(_test);
                _testedTypeNameTranslation.WriteTo(writer);
            }
        }

        private class StandardCastTranslation : ITranslation
        {
            private readonly ITranslation _castValueTranslation;
            private readonly ITranslation _castTypeNameTranslation;

            public StandardCastTranslation(
                Expression cast,
                ITranslation castValueTranslation,
                ITranslationContext context)
                : this(
                    cast.NodeType,
                    context.GetTranslationFor(cast.Type),
                    castValueTranslation)
            {
            }

            public StandardCastTranslation(
                ExpressionType nodeType,
                ITranslation castTypeNameTranslation,
                ITranslation castValueTranslation)
            {
                NodeType = nodeType;
                _castTypeNameTranslation = castTypeNameTranslation;
                _castValueTranslation = castValueTranslation;

                if (_castValueTranslation.ShouldWriteInParentheses())
                {
                    _castValueTranslation = _castValueTranslation.WithParentheses();
                }

                TranslationSize = 
                    _castTypeNameTranslation.TranslationSize + 
                    2 + // <- for ()
                    _castValueTranslation.TranslationSize;
                
                FormattingSize = 
                    _castTypeNameTranslation.FormattingSize + 
                    _castValueTranslation.FormattingSize;
            }

            public ExpressionType NodeType { get; }

            public Type Type => _castTypeNameTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize()
            {
                return _castTypeNameTranslation.GetIndentSize() +
                       _castValueTranslation.GetIndentSize();
            }

            public int GetLineCount()
            {
                return Math.Max(
                    _castTypeNameTranslation.GetLineCount(),
                    _castValueTranslation.GetLineCount());
            }

            public void WriteTo(TranslationWriter writer)
            {
                _castTypeNameTranslation.WriteInParentheses(writer);
                _castValueTranslation.WriteTo(writer);
            }
        }
    }
}