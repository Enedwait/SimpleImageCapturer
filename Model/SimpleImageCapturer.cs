using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using SimpleImageCapturer.Native;

namespace SimpleImageCapturer.Model
{
    /// <summary>
    /// The <see cref="SimpleImageCapturer"/> class.
    /// </summary>
    internal sealed class SimpleImageCapturer
    {
        #region Private
        
        /// <summary>
        /// The image capturing timer.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private System.Timers.Timer _timer;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the process name used as a target for image capturing.
        /// </summary>
        public string ProcessName { get; private set; }

        /// <summary>
        /// Gets the process used as a target for image capturing.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the process was found or not.
        /// If the process is not found, then the image capturer will use whole primary desktop as it's target.
        /// </summary>
        public bool IsProcessFound => Process != null;

        /// <summary>
        /// Gets the image capturing interval.
        /// </summary>
        public double Interval { get; private set; }

        /// <summary>
        /// Gets the capturing bounds.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// Gets the last error if any.
        /// </summary>
        public Exception LastError { get; private set; }

        /// <summary>
        /// Gets the capturer's activity flag.
        /// </summary>
        public bool IsActive => _timer.Enabled;

        #endregion

        #region Methods

        /// <summary>
        /// Begins the screenshot capturing.
        /// </summary>
        /// <remarks>if the process name not specified or process doesn't exist, then the image capturer will use whole primary desktop as it's target.</remarks>
        /// <param name="processName">process name to be used as an image target.</param>
        /// <param name="interval">screenshot capturing interval in msec.</param>
        public void BeginCapture(string processName, double interval)
        {
            Task.Run(() =>
            {
                ProcessName = processName;

                Bounds = GetBounds();

                if (IsProcessFound)
                    NativeFunctions.BringProcessWindowToFront(Process);

                // check the interval
                if (interval < 1)
                    interval = 1;

                // initialize timer
                _timer = new System.Timers.Timer(Interval = interval);
                _timer.AutoReset = true;
                _timer.Elapsed += (sender, args) =>
                {
                    Bounds = GetBounds();
                    TakeScreenshot(Bounds);
                };

                // start timer
                _timer.Start();

                // take the 1st screenshot immediately
                TakeScreenshot(Bounds);
            });
        }

        /// <summary>
        /// Gets the target bounds.
        /// </summary>
        /// <returns>target bounds if the process has been specified or primary desktop bounds.</returns>
        private Rectangle GetBounds()
        {
            try
            {
                // check if we are targeting process
                if (!string.IsNullOrWhiteSpace(ProcessName))
                {
                    // check if the process not set or exited
                    if (Process == null || Process.HasExited) // then acquire the process
                        Process = GetProcess(ProcessName);

                    // check if process acquired
                    if (Process != null)
                    {
                        Rectangle rectangle = NativeFunctions.GetProcessWindowRectangle(Process);
                        if (rectangle != Rectangle.Empty)
                            return rectangle;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
                Process = null;
            }

            return Screen.PrimaryScreen.Bounds;
        }

        /// <summary>
        /// Gets the first process found.
        /// </summary>
        /// <param name="processName">process name.</param>
        /// <returns>the first process found.</returns>
        private Process GetProcess(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return null;

            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length == 0 ? null : processes[0];
        }

        /// <summary>
        /// Takes a screenshot within the specified bounds.
        /// </summary>
        /// <param name="bounds">target bounds.</param>
        /// <returns>screenshot taken.</returns>
        public Bitmap TakeScreenshot(Rectangle bounds)
        {
            try
            {
                Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.CopyFromScreen(new Point(bounds.X, bounds.Y), Point.Empty, bounds.Size);
                }

                OnScreenshotTaken(bitmap);
                return bitmap;
            }
            catch (Exception ex)
            {
                OnCaptureFailed(ex);
                return null;
            }
        }

        /// <summary>
        /// Stops the screenshot capturing process.
        /// </summary>
        public void StopCapture() => _timer?.Stop();

        #endregion

        #region Events

        /// <summary>
        /// This event occurs when the screenshot is taken.
        /// </summary>
        public event EventHandler<Bitmap> ScreenshotTaken;

        /// <summary>
        /// Raises the 'ScreenshotTaken' event.
        /// </summary>
        /// <param name="screenshot">screenshot.</param>
        private void OnScreenshotTaken(Bitmap screenshot) => ScreenshotTaken?.Invoke(this, screenshot);

        /// <summary>
        /// This event occurs if the capture has failed.
        /// </summary>
        public event EventHandler<Exception> CaptureFailed;

        /// <summary>
        /// Raises the 'CaptureFailed' event.
        /// </summary>
        /// <param name="exception">exception.</param>
        private void OnCaptureFailed(Exception exception)
        {
            LastError = exception;
            CaptureFailed?.Invoke(this, exception);
        }

        #endregion
    }
}
