namespace AgileObjects.ReadableExpressions.Translations;

/// <summary>
/// A Null Object <see cref="ITranslation"/> implementation.
/// </summary>
public class EmptyTranslation : IPotentialEmptyTranslation
{
    /// <summary>
    /// Gets the singleton <see cref="EmptyTranslation"/> instance.
    /// </summary>
    public static readonly IPotentialEmptyTranslation Instance =
        new EmptyTranslation();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyTranslation"/> class.
    /// </summary>
    protected EmptyTranslation()
    {
    }

    /// <inheritdoc />
    public int TranslationLength => 0;

    /// <inheritdoc />
    public bool IsEmpty => true;

    /// <inheritdoc />
    public void WriteTo(TranslationWriter writer)
    {
    }
}