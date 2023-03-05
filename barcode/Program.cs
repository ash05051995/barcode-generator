using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing.Common;
using ZXing;
using System.Runtime.InteropServices;

namespace barcode
{
    internal class Program
    {
        #region Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCurrentConsoleFont(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            [Out][MarshalAs(UnmanagedType.LPStruct)] ConsoleFontInfo lpConsoleCurrentFont);

        [StructLayout(LayoutKind.Sequential)]
        internal class ConsoleFontInfo
        {
            internal int nFont;
            internal Coord dwFontSize;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Coord
        {
            [FieldOffset(0)]
            internal short X;
            [FieldOffset(2)]
            internal short Y;
        }

        private const int GENERIC_READ = unchecked((int)0x80000000);
        private const int GENERIC_WRITE = 0x40000000;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int OPEN_EXISTING = 3;

        #endregion

        private static Size GetConsoleFontSize()
        {
            IntPtr outHandle = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            int errorCode = Marshal.GetLastWin32Error();

            if (outHandle.ToInt32() == INVALID_HANDLE_VALUE)
                throw new IOException("Unable to open CONOUT$", errorCode);

            ConsoleFontInfo cfi = new ConsoleFontInfo();

            if (!GetCurrentConsoleFont(outHandle, false, cfi))
                throw new InvalidOperationException("Unable to get font information.");

            return new Size(cfi.dwFontSize.X, cfi.dwFontSize.Y);
        }

        static void Main(string[] args)
        {
            Bitmap barcodeImage = GenerateBarcode("https://example.com");

            Size imageSize = new Size(20, 10);

            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;

            int x = (consoleWidth - imageSize.Width) / 2;
            int y = (consoleHeight - imageSize.Height) / 2;

            Point location = new Point(x, y);

            using (Graphics g = Graphics.FromHwnd(GetConsoleWindow()))
            {
                using (Image image = Image.FromHbitmap(barcodeImage.GetHbitmap()))
                {
                    Size fontSize = GetConsoleFontSize();

                    Rectangle imageRect = new Rectangle(location.X * fontSize.Width, location.Y * fontSize.Height, imageSize.Width * fontSize.Width, imageSize.Height * fontSize.Height);
                    g.DrawImage(image, imageRect);
                }
            }

            Console.ReadLine();
        }

        static Bitmap GenerateBarcode(string url)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE, // use QR code format
                Options = new EncodingOptions
                {
                    Width = 300, // set the barcode image width
                    Height = 300, // set the barcode image height
                    Margin = 1 // set the barcode image margin
                }
            };
            Bitmap barcodeImage = barcodeWriter.Write(url);
            return barcodeImage;
        }
    }
}
