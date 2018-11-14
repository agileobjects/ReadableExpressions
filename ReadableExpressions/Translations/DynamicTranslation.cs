namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Text.RegularExpressions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal static class DynamicTranslation
    {
        public static ITranslation For(DynamicExpression dynamicExpression, ITranslationContext context)
        {
            var args = new DynamicTranslationArgs(dynamicExpression, context);

            if (DynamicMemberAccessTranslation.TryGetTranslation(args, out var translation))
            {
                return translation;
            }

            if (DynamicMemberWriteTranslation.TryGetTranslation(args, out translation))
            {
                return translation;
            }

            return new FixedValueTranslation(ExpressionType.Dynamic, args.OperationDescription);
        }

        private class DynamicTranslationArgs
        {
            private readonly DynamicExpression _dynamicExpression;

            public DynamicTranslationArgs(DynamicExpression dynamicExpression, ITranslationContext context)
            {
                _dynamicExpression = dynamicExpression;
                OperationDescription = dynamicExpression.ToString();
                Context = context;
            }

            public string OperationDescription { get; }

            public ITranslationContext Context { get; }

            public Expression FirstArgument => _dynamicExpression.Arguments.First();

            public Expression LastArgument => _dynamicExpression.Arguments.Last();

            public bool IsMatch(Regex matcher, out Match match)
            {
                match = matcher.Match(OperationDescription);

                return match.Success;
            }
        }

        private static class DynamicMemberAccessTranslation
        {
            private static readonly Regex _matcher = new Regex(@"^GetMember (?<MemberName>[^\(]+)\(");

            public static bool TryGetTranslation(DynamicTranslationArgs args, out ITranslation translation)
            {
                if (!args.IsMatch(_matcher, out var match))
                {
                    translation = null;
                    return false;
                }

                translation = GetTranslation(args, match);
                return true;
            }

            public static ITranslation GetTranslation(DynamicTranslationArgs args, Match match)
            {
                var subject = args.Context.GetTranslationFor(args.FirstArgument);
                var memberName = match.Groups["MemberName"].Value;

                return new MemberAccessTranslation(subject, memberName);
            }
        }

        private static class DynamicMemberWriteTranslation
        {
            private static readonly Regex _matcher = new Regex(@"^SetMember (?<MemberName>[^\(]+)\(");

            public static bool TryGetTranslation(DynamicTranslationArgs args, out ITranslation translation)
            {
                if (!args.IsMatch(_matcher, out var match))
                {
                    translation = null;
                    return false;
                }

                var targetTranslation = DynamicMemberAccessTranslation.GetTranslation(args, match);

                translation = new AssignmentTranslation(
                    ExpressionType.Assign,
                    targetTranslation,
                    args.LastArgument,
                    args.Context);

                return true;
            }
        }
    }
}