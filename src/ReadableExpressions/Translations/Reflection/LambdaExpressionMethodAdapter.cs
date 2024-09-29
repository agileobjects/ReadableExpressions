namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using System.Collections.ObjectModel;
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
            .ProjectToArray(IParameter (p) => new ClrParameterWrapper(p));

        return new(delegateParameters);
    }

    #endregion

    public bool IsExtensionMethod => false;

    public ReadOnlyCollection<IParameter> GetParameters() => _parameters;
}
