namespace AgileObjects.ReadableExpressions.Visualizers.ObjectSource
{
    using Core.Configuration;

    internal static class DialogSettingsExtensions
    {
        public static ITranslationSettings Update(
            this VisualizerDialogSettings dialogSettings,
            ITranslationSettings settings)
        {
            if (dialogSettings.UseFullyQualifiedTypeNames)
            {
                settings = settings.UseFullyQualifiedTypeNames;
            }

            if (dialogSettings.UseExplicitTypeNames)
            {
                settings = settings.UseExplicitTypeNames;
            }

            if (dialogSettings.UseExplicitGenericParameters)
            {
                settings = settings.UseExplicitGenericParameters;
            }

            if (dialogSettings.DeclareOutputParametersInline)
            {
                settings = settings.DeclareOutputParametersInline;
            }

            if (dialogSettings.DiscardUnusedParameters)
            {
                settings = settings.DiscardUnusedParameters;
            }

            if (dialogSettings.ShowImplicitArrayTypes)
            {
                settings = settings.ShowImplicitArrayTypes;
            }

            if (dialogSettings.ShowLambdaParameterTypeNames)
            {
                settings = settings.ShowLambdaParameterTypes;
            }

            if (dialogSettings.ShowQuotedLambdaComments)
            {
                settings = settings.ShowQuotedLambdaComments;
            }

            settings.IndentUsing(dialogSettings.Indent);

            return settings;
        }
    }
}
