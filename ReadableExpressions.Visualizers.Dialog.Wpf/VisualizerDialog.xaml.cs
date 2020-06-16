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

            DataContext = ViewModel = new TranslationViewModel();

            ViewModel.Translation = (string)_translationFactory.Invoke();
        }

        private TranslationViewModel ViewModel { get; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.HideMinimizeButton();
        }
    }
}
