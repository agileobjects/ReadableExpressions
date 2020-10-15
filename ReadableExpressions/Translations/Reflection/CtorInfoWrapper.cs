namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;

    internal class CtorInfoWrapper : BclMethodWrapperBase, IMethod
    {
        private readonly ConstructorInfo _ctorInfo;

        [DebuggerStepThrough]
        public CtorInfoWrapper(ConstructorInfo ctorInfo)
            : base(ctorInfo)
        {
            _ctorInfo = ctorInfo;
        }

        public bool IsExtensionMethod => false;

        public IMethod GetGenericMethodDefinition() => null;

        public Type ReturnType => _ctorInfo.DeclaringType;

        public override ReadOnlyCollection<IGenericArgument> GetGenericArguments()
            => Enumerable<IGenericArgument>.EmptyReadOnlyCollection;
    }
}