using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AmadeusW.Ambilight.Helpers
{
    // TODO: am I even using this?
    internal static class TrayIconGenerator
    {
        private static ImageSource _currentIcon;
        public static ImageSource CurrentIcon { get { return _currentIcon; }}

        public static void createIcon(bool active, string contents)
        {
            var x = active ? Properties.Resources.TrayIconOn : Properties.Resources.TrayIconOff;

            Graphics g = Graphics.FromImage(x);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.DrawString(contents, new Font("Arial", 6), new SolidBrush(System.Drawing.Color.White), 4, 4);
            Bitmap b = new Bitmap(16, 16, g);

            Image iconFile = Image.FromHbitmap(b.GetHbitmap());

            // Create .ICO
            MemoryStream ms = new MemoryStream();
            // From: http://stackoverflow.com/a/11448060/368354
            // ICO header
            ms.WriteByte(0); ms.WriteByte(0);
            ms.WriteByte(1); ms.WriteByte(0);
            ms.WriteByte(1); ms.WriteByte(0);

            // Image size
            // Set to 0 for 256 px width/height
            ms.WriteByte(0);
            ms.WriteByte(0);
            // Palette
            ms.WriteByte(0);
            // Reserved
            ms.WriteByte(0);
            // Number of color planes
            ms.WriteByte(1); ms.WriteByte(0);
            // Bits per pixel
            ms.WriteByte(32); ms.WriteByte(0);

            // Data size, will be written after the data
            ms.WriteByte(0);
            ms.WriteByte(0);
            ms.WriteByte(0);
            ms.WriteByte(0);

            // Offset to image data, fixed at 22
            ms.WriteByte(22);
            ms.WriteByte(0);
            ms.WriteByte(0);
            ms.WriteByte(0);

            // Writing actual data
            iconFile.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            // Getting data length (file length minus header)
            long Len = ms.Length - 22;

            // Write it in the correct place
            ms.Seek(14, SeekOrigin.Begin);
            ms.WriteByte((byte)Len);
            ms.WriteByte((byte)(Len >> 8));

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            

            _currentIcon = bi;

            // Now I need to make it into an usable icon file
            ms.Close();

        }

        public static void setCurrentIcon(bool active, string contents)
        {
            //BitmapImage bi = new BitmapImage();
            /*_currentIcon = Imaging.CreateBitmapSourceFromHBitmap(
                active ? Properties.Resources.TrayIconOn.GetHbitmap() : Properties.Resources.TrayIconOff.GetHbitmap(), 
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
             */
            _currentIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/TrayIconOn.png"));

        }
    }
}
