namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System.Drawing;
    using System.Windows.Forms;

    public class ExpressionDialog : Form
    {
        public static readonly ExpressionDialog Instance = new ExpressionDialog();

        private readonly Label _label;

        private ExpressionDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;

            _label = new Label
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 15, 10, 12),
                Font = new Font(FontFamily.GenericMonospace, 11.0f),
                AutoSize = true
            };

            Controls.Add(_label);
        }

        public override bool AutoSize => true;

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

        public ExpressionDialog WithText(string expression)
        {
            Instance._label.Text = expression;

            return this;
        }
    }
}
