namespace AgileObjects.ReadableExpressions.ExpressionVisualizer
{
    using System.Drawing;
    using System.Windows.Forms;

    public class ExpressionDialog : Form
    {
        private static readonly ExpressionDialog _instance = new ExpressionDialog();

        private readonly Label _label;

        private ExpressionDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(778, 600);
            MinimizeBox = false;

            _label = new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 15, 10, 12),
                Font = new Font(FontFamily.GenericMonospace, 11.0f)
            };

            Controls.Add(_label);
        }

        public override string Text => string.Empty;

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((ModifierKeys == Keys.None) && (keyData == Keys.Escape))
            {
                Close();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        public static void Show(string expression)
        {
            _instance._label.Text = expression;
            _instance.ShowDialog();
        }
    }
}
