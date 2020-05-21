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
                var unformatted = copyButton._dialog.Viewer.GetContentRaw();

                Clipboard.SetText(unformatted);
            };
        }
    }
}
