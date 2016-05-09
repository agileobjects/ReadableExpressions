namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class ExpressionDialog : Form
    {
        public static readonly ExpressionDialog Instance = new ExpressionDialog();

        private readonly TextBox _textBox;
        private readonly Size _maximumSize;
        private readonly ToolStrip _toolbar;
        private readonly int _titleBarHeight;
        private bool _autoSize;

        private ExpressionDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            _maximumSize = GetDialogMaximumSize();

            _textBox = AddTextBox();
            _toolbar = AddToolbar();

            var screenRectangle = RectangleToScreen(ClientRectangle);
            _titleBarHeight = screenRectangle.Top - Top;
        }

        private Size GetDialogMaximumSize()
        {
            var screenSize = Screen.FromControl(this).Bounds.Size;

            return new Size(
                Convert.ToInt32(screenSize.Width * .9),
                Convert.ToInt32(screenSize.Height * .8));
        }

        private TextBox AddTextBox()
        {
            var textBox = new TextBox
            {
                Font = new Font(FontFamily.GenericMonospace, 11.0f),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true
            };

            var textBoxPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            textBoxPanel.Controls.Add(textBox);
            Controls.Add(textBoxPanel);

            textBox.Resize += (sender, arg) => textBoxPanel.Size = ((Control)sender).Size;

            return textBox;
        }

        private ToolStrip AddToolbar()
        {
            var copyButton = new ToolStripButton
            {
                Alignment = ToolStripItemAlignment.Right,
                Text = "Copy",
            };

            copyButton.Click += (sender, args) => Clipboard.SetText(_textBox.Text);

            var toolbar = new ToolStrip(copyButton)
            {
                Dock = DockStyle.Bottom,
                GripStyle = ToolStripGripStyle.Hidden
            };

            Controls.Add(toolbar);

            return toolbar;
        }

        public override bool AutoSize => _autoSize;

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

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var newTextBoxSize = new Size(
                Size.Width - 10,
                Size.Height - _titleBarHeight - _toolbar.Height);

            SetTextBoxSize(newTextBoxSize);
        }

        public ExpressionDialog WithText(string expression)
        {
            var textSize = TextRenderer.MeasureText(expression, _textBox.Font);

            var textBoxSize = new Size(
                textSize.Width + _textBox.Padding.Left + _textBox.Padding.Right + SystemInformation.VerticalScrollBarWidth + 10,
                textSize.Height + _textBox.Padding.Top + _textBox.Padding.Bottom + _textBox.Font.Height);

            SetTextBoxSize(textBoxSize);

            _textBox.Text = expression;

            return this;
        }

        private void SetTextBoxSize(Size newSize)
        {
            EnableAutoSize();

            var finalSize = new Size(
                Math.Max(newSize.Width, _textBox.Parent.Width),
                Math.Max(newSize.Height, _textBox.Parent.Height));

            _textBox.Size = finalSize;

            DisableAutoSize();
        }

        private void EnableAutoSize()
        {
            _autoSize =
            _textBox.Parent.AutoSize = true;
        }

        private void DisableAutoSize()
        {
            _autoSize =
            _textBox.Parent.AutoSize = false;
        }
    }
}
