using System.Drawing;
using System.Runtime.InteropServices;

namespace SimpleImageCapturer.Native
{
    /// <summary>
    /// The <see cref="RECT"/> structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        /// <summary> X position of upper-left corner. </summary>
        public int Left;
        /// <summary> Y position of upper-left corner. </summary>
        public int Top;
        /// <summary> X position of lower-right corner. </summary>
        public int Right;
        /// <summary> Y position of lower-right corner. </summary>
        public int Bottom;

        /// <summary>
        /// Converts the <see cref="Rectangle"/> to the <see cref="RECT"/>.
        /// </summary>
        /// <param name="rectangle">rectangle to be converted.</param>
        public static implicit operator RECT(Rectangle rectangle) => new RECT() { Left = rectangle.Left, Top = rectangle.Top, Right = rectangle.Right, Bottom = rectangle.Bottom };

        /// <summary>
        /// Converts the <see cref="RECT"/> to the <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect"></param>
        public static explicit operator Rectangle(RECT rect) => new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }
}
