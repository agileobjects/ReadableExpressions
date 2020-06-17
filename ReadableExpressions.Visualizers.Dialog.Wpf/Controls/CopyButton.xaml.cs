namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Wpf.Controls
{
    using System.Windows;

    public partial class CopyButton
    {
        public CopyButton()
        {
            InitializeComponent();
        }

        public string TranslationRaw
        {
            get => (string)GetValue(TranslationRawProperty);
            set => SetValue(TranslationRawProperty, value);
        }

        public static readonly DependencyProperty TranslationRawProperty =
            DependencyProperty.RegisterAttached(
                nameof(TranslationRaw),
                typeof(string),
                typeof(CopyButton));

        public void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            var copyButton = (CopyButton)sender;
            var unformatted = copyButton.TranslationRaw;

            Clipboard.SetText(unformatted);
        }
    }
}
