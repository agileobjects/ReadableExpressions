namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Theming;
    using static System.StringComparison;

    internal class FeedbackButton : Button, IThemeable
    {
        private readonly double _imageHeight;

        public FeedbackButton(VisualizerDialog dialog)
        {
            base.Text = "Feedback";
            base.TextAlign = ContentAlignment.MiddleRight;

            Margin = new Padding(2);
            TextImageRelation = TextImageRelation.ImageBeforeText;
            ImageAlign = ContentAlignment.MiddleLeft;
            _imageHeight = Height - 4;

            dialog.RegisterThemeable(this);

            Click += (sender, args) =>
                Process.Start("https://github.com/agileobjects/ReadableExpressions/issues/new");
        }

        private void SetGitHubIcon(VisualizerDialogTheme theme)
        {
            var imageResourceName = typeof(VisualizerDialog)
                .Assembly
                .GetManifestResourceNames()
                .First(resourceName => Path
                    .GetFileNameWithoutExtension(resourceName)
                    .EndsWith($"GitHubIcon{theme.IconSuffix}", Ordinal));

            var imageStream = typeof(VisualizerDialog)
                .Assembly
                .GetManifestResourceStream(imageResourceName);

            using (imageStream)

            // ReSharper disable once AssignNullToNotNullAttribute
            using (var image = Image.FromStream(imageStream))
            {
                var scaleFactor = _imageHeight / image.Height;
                var newWidth = (int)(image.Width * scaleFactor);
                var newHeight = (int)(image.Height * scaleFactor);

                var newImage = new Bitmap(newWidth, newHeight);

                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
                }

                Image = newImage;
            }
        }

        void IThemeable.Apply(VisualizerDialogTheme theme)
        {
            SetGitHubIcon(theme);
        }
    }
}