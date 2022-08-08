using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SimpleImageCapturer.Native
{
    /// <summary>
    /// The <see cref="NativeFunctions"/> class.
    /// </summary>
    internal static class NativeFunctions
    {
        #region Consts

        [EditorBrowsable(EditorBrowsableState.Never)]
        const int SW_RESTORE = 9;

        #endregion

        #region Methods

        /// <summary>
        /// Brings the specified process' window to the front.
        /// </summary>
        /// <param name="process">process.</param>
        public static void BringProcessWindowToFront(Process process)
        {
            if (process == null || process.MainWindowHandle == IntPtr.Zero)
                return;

            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
                ShowWindow(handle, SW_RESTORE);

            SetForegroundWindow(handle);
        }

        /// <summary>
        /// Gets the window bounds of the specified process.
        /// </summary>
        /// <param name="process">process.</param>
        /// <returns>process window bounds.</returns>
        public static Rectangle GetProcessWindowRectangle(Process process)
        {
            if (process != null)
                if (GetWindowRect(process.MainWindowHandle, out RECT rect))
                    return (Rectangle)rect;

            return Rectangle.Empty;
        }

        #endregion

        #region Extern

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);             

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        #endregion
    }
}
