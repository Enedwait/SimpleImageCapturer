using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SimpleImageCapturer
{
    /// <summary>
    /// The <see cref="MainWindow"/> class.
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    internal sealed partial class MainWindow : Window
    {
        #region Private

        /// <summary>
        /// Simple image capturer.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private Model.SimpleImageCapturer _capturer = new Model.SimpleImageCapturer();

        #endregion

        #region Init

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // set window title
            Title = $"{System.Windows.Forms.Application.ProductName} [v.{System.Windows.Forms.Application.ProductVersion}]";

            // set saved values back
            textBoxProcessName.Text = Properties.Settings.Default.Process;

            textBoxDestinationDirectory.Text = Properties.Settings.Default.Destination;
            if (string.IsNullOrWhiteSpace(textBoxDestinationDirectory.Text) || !Directory.Exists(textBoxDestinationDirectory.Text))
                textBoxDestinationDirectory.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            textBoxCaptureInterval.Text = Properties.Settings.Default.Interval.ToString(CultureInfo.InvariantCulture);

            // subscribe to capturer's events
            _capturer.ScreenshotTaken += _capturer_ScreenshotTaken;
            _capturer.CaptureFailed += _capturer_CaptureFailed;
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Handles the 'ScreenshotTaken' event of the image capturer.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="bitmap">screenshot.</param>
        private async void _capturer_ScreenshotTaken(object sender, Bitmap bitmap)
        {
            Dispatcher.Invoke(() =>
            {
                // check if bitmap is set
                if (bitmap != null)
                {
                    string fileName = Path.Combine(textBoxDestinationDirectory.Text, $"{DateTime.UtcNow:yyyy-MM-dd_HHmmssfff}.png");
                    bitmap.Save(fileName, ImageFormat.Png);
                    bitmap.Dispose();
                }

                recordIndicator.Visibility = Visibility.Visible;

                TextBlockProcessDetails.Text = _capturer.IsProcessFound ?
                    $"Acquired {_capturer.Bounds.Width}x{_capturer.Bounds.Height} @ [{_capturer.Bounds.Left}, {_capturer.Bounds.Top}, {_capturer.Bounds.Right}, {_capturer.Bounds.Bottom}]" :
                    $"Process window bounds not acquired.";
            }, DispatcherPriority.Normal);

            await Task.Run(() => Thread.Sleep((int)(_capturer.Interval / 3d)));

            Dispatcher.Invoke(() => { recordIndicator.Visibility = Visibility.Hidden; }, DispatcherPriority.Normal);
        }

        /// <summary>
        /// Handles the 'CaptureFailed' event of the image capturer.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">exception.</param>
        private void _capturer_CaptureFailed(object sender, Exception e)
        { }

        /// <summary>
        /// Handles the 'Click' event of the ButtonStart.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">event args.</param>
        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonStart.IsEnabled = false;
                buttonStop.IsEnabled = true;

                // get the destination directory and validate it
                string destination = textBoxDestinationDirectory.Text;

                if (string.IsNullOrWhiteSpace(destination))
                    destination = Path.GetFullPath("");

                if (!Path.IsPathRooted(destination))
                    destination = Path.GetFullPath(destination);

                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);

                textBoxDestinationDirectory.Text = destination;

                // begin image capturing
                _capturer.BeginCapture(textBoxProcessName.Text, double.Parse(textBoxCaptureInterval.Text, CultureInfo.InvariantCulture) * 1000d);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the 'Click' event of the ButtonStop.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">event args.</param>
        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;

            TextBlockProcessDetails.Text = null;

            _capturer?.StopCapture();
        }

        /// <summary>
        /// Handles the 'Click' event of the ButtonOpen.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">event args.</param>
        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxDestinationDirectory.Text))
                textBoxDestinationDirectory.Text = Path.GetFullPath("");

            if (!Path.IsPathRooted(textBoxDestinationDirectory.Text))
                textBoxDestinationDirectory.Text = Path.GetFullPath(textBoxDestinationDirectory.Text);

            if (!Directory.Exists(textBoxDestinationDirectory.Text))
                Directory.CreateDirectory(textBoxDestinationDirectory.Text);

            Process.Start("explorer.exe", $"\"{textBoxDestinationDirectory.Text}\"");
        }

        /// <summary>
        /// Handles the 'Closed' event of the MainWindow.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">event args.</param>
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Process = textBoxProcessName.Text;
            Properties.Settings.Default.Destination = textBoxDestinationDirectory.Text;
            if (double.TryParse(textBoxCaptureInterval.Text, out double interval))
                Properties.Settings.Default.Interval = interval;
            Properties.Settings.Default.Save();
        }

        #endregion
    }
}
