namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Wpf.Controls
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media.Imaging;

    public partial class FeedbackButton
    {
        private static readonly string _assemblyName =
            typeof(FeedbackButton).Assembly.GetName().Name;

        public FeedbackButton()
        {
            InitializeComponent();
        }

        public string IconSuffix
        {
            get => (string)GetValue(IconSuffixProperty);
            set => SetValue(IconSuffixProperty, value);
        }

        public static readonly DependencyProperty IconSuffixProperty =
            DependencyProperty.RegisterAttached(
                nameof(IconSuffix),
                typeof(string),
                typeof(FeedbackButton),
                new PropertyMetadata((d, e) =>
                {
                    ((FeedbackButton)d).GitHubIcon.Source = new BitmapImage(
                        new Uri($"/{_assemblyName};component/WpfDialog/Controls/GitHubIcon{e.NewValue}.png", UriKind.Relative));
                }));

        public void LaunchGitHubIssues(object sender, RoutedEventArgs e)
            => Process.Start("https://github.com/agileobjects/ReadableExpressions/issues/new");
    }
}
