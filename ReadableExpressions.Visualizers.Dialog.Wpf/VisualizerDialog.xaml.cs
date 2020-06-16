namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Wpf
{
    using System;
    using Core;

    /// <summary>
    /// Interaction logic for VisualizerDialog.xaml
    /// </summary>
    public partial class VisualizerDialog
    {
        private readonly Func<object> _translationFactory;

        public VisualizerDialog(Func<object> translationFactory)
        {
            InitializeComponent();

            _translationFactory = translationFactory;

            Title = new TranslationViewModel().VisualizerTitle;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.HideMinimizeButton();
        }
    }
}
