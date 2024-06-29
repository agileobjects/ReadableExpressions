namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls;

using System.Windows.Forms;

internal class CloseButton : Button
{
    private readonly VisualizerDialog _dialog;

    public CloseButton(VisualizerDialog dialog)
    {
        _dialog = dialog;

        base.Text = "Close";

        Margin = new(2);

        dialog.RegisterThemeable(this);

        Click += (sender, _) => ((CloseButton)sender)._dialog.Close();
    }
}