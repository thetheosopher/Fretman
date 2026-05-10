using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Fretman
{
    internal static class AppIcon
    {
        private static readonly int[] Sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };

        public static Icon CreateApplicationIcon()
        {
            byte[][] imageData = new byte[Sizes.Length][];

            for (int i = 0; i < Sizes.Length; i++)
            {
                imageData[i] = RenderIconFrame(Sizes[i]);
            }

            using (MemoryStream iconStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(iconStream, Encoding.Default, true))
                {
                    writer.Write((ushort)0);
                    writer.Write((ushort)1);
                    writer.Write((ushort)imageData.Length);

                    int offset = 6 + (imageData.Length * 16);
                    for (int i = 0; i < Sizes.Length; i++)
                    {
                        int size = Sizes[i];
                        byte[] data = imageData[i];

                        writer.Write((byte)(size >= 256 ? 0 : size));
                        writer.Write((byte)(size >= 256 ? 0 : size));
                        writer.Write((byte)0);
                        writer.Write((byte)0);
                        writer.Write((ushort)1);
                        writer.Write((ushort)32);
                        writer.Write(data.Length);
                        writer.Write(offset);
                        offset += data.Length;
                    }

                    for (int i = 0; i < imageData.Length; i++)
                    {
                        writer.Write(imageData[i]);
                    }
                }

                iconStream.Position = 0;
                return (Icon)new Icon(iconStream).Clone();
            }
        }

        private static byte[] RenderIconFrame(int size)
        {
            using (Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.Clear(Color.Transparent);

                RectangleF body = new RectangleF(size * 0.14f, size * 0.12f, size * 0.72f, size * 0.76f);

                using (System.Drawing.Drawing2D.GraphicsPath path = CreateRoundedRectangle(body, size * 0.1f))
                using (System.Drawing.Drawing2D.LinearGradientBrush fill = new System.Drawing.Drawing2D.LinearGradientBrush(body, Color.FromArgb(96, 64, 192), Color.FromArgb(33, 150, 243), 90f))
                using (Pen outline = new Pen(Color.FromArgb(160, 22, 33, 62), Math.Max(1f, size * 0.02f)))
                using (Pen fretPen = new Pen(Color.FromArgb(235, 255, 255, 255), Math.Max(1f, size * 0.035f)))
                using (Pen stringPen = new Pen(Color.FromArgb(220, 240, 240, 240), Math.Max(1f, size * 0.016f)))
                using (SolidBrush noteBrush = new SolidBrush(Color.FromArgb(255, 255, 193, 7)))
                {
                    graphics.FillPath(fill, path);
                    graphics.DrawPath(outline, path);

                    float left = size * 0.22f;
                    float right = size * 0.78f;
                    float top = size * 0.24f;
                    float bottom = size * 0.76f;

                    for (int fret = 0; fret < 4; fret++)
                    {
                        float x = left + ((right - left) / 3f) * fret;
                        graphics.DrawLine(fretPen, x, top, x, bottom);
                    }

                    for (int str = 0; str < 4; str++)
                    {
                        float y = top + ((bottom - top) / 3f) * str;
                        graphics.DrawLine(stringPen, left, y, right, y);
                    }

                    float markerSize = size * 0.16f;
                    graphics.FillEllipse(noteBrush, size * 0.29f, size * 0.31f, markerSize, markerSize);
                    graphics.FillRectangle(noteBrush, size * 0.56f, size * 0.54f, markerSize, markerSize);
                }

                using (MemoryStream pngStream = new MemoryStream())
                {
                    bitmap.Save(pngStream, ImageFormat.Png);
                    return pngStream.ToArray();
                }
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(RectangleF rectangle, float radius)
        {
            float diameter = radius * 2f;
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
