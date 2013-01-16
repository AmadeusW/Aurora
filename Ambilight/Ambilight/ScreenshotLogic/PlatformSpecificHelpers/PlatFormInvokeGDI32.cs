using System;
using System.Runtime.InteropServices;

namespace AmadeusW.Ambilight.ScreenshotLogic.PlatformSpecificHelpers
{
	/// <summary>
	/// This class shall keep the GDI32 APIs being used in our program.
    /// Taken from http://www.csharphelp.com/2006/11/capturing-the-screen-image-using-c/
	/// </summary>
	public class PlatformInvokeGDI32
	{

		#region Class Variables
		public  const int SRCCOPY = 13369376;
		#endregion

		#region Class Functions
		[DllImport("gdi32.dll",EntryPoint="DeleteDC")]
		public static extern IntPtr DeleteDC(IntPtr hDc);

		[DllImport("gdi32.dll",EntryPoint="DeleteObject")]
		public static extern IntPtr DeleteObject(IntPtr hDc);

		[DllImport("gdi32.dll",EntryPoint="BitBlt")]
		public static extern bool BitBlt(IntPtr hdcDest,int xDest,int yDest,int wDest,int hDest,IntPtr hdcSource,int xSrc,int ySrc,int RasterOp);

		[DllImport ("gdi32.dll",EntryPoint="CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc,	int nWidth, int nHeight);

		[DllImport ("gdi32.dll",EntryPoint="CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport ("gdi32.dll",EntryPoint="SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hdc,IntPtr bmp);

        // TODO: Perhaps remove. Currently not used

        [DllImport("gdi32.dll", EntryPoint = "GetBitmapBits", SetLastError = true)]
        public static extern int GetBitmapBits([In] IntPtr hbit, int cb, IntPtr lpvBits);

        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan,
           uint cScanLines, [Out] byte[] lpvBits, ref BitmapInfo lpbmi, uint uUsage);

		#endregion
	
		#region Public Constructor
		public PlatformInvokeGDI32()
		{
			// 
			// TODO: Add constructor logic here
			//
		}
		#endregion
	
	}

    // TODO: remove. not used

    /// <summary>
    /// The BITMAPINFO structure defines the dimensions and color information for a DIB
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfo
    {
        /// <summary>
        /// Specifies a BITMAPINFOHEADER structure that contains information about the dimensions of color format
        /// </summary>        
        public BitmapInfoHeader bmiHeader;


        /// <summary>
        /// The bmiColors member contains one of the following: 
        /// An array of RGBQUAD. The elements of the array that make up the color table. 
        /// An array of 16-bit unsigned integers that specifies indexes into the currently realized logical palette. 
        /// This use of bmiColors is allowed for functions that use DIBs. When bmiColors elements contain indexes to 
        /// a realized logical palette, they must also call the following bitmap functions: 
        /// CreateDIBitmap 
        ///
        /// CreateDIBPatternBrush 
        ///
        /// CreateDIBSection 
        ///
        /// The iUsage parameter of CreateDIBSection must be set to DIB_PAL_COLORS.
        /// The number of entries in the array depends on the values of the biBitCount
        /// and biClrUsed members of the BITMAPINFOHEADER structure. 
        /// The colors in the bmiColors table appear in order of importance. For more information, see the Remarks section
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public RGBQuad[]
            bmiColors;
    }

    /// <summary>
    /// The BITMAPINFOHEADER structure contains information about the dimensions and color format of a DIB.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfoHeader
    {
        /// <summary>
        /// Specifies the number of bytes required by the structure. 
        /// </summary>
        public uint biSize;


        /// <summary>
        /// Specifies the width of the bitmap, in pixels. 
        /// </summary>
        /// <remarks> Windows 98/Me, Windows 2000/XP: If biCompression is BI_JPEG or BI_PNG, 
        /// the biWidth member specifies the width of the decompressed JPEG or PNG image file,
        /// respectively. </remarks>
        public int biWidth;


        /// <summary>
        /// Specifies the height of the bitmap, in pixels. If biHeight is positive, the bitmap
        /// is a bottom-up DIB and its origin is the lower-left corner. If biHeight is negative, 
        /// the bitmap is a top-down DIB and its origin is the upper-left corner. 
        /// If biHeight is negative, indicating a top-down DIB, biCompression must be either 
        /// BI_RGB or BI_BITFIELDS. Top-down DIBs cannot be compressed. 
        /// </summary>
        public int biHeight;


        /// <summary>
        /// Specifies the number of planes for the target device. This value must be set to 1.
        /// </summary>
        public ushort biPlanes;


        /// <summary>
        /// Specifies the number of bits-per-pixel. The biBitCount member of the BITMAPINFOHEADER 
        /// structure determines the number of bits that define each pixel and the maximum number 
        /// of colors in the bitmap. This member must be one of the following values. 
        /// </summary>
        public BitCount biBitCount;


        /// <summary>
        /// Specifies the type of compression for a compressed bottom-up bitmap 
        /// (top-down DIBs cannot be compressed). This member can be one of the following values.
        /// </summary>
        public BitmapCompression biCompression;


        /// <summary>
        /// Specifies the size, in bytes, of the image. This may be set to zero for BI_RGB bitmaps. 
        /// Windows 98/Me, Windows 2000/XP: If biCompression is BI_JPEG or BI_PNG, biSizeImage
        /// indicates the size of the JPEG or PNG image buffer, respectively.
        /// </summary>
        public uint biSizeImage;


        /// <summary>
        /// Specifies the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.
        /// An application can use this value to select a bitmap from a resource group that best matches 
        /// the characteristics of the current device.
        /// </summary>
        public int biXPelsPerMeter;


        /// <summary>
        /// Specifies the vertical resolution, in pixels-per-meter, of the target device for the bitmap.
        /// </summary>
        public int biYPelsPerMeter;


        /// <summary>
        /// Specifies the number of color indexes in the color table that are actually used by the bitmap.
        /// If this value is zero, the bitmap uses the maximum number of colors corresponding to the value
        /// of the biBitCount member for the compression mode specified by biCompression. 
        /// If biClrUsed is nonzero and the biBitCount member is less than 16, the biClrUsed member specifies
        /// the actual number of colors the graphics engine or device driver accesses. If biBitCount is 16 or
        /// greater, the biClrUsed member specifies the size of the color table used to optimize performance 
        /// of the system color palettes. If biBitCount equals 16 or 32, the optimal color palette starts 
        /// immediately following the three DWORD masks. 
        /// When the bitmap array immediately follows the BITMAPINFO structure, it is a packed bitmap. 
        /// Packed bitmaps are referenced by a single pointer. Packed bitmaps require that the biClrUsed 
        /// member must be either zero or the actual size of the color table.
        /// </summary>
        public uint biClrUsed;


        /// <summary>
        /// Specifies the number of color indexes that are required for displaying the bitmap. If this value is zero, all colors are required.
        /// </summary>
        public uint biClrImportant;
    }

    /// <summary>
    /// The RGBQUAD structure describes a color consisting of relative intensities of red, green, and blue
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQuad
    {
        /// <summary>
        /// Specifies the intensity of blue in the color
        /// </summary>
        public byte rgbBlue;


        /// <summary>
        /// Specifies the intensity of green in the color
        /// </summary>
        public byte rgbGreen;


        /// <summary>
        /// Specifies the intensity of red in the color
        /// </summary>
        public byte rgbRed;


        /// <summary>
        /// Reserved; must be zero
        /// </summary>
        public byte rgbReserved;
    }

    /// <summary>
    /// Number of bits-per-pixel 
    /// </summary>
    [Flags]
    public enum BitCount : ushort
    {
        /// <summary>
        /// Windows 98/Me, Windows 2000/XP: The number of bits-per-pixel is specified or is 
        /// implied by the JPEG or PNG format. 
        /// </summary>
        BitPerPixel0BPP = 0,
        /// <summary>
        /// The bitmap is monochrome, and the bmiColors member of BITMAPINFO contains two entries.
        /// Each bit in the bitmap array represents a pixel. If the bit is clear, the pixel is 
        /// displayed with the color of the first entry in the bmiColors table; if the bit is set,
        /// the pixel has the color of the second entry in the table.
        /// </summary>
        BitPerPixel1BPP = 1,
        /// <summary>
        /// The bitmap has a maximum of 16 colors, and the bmiColors member of BITMAPINFO contains
        /// up to 16 entries. Each pixel in the bitmap is represented by a 4-bit index into the 
        /// color table. For example, if the first byte in the bitmap is 0x1F, the byte represents 
        /// two pixels. The first pixel contains the color in the second table entry, and the second 
        /// pixel contains the color in the sixteenth table entry.
        /// </summary>
        BitPerPixel4BPP = 4,
        /// <summary>
        /// The bitmap has a maximum of 256 colors, and the bmiColors member of BITMAPINFO contains up 
        /// to 256 entries. In this case, each byte in the array represents a single pixel.
        /// </summary>
        BitPerPixel8BPP = 8,
        /// <summary>
        /// The bitmap has a maximum of 2^16 colors. If the biCompression member of the BITMAPINFOHEADER is BI_RGB,
        /// the bmiColors member of BITMAPINFO is null. Each WORD in the bitmap array represents a single pixel. 
        /// The relative intensities of red, green, and blue are represented with five bits for each color component.
        /// The value for blue is in the least significant five bits, followed by five bits each for green and red.
        /// The most significant bit is not used. The bmiColors color table is used for optimizing colors used on 
        /// palette-based devices, and must contain the number of entries specified by the biClrUsed member of the 
        /// BITMAPINFOHEADER. 
        /// If the biCompression member of the BITMAPINFOHEADER is BitmapCompression.BI_BITFIELDS, the bmiColors member contains 
        /// three DWORD color masks that specify the red, green, and blue components, respectively, of each pixel.
        /// Each WORD in the bitmap array represents a single pixel.
        /// Windows NT/Windows 2000/XP: When the biCompression member is BitmapCompression.BI_BITFIELDS, bits set in each 
        /// DWORD mask must be contiguous and should not overlap the bits of another mask. All the bits in 
        /// the pixel do not have to be used. 
        /// Windows 95/98/Me: When the biCompression member is BitmapCompression.BI_BITFIELDS, the system supports only the following
        /// 16bpp color masks: A 5-5-5 16-bit image, where the blue mask is 0x001F, the green mask is 0x03E0, and 
        /// the red mask is 0x7C00; and a 5-6-5 16-bit image, where the blue mask is 0x001F, the green mask 
        /// is 0x07E0, and the red mask is 0xF800.
        /// </summary>
        BitPerPixel16BPP = 16,
        /// <summary>
        /// The bitmap has a maximum of 2^24 colors, and the bmiColors member of BITMAPINFO is null. 
        /// Each 3-byte triplet in the bitmap array represents the relative intensities of blue, green, and red, 
        /// respectively, for a pixel. The bmiColors color table is used for optimizing colors used on palette-based
        /// devices, and must contain the number of entries specified by the biClrUsed member of the BITMAPINFOHEADER. 
        /// </summary>
        BitPerPixel24BPP = 24,
        /// <summary>
        /// The bitmap has a maximum of 2^32 colors. If the biCompression member of the BITMAPINFOHEADER is BI_RGB,
        /// the bmiColors member of BITMAPINFO is null. Each DWORD in the bitmap array represents the relative 
        /// intensities of blue, green, and red, respectively, for a pixel. The high byte in each DWORD is not used.
        /// The bmiColors color table is used for optimizing colors used on palette-based devices, and must contain 
        /// the number of entries specified by the biClrUsed member of the BITMAPINFOHEADER. 
        /// If the biCompression member of the BITMAPINFOHEADER is BitmapCompression.BI_BITFIELDS, the bmiColors member contains three
        /// DWORD color masks that specify the red, green, and blue components, respectively, of each pixel. Each 
        /// DWORD in the bitmap array represents a single pixel.
        /// Windows NT/ 2000: When the biCompression member is BitmapCompression.BI_BITFIELDS, bits set in each DWORD mask must be 
        /// contiguous and should not overlap the bits of another mask. All the bits in the pixel do not need to be
        /// used.
        /// Windows 95/98/Me: When the biCompression member is BitmapCompression.BI_BITFIELDS, the system supports only the following
        /// 32-bpp color mask: The blue mask is 0x000000FF, the green mask is 0x0000FF00, and the red mask is 
        /// 0x00FF0000.
        /// </summary>
        BitPerPixel32BPP = 32
    }

    /// <summary>
    /// Bitmap Compression values
    /// </summary>
    [Flags]
    public enum BitmapCompression : uint
    {
        /// <summary>
        /// An uncompressed format.
        /// </summary>
        BI_RGB = 0,
        /// <summary>
        /// A run-length encoded (RLE) format for bitmaps with 8 bpp. 
        /// The compression format is a 2-byte format consisting of a count 
        /// byte followed by a byte containing a color index.
        /// </summary>
        BI_RLE8 = 1,
        /// <summary>
        /// An RLE format for bitmaps with 4 bpp. The compression format is a 
        /// 2-byte format consisting of a count byte followed by two word-length
        /// color indexes.
        /// </summary>
        BI_RLE4 = 2,
        /// <summary>
        /// Specifies that the bitmap is not compressed and that the color table 
        /// consists of three DWORD color masks that specify the red, green, and
        /// blue components, respectively, of each pixel. This is valid when used 
        /// with 16- and 32-bpp bitmaps.
        /// </summary>
        BI_BITFIELDS = 3,
        /// <summary>
        /// Windows 98/Me, Windows 2000/XP: Indicates that the image is a JPEG image.
        /// </summary>
        BI_JPEG = 4,
        /// <summary>
        /// Windows 98/Me, Windows 2000/XP: Indicates that the image is a PNG image.
        /// </summary>
        BI_PNG = 5,
    }

}
