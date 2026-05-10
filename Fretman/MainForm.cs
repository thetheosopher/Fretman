using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Fretman
{
    public sealed class RenderConfiguration
    {
        public string Title { get; set; }

        public short Frets { get; set; }

        public string TitleFont { get; set; }

        public string NoteFont { get; set; }

        public float FretboardLength { get; set; }

        public float StringSpacing { get; set; }

        public float Padding { get; set; }

        public float NutWidth { get; set; }

        public float FretWidth { get; set; }

        public float StringWidth { get; set; }

        public bool DisplayDots { get; set; }

        public float DotWidth { get; set; }

        public float NoteWidth { get; set; }

        public int BackgroundColorArgb { get; set; }

        public int FretColorArgb { get; set; }

        public int DotColorArgb { get; set; }

        public int StringColorArgb { get; set; }

        public int NutColorArgb { get; set; }

        public int TitleColorArgb { get; set; }

        public int NoteOutlineColorArgb { get; set; }

        public float NoteOutlineWidth { get; set; }

        public bool ShowFretNumbers { get; set; }

        public int RootHighlightColorArgb { get; set; }

        public int FingerboardBackground { get; set; }

        public bool UseSharps { get; set; }

        public int RootNoteIndex { get; set; }

        public int[] BaseNotes { get; set; }

        public int[] NoteStyles { get; set; }

        public int[] NoteColorsArgb { get; set; }

        public int[] NoteTextColorsArgb { get; set; }
    }

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

    public partial class MainForm : Form
    {
        private sealed class RootNoteOption
        {
            public RootNoteOption(Fretboard.Notes note, string menuText, string sharpText, string flatText)
            {
                Note = note;
                MenuText = menuText;
                SharpText = sharpText;
                FlatText = flatText;
            }

            public Fretboard.Notes Note { get; private set; }

            public string MenuText { get; private set; }

            public string SharpText { get; private set; }

            public string FlatText { get; private set; }
        }

        private sealed class ScalePattern
        {
            public ScalePattern(string menuText, string titleText, params int[] intervals)
            {
                MenuText = menuText;
                TitleText = titleText;
                Intervals = intervals;
            }

            public string MenuText { get; private set; }

            public string TitleText { get; private set; }

            public int[] Intervals { get; private set; }
        }

        private sealed class ScaleCategory
        {
            public ScaleCategory(string menuText, params ScalePattern[] patterns)
            {
                MenuText = menuText;
                Patterns = patterns;
            }

            public string MenuText { get; private set; }

            public ScalePattern[] Patterns { get; private set; }
        }

        private sealed class ScaleSelection
        {
            public ScaleSelection(RootNoteOption root, ScalePattern pattern)
            {
                Root = root;
                Pattern = pattern;
            }

            public RootNoteOption Root { get; private set; }

            public ScalePattern Pattern { get; private set; }
        }

        private sealed class ThemeDefinition
        {
            public ThemeDefinition(string menuText, Color nonScaleNoteColor, Color nonScaleTextColor, Fretboard.FingerboardBackgroundKind preferredBackground, Color[] intervalNoteColors, Color[] intervalTextColors)
            {
                MenuText = menuText;
                NonScaleNoteColor = nonScaleNoteColor;
                NonScaleTextColor = nonScaleTextColor;
                PreferredBackground = preferredBackground;
                IntervalNoteColors = intervalNoteColors;
                IntervalTextColors = intervalTextColors;
            }

            public string MenuText { get; private set; }

            public Color NonScaleNoteColor { get; private set; }

            public Color NonScaleTextColor { get; private set; }

            public Fretboard.FingerboardBackgroundKind PreferredBackground { get; private set; }

            public Color[] IntervalNoteColors { get; private set; }

            public Color[] IntervalTextColors { get; private set; }
        }

        private sealed class BackgroundDefinition
        {
            public BackgroundDefinition(string menuText, Fretboard.FingerboardBackgroundKind backgroundKind)
            {
                MenuText = menuText;
                BackgroundKind = backgroundKind;
            }

            public string MenuText { get; private set; }

            public Fretboard.FingerboardBackgroundKind BackgroundKind { get; private set; }
        }

        private sealed class InstrumentCategory
        {
            public InstrumentCategory(string menuText, params Fretboard.TuningDefinition[] tunings)
            {
                MenuText = menuText;
                Tunings = tunings;
            }

            public string MenuText { get; private set; }

            public Fretboard.TuningDefinition[] Tunings { get; private set; }
        }

        private static readonly RootNoteOption[] ScaleRoots = new RootNoteOption[]
        {
            new RootNoteOption(Fretboard.Notes.C, "C", "C", "C"),
            new RootNoteOption(Fretboard.Notes.Db, "C#/Db", "C#", "Db"),
            new RootNoteOption(Fretboard.Notes.D, "D", "D", "D"),
            new RootNoteOption(Fretboard.Notes.Eb, "D#/Eb", "D#", "Eb"),
            new RootNoteOption(Fretboard.Notes.E, "E", "E", "E"),
            new RootNoteOption(Fretboard.Notes.F, "F", "F", "F"),
            new RootNoteOption(Fretboard.Notes.Gb, "F#/Gb", "F#", "Gb"),
            new RootNoteOption(Fretboard.Notes.G, "G", "G", "G"),
            new RootNoteOption(Fretboard.Notes.Ab, "G#/Ab", "G#", "Ab"),
            new RootNoteOption(Fretboard.Notes.A, "A", "A", "A"),
            new RootNoteOption(Fretboard.Notes.Bb, "A#/Bb", "A#", "Bb"),
            new RootNoteOption(Fretboard.Notes.B, "B", "B", "B")
        };

        private static readonly ScaleCategory[] ScaleCategories = new ScaleCategory[]
        {
            new ScaleCategory("&Common",
                new ScalePattern("Major (Ionian)", "Major (Ionian)", 0, 2, 4, 5, 7, 9, 11),
                new ScalePattern("Natural Minor (Aeolian)", "Natural Minor (Aeolian)", 0, 2, 3, 5, 7, 8, 10),
                new ScalePattern("Harmonic Minor", "Harmonic Minor", 0, 2, 3, 5, 7, 8, 11),
                new ScalePattern("Melodic Minor", "Melodic Minor", 0, 2, 3, 5, 7, 9, 11),
                new ScalePattern("Major Pentatonic", "Major Pentatonic", 0, 2, 4, 7, 9),
                new ScalePattern("Minor Pentatonic", "Minor Pentatonic", 0, 3, 5, 7, 10),
                new ScalePattern("Minor Blues", "Minor Blues", 0, 3, 5, 6, 7, 10),
                new ScalePattern("Major Blues", "Major Blues", 0, 2, 3, 4, 7, 9),
                new ScalePattern("Chromatic", "Chromatic", 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)),
            new ScaleCategory("&Modes",
                new ScalePattern("Dorian", "Dorian", 0, 2, 3, 5, 7, 9, 10),
                new ScalePattern("Phrygian", "Phrygian", 0, 1, 3, 5, 7, 8, 10),
                new ScalePattern("Lydian", "Lydian", 0, 2, 4, 6, 7, 9, 11),
                new ScalePattern("Mixolydian", "Mixolydian", 0, 2, 4, 5, 7, 9, 10),
                new ScalePattern("Locrian", "Locrian", 0, 1, 3, 5, 6, 8, 10),
                new ScalePattern("Phrygian Dominant", "Phrygian Dominant", 0, 1, 4, 5, 7, 8, 10),
                new ScalePattern("Lydian Dominant", "Lydian Dominant", 0, 2, 4, 6, 7, 9, 10)),
            new ScaleCategory("&Jazz && Symmetrical",
                new ScalePattern("Whole Tone", "Whole Tone", 0, 2, 4, 6, 8, 10),
                new ScalePattern("Diminished (Half-Whole)", "Diminished (Half-Whole)", 0, 1, 3, 4, 6, 7, 9, 10),
                new ScalePattern("Diminished (Whole-Half)", "Diminished (Whole-Half)", 0, 2, 3, 5, 6, 8, 9, 11),
                new ScalePattern("Altered", "Altered", 0, 1, 3, 4, 6, 8, 10),
                new ScalePattern("Bebop Dominant", "Bebop Dominant", 0, 2, 4, 5, 7, 9, 10, 11),
                new ScalePattern("Bebop Major", "Bebop Major", 0, 2, 4, 5, 7, 8, 9, 11),
                new ScalePattern("Prometheus", "Prometheus", 0, 2, 4, 6, 9, 10)),
            new ScaleCategory("E&xotic && Obscure",
                new ScalePattern("Double Harmonic Major (Byzantine)", "Double Harmonic Major (Byzantine)", 0, 1, 4, 5, 7, 8, 11),
                new ScalePattern("Hungarian Minor", "Hungarian Minor", 0, 2, 3, 6, 7, 8, 11),
                new ScalePattern("Hungarian Major", "Hungarian Major", 0, 3, 4, 6, 7, 9, 10),
                new ScalePattern("Neapolitan Minor", "Neapolitan Minor", 0, 1, 3, 5, 7, 8, 11),
                new ScalePattern("Neapolitan Major", "Neapolitan Major", 0, 1, 3, 5, 7, 9, 11),
                new ScalePattern("Enigmatic", "Enigmatic", 0, 1, 4, 6, 8, 10, 11),
                new ScalePattern("Persian", "Persian", 0, 1, 4, 5, 6, 8, 11),
                new ScalePattern("Romanian Minor", "Romanian Minor", 0, 2, 3, 6, 7, 9, 10),
                new ScalePattern("Ukrainian Dorian", "Ukrainian Dorian", 0, 2, 3, 6, 7, 9, 10),
                new ScalePattern("Hirajoshi", "Hirajoshi", 0, 2, 3, 7, 8),
                new ScalePattern("In Sen", "In Sen", 0, 1, 5, 7, 10),
                new ScalePattern("Iwato", "Iwato", 0, 1, 5, 6, 10))
        };

        private static readonly ThemeDefinition[] ScaleThemes = new ThemeDefinition[]
        {
            new ThemeDefinition("&Classic Monochrome", Color.Black, Color.White, Fretboard.FingerboardBackgroundKind.Rosewood,
                CreateColors(Color.Black, Color.DimGray, Color.Gray, Color.Silver, Color.Black, Color.DimGray, Color.Gray, Color.Silver, Color.Black, Color.DimGray, Color.Gray, Color.Silver),
                CreateColors(Color.White, Color.White, Color.White, Color.Black, Color.White, Color.White, Color.White, Color.Black, Color.White, Color.White, Color.White, Color.Black)),
            new ThemeDefinition("&Rainbow Spectrum", Color.Gainsboro, Color.Black, Fretboard.FingerboardBackgroundKind.Charcoal,
                CreateColors(Color.FromArgb(220, 53, 69), Color.FromArgb(253, 126, 20), Color.FromArgb(255, 193, 7), Color.FromArgb(40, 167, 69), Color.FromArgb(23, 162, 184), Color.FromArgb(0, 123, 255), Color.FromArgb(111, 66, 193), Color.FromArgb(232, 62, 140), Color.FromArgb(214, 51, 132), Color.FromArgb(102, 16, 242), Color.FromArgb(13, 110, 253), Color.FromArgb(32, 201, 151)),
                CreateColors(Color.White, Color.Black, Color.Black, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.Black)),
            new ThemeDefinition("&Ocean", Color.FromArgb(227, 242, 253), Color.FromArgb(13, 71, 161), Fretboard.FingerboardBackgroundKind.MidnightBlue,
                CreateColors(Color.FromArgb(1, 87, 155), Color.FromArgb(2, 119, 189), Color.FromArgb(3, 155, 229), Color.FromArgb(0, 172, 193), Color.FromArgb(0, 188, 212), Color.FromArgb(38, 198, 218), Color.FromArgb(77, 208, 225), Color.FromArgb(38, 166, 154), Color.FromArgb(0, 137, 123), Color.FromArgb(0, 105, 92), Color.FromArgb(13, 71, 161), Color.FromArgb(25, 118, 210)),
                CreateColors(Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.Black, Color.Black, Color.White, Color.White, Color.White, Color.White, Color.White)),
            new ThemeDefinition("&Forest", Color.FromArgb(232, 245, 233), Color.FromArgb(27, 94, 32), Fretboard.FingerboardBackgroundKind.SageGreen,
                CreateColors(Color.FromArgb(27, 94, 32), Color.FromArgb(46, 125, 50), Color.FromArgb(56, 142, 60), Color.FromArgb(67, 160, 71), Color.FromArgb(104, 159, 56), Color.FromArgb(124, 179, 66), Color.FromArgb(158, 157, 36), Color.FromArgb(85, 139, 47), Color.FromArgb(51, 105, 30), Color.FromArgb(27, 94, 32), Color.FromArgb(46, 125, 50), Color.FromArgb(67, 160, 71)),
                CreateColors(Color.White, Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.Black, Color.White, Color.White, Color.White, Color.White, Color.White)),
            new ThemeDefinition("&Sunset", Color.FromArgb(255, 243, 224), Color.FromArgb(191, 54, 12), Fretboard.FingerboardBackgroundKind.PauFerro,
                CreateColors(Color.FromArgb(191, 54, 12), Color.FromArgb(216, 67, 21), Color.FromArgb(230, 81, 0), Color.FromArgb(245, 124, 0), Color.FromArgb(251, 140, 0), Color.FromArgb(255, 167, 38), Color.FromArgb(255, 183, 77), Color.FromArgb(244, 81, 30), Color.FromArgb(233, 30, 99), Color.FromArgb(216, 27, 96), Color.FromArgb(173, 20, 87), Color.FromArgb(106, 27, 154)),
                CreateColors(Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.Black, Color.Black, Color.White, Color.White, Color.White, Color.White, Color.White)),
            new ThemeDefinition("&Fire", Color.FromArgb(251, 233, 231), Color.FromArgb(183, 28, 28), Fretboard.FingerboardBackgroundKind.Burgundy,
                CreateColors(Color.FromArgb(183, 28, 28), Color.FromArgb(198, 40, 40), Color.FromArgb(211, 47, 47), Color.FromArgb(229, 57, 53), Color.FromArgb(244, 67, 54), Color.FromArgb(255, 87, 34), Color.FromArgb(255, 112, 67), Color.FromArgb(255, 152, 0), Color.FromArgb(255, 171, 64), Color.FromArgb(255, 193, 7), Color.FromArgb(255, 213, 79), Color.FromArgb(255, 235, 59)),
                CreateColors(Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black)),
            new ThemeDefinition("&Ice", Color.FromArgb(232, 245, 253), Color.FromArgb(38, 50, 56), Fretboard.FingerboardBackgroundKind.Ebony,
                CreateColors(Color.FromArgb(38, 50, 56), Color.FromArgb(55, 71, 79), Color.FromArgb(69, 90, 100), Color.FromArgb(84, 110, 122), Color.FromArgb(96, 125, 139), Color.FromArgb(120, 144, 156), Color.FromArgb(144, 164, 174), Color.FromArgb(176, 190, 197), Color.FromArgb(207, 216, 220), Color.FromArgb(129, 212, 250), Color.FromArgb(79, 195, 247), Color.FromArgb(41, 182, 246)),
                CreateColors(Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.White)),
            new ThemeDefinition("&Pastel", Color.FromArgb(250, 250, 250), Color.FromArgb(97, 97, 97), Fretboard.FingerboardBackgroundKind.Maple,
                CreateColors(Color.FromArgb(244, 143, 177), Color.FromArgb(206, 147, 216), Color.FromArgb(179, 157, 219), Color.FromArgb(159, 168, 218), Color.FromArgb(144, 202, 249), Color.FromArgb(129, 212, 250), Color.FromArgb(128, 222, 234), Color.FromArgb(165, 214, 167), Color.FromArgb(197, 225, 165), Color.FromArgb(255, 245, 157), Color.FromArgb(255, 224, 178), Color.FromArgb(255, 204, 188)),
                CreateColors(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black)),
            new ThemeDefinition("&Neon", Color.FromArgb(33, 33, 33), Color.White, Fretboard.FingerboardBackgroundKind.Charcoal,
                CreateColors(Color.FromArgb(57, 255, 20), Color.FromArgb(0, 255, 255), Color.FromArgb(255, 0, 255), Color.FromArgb(255, 255, 0), Color.FromArgb(255, 105, 180), Color.FromArgb(0, 191, 255), Color.FromArgb(124, 252, 0), Color.FromArgb(255, 140, 0), Color.FromArgb(186, 85, 211), Color.FromArgb(255, 20, 147), Color.FromArgb(0, 250, 154), Color.FromArgb(173, 255, 47)),
                CreateColors(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black)),
            new ThemeDefinition("&Earth Tones", Color.FromArgb(245, 240, 230), Color.FromArgb(93, 64, 55), Fretboard.FingerboardBackgroundKind.Walnut,
                CreateColors(Color.FromArgb(78, 52, 46), Color.FromArgb(93, 64, 55), Color.FromArgb(109, 76, 65), Color.FromArgb(121, 85, 72), Color.FromArgb(141, 110, 99), Color.FromArgb(161, 136, 127), Color.FromArgb(188, 170, 164), Color.FromArgb(160, 82, 45), Color.FromArgb(205, 133, 63), Color.FromArgb(139, 69, 19), Color.FromArgb(112, 128, 144), Color.FromArgb(85, 107, 47)),
                CreateColors(Color.White, Color.White, Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.White, Color.Black, Color.White, Color.White, Color.White)),
            new ThemeDefinition("&Royal", Color.FromArgb(245, 245, 255), Color.FromArgb(49, 27, 146), Fretboard.FingerboardBackgroundKind.MidnightBlue,
                CreateColors(Color.FromArgb(49, 27, 146), Color.FromArgb(69, 39, 160), Color.FromArgb(81, 45, 168), Color.FromArgb(94, 53, 177), Color.FromArgb(106, 27, 154), Color.FromArgb(123, 31, 162), Color.FromArgb(142, 36, 170), Color.FromArgb(171, 71, 188), Color.FromArgb(186, 104, 200), Color.FromArgb(255, 215, 64), Color.FromArgb(255, 202, 40), Color.FromArgb(255, 179, 0)),
                CreateColors(Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.Black, Color.Black, Color.Black, Color.Black))
        };

        private static readonly BackgroundDefinition[] Backgrounds = new BackgroundDefinition[]
        {
            new BackgroundDefinition("&Rosewood", Fretboard.FingerboardBackgroundKind.Rosewood),
            new BackgroundDefinition("&Ebony", Fretboard.FingerboardBackgroundKind.Ebony),
            new BackgroundDefinition("&Maple", Fretboard.FingerboardBackgroundKind.Maple),
            new BackgroundDefinition("&Walnut", Fretboard.FingerboardBackgroundKind.Walnut),
            new BackgroundDefinition("Pau &Ferro", Fretboard.FingerboardBackgroundKind.PauFerro),
            new BackgroundDefinition("&Charcoal", Fretboard.FingerboardBackgroundKind.Charcoal),
            new BackgroundDefinition("&Midnight Blue", Fretboard.FingerboardBackgroundKind.MidnightBlue),
            new BackgroundDefinition("S&age Green", Fretboard.FingerboardBackgroundKind.SageGreen),
            new BackgroundDefinition("B&urgundy", Fretboard.FingerboardBackgroundKind.Burgundy),
            new BackgroundDefinition("Sands&tone", Fretboard.FingerboardBackgroundKind.Sandstone)
        };

        private static readonly InstrumentCategory[] InstrumentCategories = new InstrumentCategory[]
        {
            new InstrumentCategory("&Guitar",
                GetTuning("Guitar", "6-String Standard"),
                GetTuning("Guitar", "Drop D"),
                GetTuning("Guitar", "D Standard"),
                GetTuning("Guitar", "Open G"),
                GetTuning("Guitar", "Open D"),
                GetTuning("Guitar", "DADGAD"),
                GetTuning("Guitar", "Baritone B Standard"),
                GetTuning("Guitar", "7-String Standard"),
                GetTuning("Guitar", "7-String Drop A"),
                GetTuning("Guitar", "8-String Standard")),
            new InstrumentCategory("&Bass",
                GetTuning("Bass", "4-String Standard"),
                GetTuning("Bass", "Drop D"),
                GetTuning("Bass", "5-String Standard"),
                GetTuning("Bass", "5-String High C"),
                GetTuning("Bass", "6-String Standard")),
            new InstrumentCategory("&Ukulele",
                GetTuning("Ukulele", "Standard (GCEA)"),
                GetTuning("Ukulele", "D Tuning (ADF#B)"),
                GetTuning("Ukulele", "Baritone (DGBE)")),
            new InstrumentCategory("B&anjo",
                GetTuning("Banjo", "5-String Open G"),
                GetTuning("Banjo", "5-String Double C"),
                GetTuning("Banjo", "4-String Tenor")),
            new InstrumentCategory("&Orchestral Strings",
                GetTuning("Violin", "Standard"),
                GetTuning("Viola", "Standard"),
                GetTuning("Cello", "Standard"))
        };

        private static Color[] CreateColors(params Color[] colors)
        {
            return colors;
        }

        private static Fretboard.TuningDefinition GetTuning(string instrumentName, string tuningName)
        {
            for (int i = 0; i < Fretboard.CommonTunings.Length; i++)
            {
                Fretboard.TuningDefinition tuning = Fretboard.CommonTunings[i];

                if (tuning.InstrumentName == instrumentName && tuning.TuningName == tuningName)
                {
                    return tuning;
                }
            }

            throw new InvalidOperationException(string.Format("Unknown tuning: {0} - {1}", instrumentName, tuningName));
        }

        private Fretboard fretboard;
        private Button[] colorButtons;
        private Button[] textColorButtons;
        private ComboBox[] noteStyles;
        private Label[] noteLabels;
        private bool updatingNoteStyleSelectors;
        private ScaleSelection currentScaleSelection;
        private ThemeDefinition currentTheme;

        public MainForm()
        {
            InitializeComponent();

            Icon = AppIcon.CreateApplicationIcon();
            FormClosing += MainForm_FormClosing;

            fretboard = new Fretboard();

            fretboardProperties.SelectedObject = fretboard;
            fretboard.Changed += fretboard_Changed;
            currentTheme = ScaleThemes[0];

            colorButtons = new Button[12];
            colorButtons[0] = noteColor0;
            colorButtons[1] = noteColor1;
            colorButtons[2] = noteColor2;
            colorButtons[3] = noteColor3;
            colorButtons[4] = noteColor4;
            colorButtons[5] = noteColor5;
            colorButtons[6] = noteColor6;
            colorButtons[7] = noteColor7;
            colorButtons[8] = noteColor8;
            colorButtons[9] = noteColor9;
            colorButtons[10] = noteColor10;
            colorButtons[11] = noteColor11;

            textColorButtons = new Button[12];
            textColorButtons[0] = noteTextColor0;
            textColorButtons[1] = noteTextColor1;
            textColorButtons[2] = noteTextColor2;
            textColorButtons[3] = noteTextColor3;
            textColorButtons[4] = noteTextColor4;
            textColorButtons[5] = noteTextColor5;
            textColorButtons[6] = noteTextColor6;
            textColorButtons[7] = noteTextColor7;
            textColorButtons[8] = noteTextColor8;
            textColorButtons[9] = noteTextColor9;
            textColorButtons[10] = noteTextColor10;
            textColorButtons[11] = noteTextColor11;

            noteStyles = new ComboBox[12];
            noteStyles[0] = noteStyle0;
            noteStyles[1] = noteStyle1;
            noteStyles[2] = noteStyle2;
            noteStyles[3] = noteStyle3;
            noteStyles[4] = noteStyle4;
            noteStyles[5] = noteStyle5;
            noteStyles[6] = noteStyle6;
            noteStyles[7] = noteStyle7;
            noteStyles[8] = noteStyle8;
            noteStyles[9] = noteStyle9;
            noteStyles[10] = noteStyle10;
            noteStyles[11] = noteStyle11;

            noteLabels = new Label[12];
            noteLabels[0] = noteLabel0;
            noteLabels[1] = noteLabel1;
            noteLabels[2] = noteLabel2;
            noteLabels[3] = noteLabel3;
            noteLabels[4] = noteLabel4;
            noteLabels[5] = noteLabel5;
            noteLabels[6] = noteLabel6;
            noteLabels[7] = noteLabel7;
            noteLabels[8] = noteLabel8;
            noteLabels[9] = noteLabel9;
            noteLabels[10] = noteLabel10;
            noteLabels[11] = noteLabel11;


            for (int i = 0; i < 12; i++)
            {
                noteStyles[i].SelectedIndex = 0;
            }

            InitializeFileMenu();
            InitializeInstrumentMenu();
            InitializeScaleMenu();
            InitializeThemeMenu();
            InitializeBackgroundMenu();
            ApplyBaseThemeColors(currentTheme);
            UpdateNoteLabels();
            ConfigureControlTips();

            if (!TryRestoreLastRenderedConfiguration())
            {
                fretboardImage.Image = fretboard.Render();
            }
        }

        private void fretboard_Changed(object sender, EventArgs e)
        {
            UpdateNoteLabels();
            UpdateStatusText();
            fretboardProperties.Refresh();
            fretboardImage.Image = fretboard.Render();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveLastRenderedConfiguration();
        }

        private void InitializeScaleMenu()
        {
            ToolStripMenuItem scalesMenuItem = new ToolStripMenuItem("&Scales");
            ToolStripMenuItem clearPatternMenuItem = new ToolStripMenuItem("&Clear Pattern");

            clearPatternMenuItem.Click += clearPatternMenuItem_Click;
            scalesMenuItem.DropDownItems.Add(clearPatternMenuItem);
            scalesMenuItem.DropDownItems.Add(new ToolStripSeparator());

            for (int i = 0; i < ScaleCategories.Length; i++)
            {
                ScaleCategory category = ScaleCategories[i];
                ToolStripMenuItem categoryMenuItem = new ToolStripMenuItem(category.MenuText);

                for (int j = 0; j < category.Patterns.Length; j++)
                {
                    ScalePattern pattern = category.Patterns[j];
                    ToolStripMenuItem patternMenuItem = new ToolStripMenuItem(pattern.MenuText);

                    for (int k = 0; k < ScaleRoots.Length; k++)
                    {
                        RootNoteOption root = ScaleRoots[k];
                        ToolStripMenuItem rootMenuItem = new ToolStripMenuItem(root.MenuText);
                        rootMenuItem.Tag = new ScaleSelection(root, pattern);
                        rootMenuItem.Click += scaleRootMenuItem_Click;
                        patternMenuItem.DropDownItems.Add(rootMenuItem);
                    }

                    categoryMenuItem.DropDownItems.Add(patternMenuItem);
                }

                scalesMenuItem.DropDownItems.Add(categoryMenuItem);
            }

            menuStrip1.Items.Add(scalesMenuItem);
        }

        private void InitializeFileMenu()
        {
            ToolStripMenuItem resetViewToolStripMenuItem = new ToolStripMenuItem("&Reset View");
            ToolStripMenuItem copyTitleToolStripMenuItem = new ToolStripMenuItem("Copy &Title");
            ToolStripMenuItem toggleAccidentalsToolStripMenuItem = new ToolStripMenuItem("Toggle S&harps/Flats");
            ToolStripMenuItem exitToolStripMenuItem = new ToolStripMenuItem("E&xit");

            resetViewToolStripMenuItem.Click += resetViewToolStripMenuItem_Click;
            copyTitleToolStripMenuItem.Click += copyTitleToolStripMenuItem_Click;
            toggleAccidentalsToolStripMenuItem.Click += toggleAccidentalsToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            fileToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            fileToolStripMenuItem.DropDownItems.Add(resetViewToolStripMenuItem);
            fileToolStripMenuItem.DropDownItems.Add(copyTitleToolStripMenuItem);
            fileToolStripMenuItem.DropDownItems.Add(toggleAccidentalsToolStripMenuItem);
            fileToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            fileToolStripMenuItem.DropDownItems.Add(exitToolStripMenuItem);
        }

        private void InitializeThemeMenu()
        {
            ToolStripMenuItem themesMenuItem = new ToolStripMenuItem("&Themes");

            for (int i = 0; i < ScaleThemes.Length; i++)
            {
                ToolStripMenuItem themeMenuItem = new ToolStripMenuItem(ScaleThemes[i].MenuText);
                themeMenuItem.Tag = ScaleThemes[i];
                themeMenuItem.Click += themeMenuItem_Click;
                themesMenuItem.DropDownItems.Add(themeMenuItem);
            }

            menuStrip1.Items.Add(themesMenuItem);
        }

        private void InitializeBackgroundMenu()
        {
            ToolStripMenuItem backgroundsMenuItem = new ToolStripMenuItem("&Backgrounds");

            for (int i = 0; i < Backgrounds.Length; i++)
            {
                ToolStripMenuItem backgroundMenuItem = new ToolStripMenuItem(Backgrounds[i].MenuText);
                backgroundMenuItem.Tag = Backgrounds[i];
                backgroundMenuItem.Click += backgroundMenuItem_Click;
                backgroundsMenuItem.DropDownItems.Add(backgroundMenuItem);
            }

            menuStrip1.Items.Add(backgroundsMenuItem);
        }

        private void InitializeInstrumentMenu()
        {
            ToolStripMenuItem instrumentsMenuItem = new ToolStripMenuItem("&Instruments");

            for (int i = 0; i < InstrumentCategories.Length; i++)
            {
                InstrumentCategory category = InstrumentCategories[i];
                ToolStripMenuItem categoryMenuItem = new ToolStripMenuItem(category.MenuText);

                for (int j = 0; j < category.Tunings.Length; j++)
                {
                    Fretboard.TuningDefinition tuning = category.Tunings[j];
                    ToolStripMenuItem tuningMenuItem = new ToolStripMenuItem(tuning.TuningName);
                    tuningMenuItem.Tag = tuning;
                    tuningMenuItem.Click += tuningMenuItem_Click;
                    categoryMenuItem.DropDownItems.Add(tuningMenuItem);
                }

                instrumentsMenuItem.DropDownItems.Add(categoryMenuItem);
            }

            menuStrip1.Items.Add(instrumentsMenuItem);
        }

        private void ApplyScalePattern(ScaleSelection selection)
        {
            Dictionary<int, int> scaleNoteIndexes = new Dictionary<int, int>();
            int rootIndex = (int)selection.Root.Note;

            currentScaleSelection = selection;
            fretboard.RootNoteIndex = rootIndex;

            for (int i = 0; i < selection.Pattern.Intervals.Length; i++)
            {
                int noteIndex = (rootIndex + selection.Pattern.Intervals[i]) % 12;

                if (!scaleNoteIndexes.ContainsKey(noteIndex))
                {
                    scaleNoteIndexes.Add(noteIndex, i);
                }
            }

            updatingNoteStyleSelectors = true;
            fretboard.Changed -= fretboard_Changed;
            try
            {
                ApplyBaseThemeColors(currentTheme);

                for (int i = 0; i < 12; i++)
                {
                    Fretboard.NoteStyle style = Fretboard.NoteStyle.None;
                    int intervalIndex;

                    if (scaleNoteIndexes.TryGetValue(i, out intervalIndex))
                    {
                        style = i == rootIndex ? Fretboard.NoteStyle.Square : Fretboard.NoteStyle.Circle;
                        fretboard.NoteColors[i] = currentTheme.IntervalNoteColors[intervalIndex % currentTheme.IntervalNoteColors.Length];
                        fretboard.NoteTextColors[i] = currentTheme.IntervalTextColors[intervalIndex % currentTheme.IntervalTextColors.Length];
                    }

                    fretboard.NoteStyles[i] = style;
                    noteStyles[i].SelectedIndex = (int)style;
                    colorButtons[i].BackColor = fretboard.NoteColors[i];
                    textColorButtons[i].BackColor = fretboard.NoteTextColors[i];
                }

                fretboard.Title = string.Format("{0} {1}", GetRootTitle(selection.Root), selection.Pattern.TitleText);
            }
            finally
            {
                fretboard.Changed += fretboard_Changed;
                updatingNoteStyleSelectors = false;
            }

            fretboardImage.Image = fretboard.Render();
        }

        private string GetRootTitle(RootNoteOption root)
        {
            return fretboard.UseSharps ? root.SharpText : root.FlatText;
        }

        private void ClearPattern()
        {
            currentScaleSelection = null;
            fretboard.RootNoteIndex = -1;
            updatingNoteStyleSelectors = true;
            fretboard.Changed -= fretboard_Changed;
            try
            {
                ApplyBaseThemeColors(currentTheme);

                for (int i = 0; i < 12; i++)
                {
                    fretboard.NoteStyles[i] = Fretboard.NoteStyle.None;
                    noteStyles[i].SelectedIndex = 0;
                }

                fretboard.Title = "Fretboard";
            }
            finally
            {
                fretboard.Changed += fretboard_Changed;
                updatingNoteStyleSelectors = false;
            }

            fretboardImage.Image = fretboard.Render();
        }

        private void ApplyBaseThemeColors(ThemeDefinition theme)
        {
            for (int i = 0; i < 12; i++)
            {
                fretboard.NoteColors[i] = theme.NonScaleNoteColor;
                fretboard.NoteTextColors[i] = theme.NonScaleTextColor;
                colorButtons[i].BackColor = theme.NonScaleNoteColor;
                textColorButtons[i].BackColor = theme.NonScaleTextColor;
            }
        }

        private void ApplyTheme(ThemeDefinition theme)
        {
            currentTheme = theme;
            fretboard.FingerboardBackground = theme.PreferredBackground;
            fretboard.TitleColor = theme.NonScaleTextColor;
            fretboard.NoteOutlineColor = ControlPaint.Dark(theme.NonScaleNoteColor);
            fretboard.RootHighlightColor = theme.IntervalNoteColors[0];

            if (currentScaleSelection != null)
            {
                ApplyScalePattern(currentScaleSelection);
            }
            else
            {
                ApplyBaseThemeColors(theme);
                fretboardImage.Image = fretboard.Render();
            }
        }

        private void backgroundMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            BackgroundDefinition background = menuItem == null ? null : menuItem.Tag as BackgroundDefinition;

            if (background == null)
            {
                return;
            }

            fretboard.FingerboardBackground = background.BackgroundKind;
            fretboardImage.Image = fretboard.Render();
        }

        private void ApplyInstrumentTuning(Fretboard.TuningDefinition tuning)
        {
            fretboard.ApplyTuning(tuning);
            fretboard.Title = string.Format("{0} - {1}", tuning.InstrumentName, tuning.TuningName);
            fretboardProperties.Refresh();

            if (currentScaleSelection != null)
            {
                ApplyScalePattern(currentScaleSelection);
            }
            else
            {
                fretboardImage.Image = fretboard.Render();
            }
        }

        private int GetStyleComboIndex(ComboBox sender)
        {
            for (int i = 0; i < 12; i++)
            {
                if (noteStyles[i] == sender)
                {
                    return i;
                }
            }
            return -1;
        }

        private int GetColorButtonIndex(Button sender)
        {
            for (int i = 0; i < 12; i++)
            {
                if (colorButtons[i] == sender)
                {
                    return i;
                }
            }
            return -1;
        }

        private int GetTextColorButtonIndex(Button sender)
        {
            for (int i = 0; i < 12; i++)
            {
                if (textColorButtons[i] == sender)
                {
                    return i;
                }
            }
            return -1;
        }

        private void noteColor_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            colorDialog1.Color = button.BackColor;
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                button.BackColor = colorDialog1.Color;
                int index = GetColorButtonIndex(button);
                if (index != -1)
                {
                    fretboard.NoteColors[GetColorButtonIndex(button)] = colorDialog1.Color;
                }
                else
                {
                    fretboard.NoteTextColors[GetTextColorButtonIndex(button)] = colorDialog1.Color;
                }
                fretboardImage.Image = fretboard.Render();
            }
        }

        private void noteStyle_Changed(object sender, EventArgs e)
        {
            if (updatingNoteStyleSelectors)
            {
                return;
            }

            fretboard.NoteStyles[GetStyleComboIndex((ComboBox)sender)] = (Fretboard.NoteStyle)((ComboBox)sender).SelectedIndex;
            fretboardImage.Image = fretboard.Render();
        }

        private void resetViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fretboard != null)
            {
                fretboard.Changed -= fretboard_Changed;
            }

            currentTheme = ScaleThemes[0];
            fretboard = new Fretboard();
            fretboardProperties.SelectedObject = fretboard;
            fretboard.Changed += fretboard_Changed;
            currentScaleSelection = null;
            ApplyTheme(currentTheme);

            for (int i = 0; i < 12; i++)
            {
                noteStyles[i].SelectedIndex = 0;
            }

            UpdateNoteLabels();
            UpdateStatusText();
            fretboardImage.Image = fretboard.Render();
        }

        private void copyTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(fretboard.Title))
            {
                Clipboard.SetText(fretboard.Title);
            }
        }

        private void toggleAccidentalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fretboard.UseSharps = !fretboard.UseSharps;

            if (currentScaleSelection != null)
            {
                ApplyScalePattern(currentScaleSelection);
            }
            else
            {
                UpdateNoteLabels();
                fretboardImage.Image = fretboard.Render();
            }
        }

        private void scaleRootMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            ScaleSelection selection = menuItem == null ? null : menuItem.Tag as ScaleSelection;

            if (selection == null)
            {
                return;
            }

            ApplyScalePattern(selection);
        }

        private void clearPatternMenuItem_Click(object sender, EventArgs e)
        {
            ClearPattern();
        }

        private void themeMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            ThemeDefinition theme = menuItem == null ? null : menuItem.Tag as ThemeDefinition;

            if (theme == null)
            {
                return;
            }

            ApplyTheme(theme);
        }

        private void tuningMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            Fretboard.TuningDefinition tuning = menuItem == null ? null : menuItem.Tag as Fretboard.TuningDefinition;

            if (tuning == null)
            {
                return;
            }

            ApplyInstrumentTuning(tuning);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Bitmap image = fretboard.Render();
                image.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }

        private void renderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fretboardImage.Image = fretboard.Render();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Bitmap image = fretboard.Render();
                image.Save(saveFileDialog.FileName, ImageFormat.Png);
            }
        }

        private bool TryRestoreLastRenderedConfiguration()
        {
            string serializedConfiguration = Properties.Settings.Default.LastRenderedConfiguration;

            if (string.IsNullOrWhiteSpace(serializedConfiguration))
            {
                return false;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RenderConfiguration));

                using (StringReader reader = new StringReader(serializedConfiguration))
                {
                    RenderConfiguration configuration = serializer.Deserialize(reader) as RenderConfiguration;

                    if (configuration == null)
                    {
                        return false;
                    }

                    ApplyConfiguration(configuration);
                    fretboardProperties.Refresh();
                    fretboardImage.Image = fretboard.Render();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void SaveLastRenderedConfiguration()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RenderConfiguration));

                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, CaptureConfiguration());
                    Properties.Settings.Default.LastRenderedConfiguration = writer.ToString();
                    Properties.Settings.Default.Save();
                }
            }
            catch
            {
            }
        }

        private RenderConfiguration CaptureConfiguration()
        {
            RenderConfiguration configuration = new RenderConfiguration();

            configuration.Title = fretboard.Title;
            configuration.Frets = fretboard.Frets;
            configuration.TitleFont = SerializeFont(fretboard.TitleFont);
            configuration.NoteFont = SerializeFont(fretboard.NoteFont);
            configuration.FretboardLength = fretboard.FretboardLength;
            configuration.StringSpacing = fretboard.StringSpacing;
            configuration.Padding = fretboard.Padding;
            configuration.NutWidth = fretboard.NutWidth;
            configuration.FretWidth = fretboard.FretWidth;
            configuration.StringWidth = fretboard.StringWidth;
            configuration.DisplayDots = fretboard.DisplayDots;
            configuration.DotWidth = fretboard.DotWidth;
            configuration.NoteWidth = fretboard.NoteWidth;
            configuration.BackgroundColorArgb = fretboard.BackgroundColor.ToArgb();
            configuration.FretColorArgb = fretboard.FretColor.ToArgb();
            configuration.DotColorArgb = fretboard.DotColor.ToArgb();
            configuration.StringColorArgb = fretboard.StringColor.ToArgb();
            configuration.NutColorArgb = fretboard.NutColor.ToArgb();
            configuration.TitleColorArgb = fretboard.TitleColor.ToArgb();
            configuration.NoteOutlineColorArgb = fretboard.NoteOutlineColor.ToArgb();
            configuration.NoteOutlineWidth = fretboard.NoteOutlineWidth;
            configuration.ShowFretNumbers = fretboard.ShowFretNumbers;
            configuration.RootHighlightColorArgb = fretboard.RootHighlightColor.ToArgb();
            configuration.FingerboardBackground = (int)fretboard.FingerboardBackground;
            configuration.UseSharps = fretboard.UseSharps;
            configuration.RootNoteIndex = fretboard.RootNoteIndex;
            configuration.BaseNotes = ToIntArray(fretboard.BaseNotes.Select(note => (int)note).ToArray());
            configuration.NoteStyles = ToIntArray(fretboard.NoteStyles.Select(style => (int)style).ToArray());
            configuration.NoteColorsArgb = fretboard.NoteColors.Select(color => color.ToArgb()).ToArray();
            configuration.NoteTextColorsArgb = fretboard.NoteTextColors.Select(color => color.ToArgb()).ToArray();

            return configuration;
        }

        private void ApplyConfiguration(RenderConfiguration configuration)
        {
            updatingNoteStyleSelectors = true;
            currentScaleSelection = null;
            fretboard.Changed -= fretboard_Changed;

            try
            {
                if (configuration.BaseNotes != null && configuration.BaseNotes.Length > 0)
                {
                    fretboard.SetBaseNotes(configuration.BaseNotes.Select(note => (Fretboard.Notes)note).ToArray());
                }

                fretboard.Frets = configuration.Frets;
                fretboard.Title = string.IsNullOrWhiteSpace(configuration.Title) ? "Fretboard" : configuration.Title;
                fretboard.TitleFont = DeserializeFont(configuration.TitleFont, fretboard.TitleFont);
                fretboard.NoteFont = DeserializeFont(configuration.NoteFont, fretboard.NoteFont);
                fretboard.FretboardLength = configuration.FretboardLength;
                fretboard.StringSpacing = configuration.StringSpacing;
                fretboard.Padding = configuration.Padding;
                fretboard.NutWidth = configuration.NutWidth;
                fretboard.FretWidth = configuration.FretWidth;
                fretboard.StringWidth = configuration.StringWidth;
                fretboard.DisplayDots = configuration.DisplayDots;
                fretboard.DotWidth = configuration.DotWidth;
                fretboard.NoteWidth = configuration.NoteWidth;
                fretboard.BackgroundColor = Color.FromArgb(configuration.BackgroundColorArgb);
                fretboard.FretColor = Color.FromArgb(configuration.FretColorArgb);
                fretboard.DotColor = Color.FromArgb(configuration.DotColorArgb);
                fretboard.StringColor = Color.FromArgb(configuration.StringColorArgb);
                fretboard.NutColor = configuration.NutColorArgb == 0 ? fretboard.NutColor : Color.FromArgb(configuration.NutColorArgb);
                fretboard.TitleColor = configuration.TitleColorArgb == 0 ? fretboard.TitleColor : Color.FromArgb(configuration.TitleColorArgb);
                fretboard.NoteOutlineColor = configuration.NoteOutlineColorArgb == 0 ? fretboard.NoteOutlineColor : Color.FromArgb(configuration.NoteOutlineColorArgb);
                fretboard.NoteOutlineWidth = configuration.NoteOutlineWidth <= 0 ? fretboard.NoteOutlineWidth : configuration.NoteOutlineWidth;
                fretboard.ShowFretNumbers = configuration.ShowFretNumbers;
                fretboard.RootHighlightColor = configuration.RootHighlightColorArgb == 0 ? fretboard.RootHighlightColor : Color.FromArgb(configuration.RootHighlightColorArgb);
                fretboard.FingerboardBackground = Enum.IsDefined(typeof(Fretboard.FingerboardBackgroundKind), configuration.FingerboardBackground)
                    ? (Fretboard.FingerboardBackgroundKind)configuration.FingerboardBackground
                    : fretboard.FingerboardBackground;
                fretboard.UseSharps = configuration.UseSharps;
                fretboard.RootNoteIndex = configuration.RootNoteIndex;

                ApplyArrayConfiguration(configuration);
            }
            finally
            {
                fretboard.Changed += fretboard_Changed;
                updatingNoteStyleSelectors = false;
            }
        }

        private void ApplyArrayConfiguration(RenderConfiguration configuration)
        {
            int[] savedNoteStyles = configuration.NoteStyles ?? new int[0];
            int[] savedNoteColors = configuration.NoteColorsArgb ?? new int[0];
            int[] savedNoteTextColors = configuration.NoteTextColorsArgb ?? new int[0];

            for (int i = 0; i < 12; i++)
            {
                Fretboard.NoteStyle style = i < savedNoteStyles.Length
                    ? (Fretboard.NoteStyle)Math.Max(0, Math.Min(3, savedNoteStyles[i]))
                    : Fretboard.NoteStyle.None;
                Color noteColor = i < savedNoteColors.Length ? Color.FromArgb(savedNoteColors[i]) : currentTheme.NonScaleNoteColor;
                Color noteTextColor = i < savedNoteTextColors.Length ? Color.FromArgb(savedNoteTextColors[i]) : currentTheme.NonScaleTextColor;

                fretboard.NoteStyles[i] = style;
                fretboard.NoteColors[i] = noteColor;
                fretboard.NoteTextColors[i] = noteTextColor;
                noteStyles[i].SelectedIndex = (int)style;
                colorButtons[i].BackColor = noteColor;
                textColorButtons[i].BackColor = noteTextColor;
            }

            UpdateNoteLabels();
            UpdateStatusText();
        }

        private void UpdateNoteLabels()
        {
            for (int i = 0; i < noteLabels.Length; i++)
            {
                noteLabels[i].Text = fretboard.UseSharps ? Fretboard.NotesSharped[i] : Fretboard.NotesFlatted[i];
                noteLabels[i].ForeColor = i == fretboard.RootNoteIndex ? fretboard.RootHighlightColor : SystemColors.ControlText;
            }
        }

        private void UpdateStatusText()
        {
            Text = string.Format("Fretman - {0} strings / {1} frets", fretboard.Strings, fretboard.Frets);
        }

        private void ConfigureControlTips()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(fretboardImage, "Rendered fretboard preview. Use File > Save to export PNG.");

            for (int i = 0; i < 12; i++)
            {
                string noteName = Fretboard.NotesFlatted[i];
                toolTip.SetToolTip(noteStyles[i], string.Format("Choose the shape used for {0}.", noteName));
                toolTip.SetToolTip(colorButtons[i], string.Format("Choose the fill color for {0}.", noteName));
                toolTip.SetToolTip(textColorButtons[i], string.Format("Choose the label color for {0}.", noteName));
            }
        }

        private static int[] ToIntArray(int[] values)
        {
            return values == null ? new int[0] : values;
        }

        private static string SerializeFont(Font font)
        {
            return TypeDescriptor.GetConverter(typeof(Font)).ConvertToInvariantString(font);
        }

        private static Font DeserializeFont(string serializedFont, Font fallback)
        {
            if (string.IsNullOrWhiteSpace(serializedFont))
            {
                return fallback;
            }

            try
            {
                return TypeDescriptor.GetConverter(typeof(Font)).ConvertFromInvariantString(serializedFont) as Font ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
