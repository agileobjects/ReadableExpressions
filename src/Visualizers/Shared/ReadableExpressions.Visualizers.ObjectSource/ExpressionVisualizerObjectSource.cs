namespace AgileObjects.ReadableExpressions.Visualizers.ObjectSource;

using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Core.Configuration;
using Formatting;
using Translations.Formatting;

public static class ExpressionVisualizerObjectSource
{
    private static readonly ITranslationFormatter _htmlFormatter = TranslationHtmlFormatter.Instance;

    public static void GetData(
        object target,
        Stream outgoingData,
        Action<Stream, string> serializer)
    {
        try
        {
            var translated = GetTranslationFor(target) ?? "default(void)";

            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid().ToString().Substring(0, 6)}.html");

            File.WriteAllText(outputPath, translated);

            serializer.Invoke(outgoingData, outputPath);
        }
        catch (Exception ex)
        {
            var errorMessage = $@"</pre>
<h1>Ut-oh</h1>
<p>
    An exception occurred translating that 
    <span class=""tn"">{target?.GetType().Name ?? "target"}</span>.
</p>
<p>
    Please report this error with the stack trace below using 
    <a href=""https://github.com/agileobjects/ReadableExpressions/issues/new"" target=""_blank"">
    https://github.com/agileobjects/ReadableExpressions/issues/new
    </a>.
</p>

<p>Thanks! (and sorry about that)<br />Steve</p>

<hr />

<p>{ex}</p>
<pre>".TrimStart();

            serializer.Invoke(outgoingData, errorMessage);
        }
    }

    private static string GetTranslationFor(object target)
    {
        switch (target)
        {
            case Expression expression:
                return expression.ToReadableString(ApplyDialogSettings);

            case Type type:
                return Translate(type);

            case ConstructorInfo ctor:
                return ctor.ToReadableString(ApplyDialogSettings);

            case MethodInfo method:
                return method.ToReadableString(ApplyDialogSettings);

            case PropertyInfo property:
                return property.ToReadableString(ApplyDialogSettings);

            default:
                if (target == null)
                {
                    return string.Empty;
                }

                return Translate(target.GetType());
        }
    }

    private static string Translate(Type type)
        => type.ToReadableString(ApplyDialogSettings);

    private static ITranslationSettings ApplyDialogSettings(ITranslationSettings settings)
        => GetDialogSettings().Update(settings.FormatUsing(_htmlFormatter));

    private static VisualizerDialogSettings GetDialogSettings()
        => VisualizerDialogSettings.GetInstance();
}