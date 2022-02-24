namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.ObjectModel;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;

    internal class LambdaExpressionMethodAdapter : IMethodBase
    {
        private readonly ReadOnlyCollection<IParameter> _parameters;

        public LambdaExpressionMethodAdapter(LambdaExpression lambda)
        {
            _parameters = GetParameters(lambda);
        }

        #region Setup

        private static ReadOnlyCollection<IParameter> GetParameters(
            LambdaExpression lambda)
        {
            var parameters = lambda.Parameters;

            if (parameters.Count == 0)
            {
                return Enumerable<IParameter>.EmptyReadOnlyCollection;
            }

            var delegateParameters = lambda.Type
                .GetPublicInstanceMethod("Invoke")
                .GetParameters()
                .ProjectToArray(p => (IParameter)new ClrParameterWrapper(p));

            return new ReadOnlyCollection<IParameter>(delegateParameters);
        }

        #endregion

        public bool IsExtensionMethod => false;

        public ReadOnlyCollection<IParameter> GetParameters() => _parameters;
    }
}
