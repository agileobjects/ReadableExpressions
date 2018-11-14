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

            return new FixedValueTranslation(ExpressionType.Dynamic, args.OperationDescription);
        }

        private class DynamicTranslationArgs
        {
            public DynamicTranslationArgs(DynamicExpression dynamicExpression, ITranslationContext context)
            {
                DynamicExpression = dynamicExpression;
                OperationDescription = dynamicExpression.ToString();
                Context = context;
            }

            public DynamicExpression DynamicExpression { get; }

            public string OperationDescription { get; }

            public ITranslationContext Context { get; }

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

                var subject = args.Context.GetTranslationFor(args.DynamicExpression.Arguments.First());
                var memberName = match.Groups["MemberName"].Value;

                translation = new MemberAccessTranslation(subject, memberName);
                return true;
            }
        }
    }
}