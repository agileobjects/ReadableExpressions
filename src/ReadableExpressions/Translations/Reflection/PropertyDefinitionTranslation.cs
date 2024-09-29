namespace AgileObjects.ReadableExpressions.Translations.Reflection;

using System.Collections.Generic;
using System.Reflection;
using Extensions;
#if NETSTANDARD1_0
using NetStandardPolyfills;
#endif

/// <summary>
/// An <see cref="ITranslation"/> for a property signature, including accessibility and scope.
/// </summary>
public class PropertyDefinitionTranslation : INodeTranslation
{
    private readonly bool _writeModifiers;
    private readonly string _accessibility;
    private readonly string _modifiers;
    private readonly ITranslation _declaringTypeNameTranslation;
    private readonly ITranslation _propertyTypeNameTranslation;
    private readonly string _propertyName;
    private readonly ITranslation[] _accessorTranslations;

    internal PropertyDefinitionTranslation(
        PropertyInfo property,
        TranslationSettings settings) :
        this(property, property.GetAccessors(nonPublic: true), settings)
    {
    }

    internal PropertyDefinitionTranslation(
        PropertyInfo property,
        MethodInfo accessor,
        TranslationSettings settings) :
        this(property, [accessor], settings)
    {
    }

    private PropertyDefinitionTranslation(
        PropertyInfo property,
        IList<MethodInfo> accessors,
        TranslationSettings settings) :
        this(
            new ClrPropertyWrapper(property, settings),
            accessors.ProjectToArray<MethodInfo, IMethod>(acc =>
                new ClrMethodWrapper(acc, settings)),
            includeDeclaringType: true,
            settings)
    {
    }

    private PropertyDefinitionTranslation(
        IProperty property,
        IList<IMethod> accessors,
        bool includeDeclaringType,
        TranslationSettings settings) :
        this(
            property,
            accessors,
            includeDeclaringType,
           (pd, acc, _) => new PropertyAccessorDefinitionTranslation(pd, acc),
            settings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDefinitionTranslation"/> class.
    /// </summary>
    /// <param name="property">The <see cref="IProperty"/> to translate.</param>
    /// <param name="accessors">
    /// One or two <see cref="IMethod"/>s describing one or both of the
    /// <paramref name="property"/>'s accessor(s). 
    /// </param>
    /// <param name="includeDeclaringType">
    /// A value indicating whether to include the <paramref name="property"/>'s declaring type
    /// in the translation.
    /// </param>
    /// <param name="accessorTranslationFactory">
    /// A <see cref="PropertyAccessorTranslationFactory"/> with which to translate the
    /// <paramref name="property"/>'s accessor(s).
    /// </param>
    /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
    protected PropertyDefinitionTranslation(
        IProperty property,
        IList<IMethod> accessors,
        bool includeDeclaringType,
        PropertyAccessorTranslationFactory accessorTranslationFactory,
        TranslationSettings settings)
    {
        var translationLength = 0;

        _writeModifiers = !property.IsInterfaceMember();

        if (_writeModifiers)
        {
            _accessibility = property.GetAccessibilityForTranslation();
            _modifiers = property.GetModifiersForTranslation();

            translationLength += _accessibility.Length + _modifiers.Length;
        }

        _propertyTypeNameTranslation =
            new TypeNameTranslation(property.Type, settings);

        _propertyName = property.Name;

        translationLength +=
            _propertyTypeNameTranslation.TranslationLength +
            _propertyName.Length;

        if (includeDeclaringType && property.DeclaringType != null)
        {
            _declaringTypeNameTranslation =
                new TypeNameTranslation(property.DeclaringType, settings);

            translationLength += _declaringTypeNameTranslation.TranslationLength + ".".Length;
        }

        _accessorTranslations = new ITranslation[accessors.Count];

        for (var i = 0; i < accessors.Count; ++i)
        {
            var accessorTranslation =
                accessorTranslationFactory.Invoke(this, accessors[i], settings);

            translationLength += accessorTranslation.TranslationLength;

            _accessorTranslations[i] = accessorTranslation;
        }

        TranslationLength = translationLength;
    }

    /// <inheritdoc />
    public ExpressionType NodeType => ExpressionType.MemberAccess;

    /// <inheritdoc />
    public int TranslationLength { get; }

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
        WritePropertyStartTo(writer);

        if (_writeModifiers)
        {
            writer.WriteKeywordToTranslation(_accessibility + _modifiers);
        }

        _propertyTypeNameTranslation.WriteTo(writer);
        writer.WriteSpaceToTranslation();

        if (_declaringTypeNameTranslation != null)
        {
            _declaringTypeNameTranslation.WriteTo(writer);
            writer.WriteDotToTranslation();
        }

        writer.WriteToTranslation(_propertyName);
        WriteAccessorsStartTo(writer);

        foreach (var accessorTranslation in _accessorTranslations)
        {
            accessorTranslation.WriteTo(writer);
        }

        WriteAccessorsEndTo(writer);
    }

    /// <summary>
    /// Write characters to the given <paramref name="writer"/> to begin translation of the
    /// property.
    /// </summary>
    /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the characters.</param>
    protected virtual void WritePropertyStartTo(TranslationWriter writer)
    {
    }

    /// <summary>
    /// Write characters to the given <paramref name="writer"/> to begin translation of the
    /// property accessors.
    /// </summary>
    /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the characters.</param>
    protected virtual void WriteAccessorsStartTo(TranslationWriter writer)
        => writer.WriteToTranslation(" { ");

    /// <summary>
    /// Write characters to the given <paramref name="writer"/> to end translation of the
    /// property accessors.
    /// </summary>
    /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the characters.</param>
    protected virtual void WriteAccessorsEndTo(TranslationWriter writer)
        => writer.WriteToTranslation('}');

    /// <summary>
    /// An <see cref="ITranslation"/> for a property accessor.
    /// </summary>
    protected class PropertyAccessorDefinitionTranslation : ITranslation
    {
        private readonly string _accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessorDefinitionTranslation"/>
        /// class.
        /// </summary>
        /// <param name="parent">
        /// The <see cref="PropertyDefinitionTranslation"/> to which the <paramref name="accessor"/>
        /// belongs.
        /// </param>
        /// <param name="accessor">
        /// An <see cref="IComplexMember"/> representing the accessor for which to create the
        /// <see cref="ITranslation"/>.
        /// </param>
        public PropertyAccessorDefinitionTranslation(
            PropertyDefinitionTranslation parent,
            IMember accessor)
        {
            _accessor = IsGetter(accessor) ? "get" : "set";

            var translationSize = _accessor.Length + "; ".Length;

            var accessibility = accessor.GetAccessibilityForTranslation();

            if (accessibility == parent._accessibility)
            {
                TranslationLength = translationSize;
                return;
            }

            _accessor = accessibility + _accessor;
            TranslationLength = translationSize + accessibility.Length;
        }

        #region Setup

        /// <summary>
        /// Gets a value indicating whether the given <paramref name="accessor"/> represents the
        /// property getter. If false, the accessor represents the property setter.
        /// </summary>
        /// <param name="accessor">
        ///  An <see cref="IMember"/> representing the accessor for which to make the determination.
        /// </param>
        /// <returns></returns>
        protected static bool IsGetter(IMember accessor)
            => !accessor.Type.Equals(ClrTypeWrapper.Void);

        #endregion

        /// <inheritdoc />
        public virtual int TranslationLength { get; }

        /// <inheritdoc />
        public virtual void WriteTo(TranslationWriter writer)
        {
            WriteAccessorTo(writer);
            writer.WriteToTranslation("; ");
        }

        /// <summary>
        /// Writes the get or set accessor keyword to the given <paramref name="writer"/>, along
        /// with the appropriate modifier keywords ('internal', 'private', etc).
        /// </summary>
        /// <param name="writer">The <see cref="TranslationWriter"/> to which to write the accessor.</param>
        protected void WriteAccessorTo(TranslationWriter writer)
            => writer.WriteKeywordToTranslation(_accessor);
    }
}