namespace AgileObjects.ReadableExpressions.ExpressionVisualizer
{
    using System.Drawing;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.DebuggerVisualizers;

    public class Vs2015ExpressionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(
            IDialogVisualizerService windowService,
            IVisualizerObjectProvider objectProvider)
        {
            var form = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(778, 600),
                Text = string.Empty,
                MinimizeBox = false
            };

            var label = new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 15, 10, 12),
                Font = new Font(FontFamily.GenericMonospace, 11.0f),
                Text = objectProvider.GetObject().ToString()
            };

            form.Controls.Add(label);
            form.ShowDialog();
        }
    }
}
