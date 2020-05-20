namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using System.Windows.Forms;

    internal class CopyButton : Button
    {
        private readonly VisualizerDialog _dialog;

        public CopyButton(VisualizerDialog dialog)
        {
            _dialog = dialog;

            base.Text = "Copy";

            Margin = new Padding(2);

            dialog.RegisterThemeable(this);

            Click += (sender, args) =>
            {
                var copyButton = (CopyButton)sender;

                // ReSharper disable PossibleNullReferenceException
                var viewerContent = copyButton._dialog.Viewer.Document.Body.InnerHtml;
                // ReSharper restore PossibleNullReferenceException

                var unformatted = copyButton._dialog.Formatter.GetRaw(viewerContent);

                Clipboard.SetText(unformatted);
            };
        }
    }
}
