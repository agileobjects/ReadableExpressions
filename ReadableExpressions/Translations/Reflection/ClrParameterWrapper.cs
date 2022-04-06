namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Reflection;
    using NetStandardPolyfills;

    internal class ClrParameterWrapper : IParameter
    {
        private readonly ParameterInfo _parameter;
        private IType _type;

        public ClrParameterWrapper(ParameterInfo parameter)
        {
            _parameter = parameter;
        }

        public IType Type
            => _type ??= ClrTypeWrapper.For(ParameterType);

        private Type ParameterType => _parameter.ParameterType;

        public string Name => _parameter.Name;

        public bool IsOut => _parameter.IsOut;

        public bool IsRef => !IsOut && ParameterType.IsByRef;

        public bool IsParamsArray => _parameter.IsParamsArray();
    }
}