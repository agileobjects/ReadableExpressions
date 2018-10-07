namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Linq;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using NetStandardPolyfills;

    internal class NewingTranslation : ITranslation
    {
        private readonly bool _isAnonymousType;
        private readonly ParameterSetTranslation _parameters;
        private readonly ParameterInfo[] _ctorParameters;
        private readonly ITranslation _typeNameTranslation;
        private bool _omitParenthesesIfParameterless;

        public NewingTranslation(NewExpression newing, ITranslationContext context)
        {
            _isAnonymousType = newing.Type.IsAnonymous();
            _parameters = new ParameterSetTranslation(newing.Arguments, context);

            if (_isAnonymousType)
            {
                _ctorParameters = newing.Constructor.GetParameters();
                EstimatedSize = GetAnonymousTypeEstimatedSize();
                return;
            }

            _typeNameTranslation = context.GetTranslationFor(newing.Type);
            EstimatedSize = GetEstimatedSize();
        }

        private int GetAnonymousTypeEstimatedSize()
        {
            return _ctorParameters.Sum(p => p.Name.Length) +
                   (3 * _ctorParameters.Length) + // <- for ' = '
                   _parameters.EstimatedSize +
                   "new {  }".Length;
        }

        private int GetEstimatedSize()
        {
            return _typeNameTranslation.EstimatedSize +
                   _parameters.EstimatedSize +
                   "new ()".Length;
        }

        public ExpressionType NodeType => ExpressionType.New;

        public int EstimatedSize { get; }

        public NewingTranslation WithoutParenthesesIfParameterless()
        {
            _omitParenthesesIfParameterless = true;
            return this;
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_isAnonymousType)
            {
                WriteAnonymousTypeNewingTo(context);
                return;
            }

            context.WriteToTranslation("new ");
            _typeNameTranslation.WriteTo(context);

            if (_omitParenthesesIfParameterless && _parameters.None)
            {
                return;
            }

            _parameters.WithParentheses().WriteTo(context);
        }

        private void WriteAnonymousTypeNewingTo(ITranslationContext context)
        {
            context.WriteToTranslation("new { ");

            for (var i = 0; ; ++i)
            {
                context.WriteToTranslation(_ctorParameters[i].Name);
                context.WriteToTranslation(" = ");
                _parameters[i].WriteTo(context);

                if (i == (_ctorParameters.Length - 1))
                {
                    break;
                }

                context.WriteToTranslation(", ");
            }

            context.WriteToTranslation(" }");
        }
    }
}