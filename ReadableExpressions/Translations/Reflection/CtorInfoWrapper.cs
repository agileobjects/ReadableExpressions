namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;

    internal class CtorInfoWrapper : BclMethodWrapperBase
    {
        private readonly ConstructorInfo _ctorInfo;

        [DebuggerStepThrough]
        public CtorInfoWrapper(ConstructorInfo ctorInfo)
            : base(ctorInfo)
        {
            _ctorInfo = ctorInfo;
        }

        public override bool IsExtensionMethod => false;

        public override IMethod GetGenericMethodDefinition() => null;

        public override Type ReturnType => _ctorInfo.DeclaringType;

        public override ReadOnlyCollection<IGenericArgument> GetGenericArguments()
            => Enumerable<IGenericArgument>.EmptyReadOnlyCollection;
    }
}