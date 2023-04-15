namespace AgileObjects.ReadableExpressions.Translations;

/// <summary>
/// Implementing translation classes may generate no translation output.
/// </summary>
public interface IPotentialEmptyTranslation : ITranslation
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="IPotentialEmptyTranslation"/> will
    /// generate no translation output.
    /// </summary>
    bool IsEmpty { get; }
}