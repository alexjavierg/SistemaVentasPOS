using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Ventas.Desktop.Helpers
{
    public static class EscPosImageHelper
    {
        public static byte[] GetImageBytes(string imagePath, int maxWidth = 384)
        {
            if (!File.Exists(imagePath)) return Array.Empty<byte>();

            try
            {
                using var bmp = new Bitmap(imagePath);
                int width = bmp.Width;
                int height = bmp.Height;

                if (width > maxWidth)
                {
                    height = (int)((double)height * maxWidth / width);
                    width = maxWidth;
                }

                using var resizedBmp = new Bitmap(bmp, new Size(width, height));
                
                int widthBytes = (resizedBmp.Width + 7) / 8;
                var data = new List<byte>();
                
                // Initialize raster image print (GS v 0)
                data.AddRange(new byte[] { 0x1D, 0x76, 0x30, 0x00 });
                data.Add((byte)(widthBytes % 256));
                data.Add((byte)(widthBytes / 256));
                data.Add((byte)(resizedBmp.Height % 256));
                data.Add((byte)(resizedBmp.Height / 256));

                for (int y = 0; y < resizedBmp.Height; y++)
                {
                    for (int xByte = 0; xByte < widthBytes; xByte++)
                    {
                        byte b = 0;
                        for (int bit = 0; bit < 8; bit++)
                        {
                            int x = xByte * 8 + bit;
                            if (x < resizedBmp.Width)
                            {
                                var color = resizedBmp.GetPixel(x, y);
                                int luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                                if (luminance < 128 && color.A > 128)
                                {
                                    b |= (byte)(1 << (7 - bit));
                                }
                            }
                        }
                        data.Add(b);
                    }
                }
                return data.ToArray();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
    }
}
