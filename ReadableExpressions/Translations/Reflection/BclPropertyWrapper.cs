namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Reflection;
    using NetStandardPolyfills;

    /// <summary>
    /// Helper base class for <see cref="IMember"/> implementations which wrap MemberInfo-derived
    /// objects.
    /// </summary>
    public class BclPropertyWrapper : IProperty
    {
        private readonly MemberInfo _property;
        private readonly IMethod _getter;
        private readonly IMethod _setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclPropertyWrapper"/> class.
        /// </summary>
        /// <param name="property">The PropertyInfo to which the <see cref="BclPropertyWrapper"/> relates.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        public BclPropertyWrapper(PropertyInfo property, TranslationSettings settings)
        {
            _property = property;

            var getter = property.GetGetter(nonPublic: true);
            var setter = property.GetSetter(nonPublic: true);

            if (getter != null)
            {
                _getter = new BclMethodWrapper(getter, settings);
            }

            if (setter != null)
            {
                _setter = new BclMethodWrapper(setter, settings);
            }
        }

        /// <inheritdoc />
        public Type DeclaringType => _property.DeclaringType;

        /// <inheritdoc />
        public bool IsStatic
            => Getter?.IsStatic == true || Setter?.IsStatic == true;

        /// <inheritdoc />
        public bool IsPublic => IsReadable || IsWritable;

        /// <inheritdoc />
        public bool IsInternal
        {
            get
            {
                if (IsPublic)
                {
                    return false;
                }

                return Getter?.IsInternal == true || Setter?.IsInternal == true;
            }
        }

        /// <inheritdoc />
        public bool IsProtectedInternal
        {
            get
            {
                if (IsPublic || IsInternal)
                {
                    return false;
                }

                return Getter?.IsProtectedInternal == true ||
                       Setter?.IsProtectedInternal == true;
            }
        }

        /// <inheritdoc />
        public bool IsProtected
        {
            get
            {
                if (IsPublic || IsInternal || IsProtectedInternal)
                {
                    return false;
                }

                return Getter?.IsProtected == true || Setter?.IsProtected == true;
            }
        }

        /// <inheritdoc />
        public bool IsPrivateProtected
        {
            get
            {
                if (IsPublic || IsInternal || IsProtectedInternal || IsProtected)
                {
                    return false;
                }

                return Getter?.IsPrivateProtected == true ||
                       Setter?.IsPrivateProtected == true;
            }
        }

        /// <inheritdoc />
        public bool IsPrivate
        {
            get
            {
                if (IsPublic || IsInternal || IsProtectedInternal || IsProtected ||
                    IsPrivateProtected)
                {
                    return false;
                }

                return Getter?.IsPrivate == true || Setter?.IsPrivate == true;
            }
        }

        /// <inheritdoc />
        public string Name => _property.Name;

        /// <inheritdoc />
        public Type Type => Getter?.Type ?? Setter.Type;

        /// <inheritdoc />
        public bool IsAbstract => _getter?.IsAbstract ?? _setter.IsAbstract;

        /// <inheritdoc />
        public bool IsVirtual
            => !IsAbstract && (_getter?.IsVirtual ?? _setter.IsVirtual);

        /// <inheritdoc />
        public bool IsOverride => _getter?.IsOverride ?? _setter.IsOverride;

        /// <inheritdoc />
        public bool IsReadable => Getter?.IsPublic == true;

        /// <inheritdoc />
        public IComplexMember Getter => _getter;

        /// <inheritdoc />
        public bool IsWritable => Setter?.IsPublic == true;

        /// <inheritdoc />
        public IComplexMember Setter => _setter;
    }
}