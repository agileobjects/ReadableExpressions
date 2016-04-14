namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class ExpressionDialog : Form
    {
        public static readonly ExpressionDialog Instance = new ExpressionDialog();

        private readonly Label _label;
        private readonly Size _maximumSize;

        private ExpressionDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;

            _label = new Label
            {
                Margin = new Padding(10, 15, 10, 12),
                Font = new Font(FontFamily.GenericMonospace, 11.0f),
                AutoSize = true
            };

            _maximumSize = GetMaximumSize();

            Controls.Add(_label);
        }

        private Size GetMaximumSize()
        {
            var screenSize = Screen.FromControl(this).Bounds.Size;

            return new Size(
                Convert.ToInt32(screenSize.Width * .9),
                Convert.ToInt32(screenSize.Height * .8));
        }

        public override bool AutoSize => true;

        public override bool AutoScroll => true;

        public override Size MaximumSize => _maximumSize;

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
