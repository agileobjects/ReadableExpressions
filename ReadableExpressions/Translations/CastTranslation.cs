﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Interfaces;
    using NetStandardPolyfills;
    using Translators;

    internal static class CastTranslation
    {
        public static ITranslation For(UnaryExpression cast, ITranslationContext context)
        {
            var castValueTranslation = context.GetTranslationFor(cast.Operand);

            switch (cast.NodeType)
            {
                case ExpressionType.Convert:
                case ConvertChecked:
                    if (cast.Type == typeof(object))
                    {
                        // Don't bother to show a boxing cast:
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
                            new BclMethodWrapper(cast.Method),
                            castValueTranslation,
                            context);
                    }

                    if (IsDelegateCast(cast, out var createDelegateCall))
                    {
                        return new CreateDelegateCallTranslation(cast.NodeType, createDelegateCall, context);
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

        private class CreateDelegateCallTranslation : ITranslation
        {
            private readonly ITranslation _methodSubjectTranslation;
            private readonly string _subjectMethodName;

            public CreateDelegateCallTranslation(
                ExpressionType nodeType,
                MethodCallExpression createDelegateCall,
                ITranslationContext context)
            {
                NodeType = nodeType;
#if NET35
                var subjectMethod = (MethodInfo)((ConstantExpression)createDelegateCall.Arguments.Last()).Value;
#else
                // ReSharper disable once PossibleNullReferenceException
                var subjectMethod = (MethodInfo)((ConstantExpression)createDelegateCall.Object).Value;
#endif

                _methodSubjectTranslation = subjectMethod.IsStatic
                    ? context.GetTranslationFor(subjectMethod.DeclaringType)
                    : context.GetTranslationFor(createDelegateCall.Arguments.ElementAtOrDefault(1));

                _subjectMethodName = subjectMethod.Name;
                EstimatedSize = _methodSubjectTranslation.EstimatedSize + ".".Length + _subjectMethodName.Length;
            }

            public ExpressionType NodeType { get; }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _methodSubjectTranslation.WriteTo(context);
                context.WriteToTranslation('.');
                context.WriteToTranslation(_subjectMethodName);
            }
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
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                return _testedValueTranslation.EstimatedSize +
                       _test.Length +
                       _testedTypeNameTranslation.EstimatedSize;
            }

            public ExpressionType NodeType { get; }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _testedValueTranslation.WriteTo(context);
                context.WriteToTranslation(_test);
                _testedTypeNameTranslation.WriteTo(context);
            }
        }

        private class StandardCastTranslation : ITranslation
        {
            private readonly ITranslation _castValueTranslation;
            private readonly ITranslation _castTypeNameTranslation;

            public StandardCastTranslation(Expression cast, ITranslation castValueTranslation, ITranslationContext context)
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

                EstimatedSize = _castTypeNameTranslation.EstimatedSize + _castValueTranslation.EstimatedSize;
            }

            public ExpressionType NodeType { get; }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _castTypeNameTranslation.WriteInParentheses(context);
                _castValueTranslation.WriteTo(context);
            }
        }
    }
}