namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;

    internal class CtorInfoWrapper : BclMethodWrapperBase
    {
        private readonly ConstructorInfo _ctorInfo;
        private IType _declaringType;

        [DebuggerStepThrough]
        public CtorInfoWrapper(ConstructorInfo ctorInfo)
            : base(ctorInfo)
        {
            _ctorInfo = ctorInfo;
        }

        public override bool IsExtensionMethod => false;

        public override IMethod GetGenericMethodDefinition() => null;

        public override IType ReturnType
            => _declaringType ??= BclTypeWrapper.For(_ctorInfo.DeclaringType);

        public override ReadOnlyCollection<IGenericParameter> GetGenericArguments()
            => Enumerable<IGenericParameter>.EmptyReadOnlyCollection;
    }
}