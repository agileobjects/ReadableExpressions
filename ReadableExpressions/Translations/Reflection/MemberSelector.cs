namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;

    /// <summary>
    /// Provides options for configuring the selection of a set of <see cref="IMember"/>s.
    /// </summary>
    public class MemberSelector
    {
        private bool _includeFields;
        private bool _includeProperties;
        private bool _includeMethods;
        private bool _includePublic;
        private bool _includeNonPublic;
        private bool _includeInstance;
        private bool _includeStatic;
        private string _name;

        internal MemberSelector(Action<MemberSelector> configuration)
        {
            configuration.Invoke(this);
        }

        /// <summary>
        /// Include fields in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Fields()
        {
            _includeFields = true;
            return this;
        }

        /// <summary>
        /// Include properties in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Properties()
        {
            _includeProperties = true;
            return this;
        }

        /// <summary>
        /// Include methods in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Methods()
        {
            _includeMethods = true;
            return this;
        }

        /// <summary>
        /// Include public members in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Public()
        {
            _includePublic = true;
            return this;
        }

        /// <summary>
        /// Include non-public members in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector NonPublic()
        {
            _includeNonPublic = true;
            return this;
        }

        /// <summary>
        /// Include instance members in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Instance()
        {
            _includeInstance = true;
            return this;
        }

        /// <summary>
        /// Include static members in the member selection.
        /// </summary>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Static()
        {
            _includeStatic = true;
            return this;
        }

        /// <summary>
        /// Include only members with the given <paramref name="name"/> in the member selection.
        /// </summary>
        /// <param name="name">The name of the <see cref="IMember"/>s to include.</param>
        /// <returns>This <see cref="MemberSelector"/>, to support a fluent API.</returns>
        public MemberSelector Named(string name)
        {
            _name = name;
            return this;
        }

        internal bool Include(IMember member)
        {
            if (_name != null && member.Name != _name)
            {
                return false;
            }

            if (FilterByType(member))
            {
                return false;
            }

            if (FilterByVisibility(member))
            {
                return false;
            }

            if (FilterByScope(member))
            {
                return false;
            }

            return true;
        }

        private bool FilterByType(IMember member)
        {
            if (_includeFields || _includeProperties || _includeMethods)
            {
                if (!_includeMethods && member is IMethod)
                {
                    return true;
                }

                if (!_includeProperties && member is IProperty)
                {
                    return true;
                }

                if (!_includeFields && member is IField)
                {
                    return true;
                }
            }

            return false;
        }

        private bool FilterByVisibility(IMember member)
        {
            if (_includePublic || _includeNonPublic)
            {
                if (!_includePublic && member.IsPublic)
                {
                    return true;
                }

                if (!_includeNonPublic && !member.IsPublic)
                {
                    return true;
                }
            }

            return false;
        }

        private bool FilterByScope(IMember member)
        {
            if (_includeInstance || _includeStatic)
            {
                if (!_includeInstance && !member.IsStatic)
                {
                    return true;
                }

                if (!_includeStatic && member.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }
    }
}