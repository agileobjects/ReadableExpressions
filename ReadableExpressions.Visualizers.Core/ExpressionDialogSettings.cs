namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    internal class ExpressionTranslationTheme
    {
        public string Background { get; set; }
        
        public string Default { get; set; }
        
        public string Keyword { get; set; }
        
        public string Variable { get; set; }
        
        public string TypeName { get; set; }
        
        public string InterfaceName { get; set; }
        
        public string CommandStatement { get; set; }
        
        public string Text { get; set; }
        
        public string Numeric { get; set; }
        
        public string Comment { get; set; }
    }

    internal class ExpressionDialogSettings
    {
        public ExpressionTranslationTheme Theme { get; set; }

        public bool UseFullyQualifiedTypeNames { get; set; }

        public bool UseExplicitGenericParameters { get; set; }

        public bool DeclareOutputParametersInline { get; set; }

        public bool ShowQuotedLambdaComments { get; set; }
    }
}
