using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fretman
{
    public class Fretboard
    {
        private sealed class BackgroundPalette
        {
            public Color BaseColor { get; set; }

            public Color SecondaryColor { get; set; }

            public Color GrainColor { get; set; }

            public bool UseTexture { get; set; }

            public Color FretColor { get; set; }

            public Color NutColor { get; set; }

            public Color StringColor { get; set; }

            public Color DotColor { get; set; }
        }

        public sealed class IntervalPatternDefinition
        {
            public IntervalPatternDefinition(string menuText, string titleText, params int[] intervals)
            {
                MenuText = menuText;
                TitleText = titleText;
                Intervals = intervals;
            }

            public string MenuText { get; private set; }

            public string TitleText { get; private set; }

            public int[] Intervals { get; private set; }
        }

        public sealed class TuningDefinition
        {
            public TuningDefinition(string instrumentName, string tuningName, params Notes[] baseNotes)
            {
                InstrumentName = instrumentName;
                TuningName = tuningName;
                BaseNotes = baseNotes;
            }

            public string InstrumentName { get; private set; }

            public string TuningName { get; private set; }

            public Notes[] BaseNotes { get; private set; }
        }

        public static float[] FretFactors = { 0, 0.056126f, 0.109101f, 0.159104f, 0.206291f, 0.250847f, 0.292893f, 0.33258f, 0.370039f, 0.405396f, 0.438769f, 0.470268f, 0.5f, 
                                                     0.528063f, 0.554551f, 0.579552f, 0.60315f, 0.625423f, 0.646447f, 0.66629f, 0.68502f, 0.702698f, 0.719385f, 0.735134f, 0.75f };

        public static string[] NotesSharped = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B", "C" };
        public static string[] NotesFlatted = { "C", "Db", "D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B", "C" };

        public static readonly TuningDefinition[] CommonTunings = new TuningDefinition[]
        {
            new TuningDefinition("Guitar", "6-String Standard", Notes.E, Notes.A, Notes.D, Notes.G, Notes.B, Notes.E),
            new TuningDefinition("Guitar", "Drop D", Notes.D, Notes.A, Notes.D, Notes.G, Notes.B, Notes.E),
            new TuningDefinition("Guitar", "D Standard", Notes.D, Notes.G, Notes.C, Notes.F, Notes.A, Notes.D),
            new TuningDefinition("Guitar", "Open G", Notes.D, Notes.G, Notes.D, Notes.G, Notes.B, Notes.D),
            new TuningDefinition("Guitar", "Open D", Notes.D, Notes.A, Notes.D, Notes.Gb, Notes.A, Notes.D),
            new TuningDefinition("Guitar", "DADGAD", Notes.D, Notes.A, Notes.D, Notes.G, Notes.A, Notes.D),
            new TuningDefinition("Guitar", "Baritone B Standard", Notes.B, Notes.E, Notes.A, Notes.D, Notes.Gb, Notes.B),
            new TuningDefinition("Guitar", "7-String Standard", Notes.B, Notes.E, Notes.A, Notes.D, Notes.G, Notes.B, Notes.E),
            new TuningDefinition("Guitar", "7-String Drop A", Notes.A, Notes.E, Notes.A, Notes.D, Notes.G, Notes.B, Notes.E),
            new TuningDefinition("Guitar", "8-String Standard", Notes.Gb, Notes.B, Notes.E, Notes.A, Notes.D, Notes.G, Notes.B, Notes.E),
            new TuningDefinition("Bass", "4-String Standard", Notes.E, Notes.A, Notes.D, Notes.G),
            new TuningDefinition("Bass", "Drop D", Notes.D, Notes.A, Notes.D, Notes.G),
            new TuningDefinition("Bass", "5-String Standard", Notes.B, Notes.E, Notes.A, Notes.D, Notes.G),
            new TuningDefinition("Bass", "5-String High C", Notes.E, Notes.A, Notes.D, Notes.G, Notes.C),
            new TuningDefinition("Bass", "6-String Standard", Notes.B, Notes.E, Notes.A, Notes.D, Notes.G, Notes.C),
            new TuningDefinition("Ukulele", "Standard (GCEA)", Notes.G, Notes.C, Notes.E, Notes.A),
            new TuningDefinition("Ukulele", "D Tuning (ADF#B)", Notes.A, Notes.D, Notes.Gb, Notes.B),
            new TuningDefinition("Ukulele", "Baritone (DGBE)", Notes.D, Notes.G, Notes.B, Notes.E),
            new TuningDefinition("Banjo", "5-String Open G", Notes.G, Notes.D, Notes.G, Notes.B, Notes.D),
            new TuningDefinition("Banjo", "5-String Double C", Notes.G, Notes.C, Notes.G, Notes.C, Notes.D),
            new TuningDefinition("Banjo", "4-String Tenor", Notes.C, Notes.G, Notes.D, Notes.A),
            new TuningDefinition("Violin", "Standard", Notes.G, Notes.D, Notes.A, Notes.E),
            new TuningDefinition("Viola", "Standard", Notes.C, Notes.G, Notes.D, Notes.A),
            new TuningDefinition("Cello", "Standard", Notes.C, Notes.G, Notes.D, Notes.A)
        };

        public delegate void ChangeHandler(object sender, EventArgs e);

        public ChangeHandler Changed;

        public enum NoteStyle
        {
            None = 0,
            Square = 1,
            Circle = 2,
            Triangle = 3
        }

        public enum FingerboardBackgroundKind
        {
            Rosewood = 0,
            Ebony = 1,
            Maple = 2,
            Walnut = 3,
            PauFerro = 4,
            Charcoal = 5,
            MidnightBlue = 6,
            SageGreen = 7,
            Burgundy = 8,
            Sandstone = 9
        }

        public enum Notes
        {
            C = 0,
            Db = 1,
            D = 2,
            Eb = 3,
            E = 4,
            F = 5,
            Gb = 6,
            G = 7,
            Ab = 8,
            A = 9,
            Bb = 10,
            B = 11
        }

        private static readonly Dictionary<FingerboardBackgroundKind, Bitmap> BackgroundTextureCache = new Dictionary<FingerboardBackgroundKind, Bitmap>();

        /* Note Indexes
         * 0 = C, 
         * 1 = C#/Db, 
         * 2 = D
         * 3 = D#/Eb
         * 4 = E
         * 5 = F
         * 6 = F#/Gb
         * 7 = G
         * 8 = G#/Ab
         * 9 = A
         * 10 = Bb
         * 11 = B
         */

        private string _title = "Fretboard";
        [DisplayName("Title")]
        public string Title 
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    this.onChanged();
                }
            }
        }

        private float _topJustifyPadding = 24f;
        [DisplayName("Top Preview Padding")]
        public float TopJustifyPadding
        {
            get { return _topJustifyPadding; }
            set
            {
                if (value != _topJustifyPadding)
                {
                    _topJustifyPadding = Math.Max(0f, value);
                    this.onChanged();
                }
            }
        }

        private FingerboardBackgroundKind _fingerboardBackground = FingerboardBackgroundKind.Rosewood;
        [DisplayName("Fretboard Background")]
        public FingerboardBackgroundKind FingerboardBackground
        {
            get { return _fingerboardBackground; }
            set
            {
                if (value != _fingerboardBackground)
                {
                    _fingerboardBackground = value;
                    ApplyBackgroundPalette();
                    this.onChanged();
                }
            }
        }

        private Color _titleColor = Color.Black;
        [DisplayName("Title Color")]
        public Color TitleColor
        {
            get { return _titleColor; }
            set
            {
                if (value != _titleColor)
                {
                    _titleColor = value;
                    this.onChanged();
                }
            }
        }

        private Color _nutColor = Color.Black;
        [DisplayName("Nut Color")]
        public Color NutColor
        {
            get { return _nutColor; }
            set
            {
                if (value != _nutColor)
                {
                    _nutColor = value;
                    this.onChanged();
                }
            }
        }

        private short _strings = 6;
        [DisplayName("String Count")]
        public short Strings
        {
            get { return _strings; }
            set
            {
                if (value != _strings)
                {
                    if (value < 1 || value > 12)
                    {
                        throw new ArgumentOutOfRangeException("String count must be between 1 and 12.");
                    }
                    _strings = value;
                    _baseNotes = BuildDefaultTuning(_strings);
                    this.onChanged();
                }
            }
        }

        private Color _noteOutlineColor = Color.FromArgb(80, 0, 0, 0);
        [DisplayName("Note Outline Color")]
        public Color NoteOutlineColor
        {
            get { return _noteOutlineColor; }
            set
            {
                if (value != _noteOutlineColor)
                {
                    _noteOutlineColor = value;
                    this.onChanged();
                }
            }
        }

        private float _noteOutlineWidth = 2f;
        [DisplayName("Note Outline Width")]
        public float NoteOutlineWidth
        {
            get { return _noteOutlineWidth; }
            set
            {
                if (value != _noteOutlineWidth)
                {
                    _noteOutlineWidth = value;
                    this.onChanged();
                }
            }
        }

        private bool _showFretNumbers = true;
        [DisplayName("Show Fret Numbers")]
        public bool ShowFretNumbers
        {
            get { return _showFretNumbers; }
            set
            {
                if (value != _showFretNumbers)
                {
                    _showFretNumbers = value;
                    this.onChanged();
                }
            }
        }

        private Color _rootHighlightColor = Color.FromArgb(255, 255, 193, 7);
        [DisplayName("Root Highlight Color")]
        public Color RootHighlightColor
        {
            get { return _rootHighlightColor; }
            set
            {
                if (value != _rootHighlightColor)
                {
                    _rootHighlightColor = value;
                    this.onChanged();
                }
            }
        }

        private short _frets = 24;
        [DisplayName("Fret Count")]
        public short Frets
        {
            get
            {
                return _frets;
            }

            set
            {
                if (value != _frets)
                {
                    if (value < 1)
                    {
                        throw new ArgumentOutOfRangeException("Minimum fret count is one (1).");
                    }
                    if (value > 24)
                    {
                        throw new ArgumentOutOfRangeException("Maximum fret count is 24.");
                    }
                    _frets = value;
                    this.onChanged();
                }
            }
        }

        private Font _titleFont = new Font("Arial Black", 72, GraphicsUnit.Pixel);
        [DisplayName("Title Font")]
        public Font TitleFont
        {
            get
            {
                return _titleFont;
            }
            set
            {
                _titleFont = value;
                this.onChanged();
            }
        }

        private Font _noteFont = new Font("Arial Black", 36, GraphicsUnit.Pixel);
        [DisplayName("Note Font")]
        public Font NoteFont
        {
            get { return _noteFont; }
            set
            {
                _noteFont = value;
                this.onChanged();
            }
        }

        // [DisplayName("Image Width")]
        // public float Width { get; set; }

        // [DisplayName("Image Height")]
        // public float Height { get; set; }

        private float _fretboardLength = 5120;
        [DisplayName("Fretboard Length")]
        public float FretboardLength
        {
            get { return _fretboardLength; }
            set
            {
                if (_fretboardLength != value)
                {
                    _fretboardLength = value;
                    this.onChanged();
                }
            }
        }

        private float _stringSpacing = 80;
        [DisplayName("String Spacing")]
        public float StringSpacing
        {
            get { return _stringSpacing; }
            set
            {
                if(value != _stringSpacing)
                {
                    _stringSpacing = value;
                    this.onChanged();
                }
            }
        }

        private float _padding = 80;
        [DisplayName("Padding")]
        public float Padding 
        {
            get { return _padding; }
            set
            {
                if (value != _padding)
                {
                    _padding = value;
                    this.onChanged();
                }
            }
        }

        private float _nutWidth = 40;
        [DisplayName("Nut Width")]
        public float NutWidth 
        {
            get { return _nutWidth; }
            set
            {
                if (value != _nutWidth)
                {
                    _nutWidth = value;
                    this.onChanged();
                }
            }
        }

        private float _fretWidth = 8;
        [DisplayName("Fret Width")]
        public float FretWidth 
        {
            get { return _fretWidth; }
            set
            {
                if (value != _fretWidth)
                {
                    _fretWidth = value;
                    this.onChanged();
                }
            }
        }

        private float _stringWidth = 1.5f;
        [DisplayName("String Width")]
        public float StringWidth 
        {
            get { return _stringWidth; }
            set
            {
                if (value != _stringWidth)
                {
                    _stringWidth = value;
                    this.onChanged();
                }
            }
        }

        private bool _displayDots = true;
        [DisplayName("Display Fretboard Dots")]
        public bool DisplayDots
        {
            get { return _displayDots; }
            set
            {
                _displayDots = value;
                this.onChanged();
            }
        }

        private float _dotWidth = 48;
        [DisplayName("Fret Dot Width")]
        public float DotWidth
        {
            get { return _dotWidth; }
            set
            {
                if (value != _dotWidth)
                {
                    _dotWidth = value;
                    this.onChanged();
                }
            }
        }

        private float _noteWidth = 48;
        [DisplayName("Note Width")]
        public float NoteWidth
        {
            get { return _noteWidth; }
            set
            {
                if (value != _noteWidth)
                {
                    _noteWidth = value;
                    this.onChanged();
                }
            }
        }

        private Color _backgroundColor = Color.White;
        [DisplayName("Background Color")]
        public Color BackgroundColor 
        {
            get { return _backgroundColor; }
            set
            {
                if (value != _backgroundColor)
                {
                    _backgroundColor = value;
                    this.onChanged();
                }
            }
        }

        private Color _fretColor = Color.Black;
        [DisplayName("Fret Color")]
        public Color FretColor 
        {
            get { return _fretColor; }
            set
            {
                if (value != _fretColor)
                {
                    _fretColor = value;
                    this.onChanged();
                }
            }
        }

        private Color _dotColor = Color.LightGray;
        [DisplayName("Fret Dot Color")]
        public Color DotColor 
        {
            get { return _dotColor; }
            set
            {
                if (value != _dotColor)
                {
                    _dotColor = value;
                    this.onChanged();
                }
            }
        }

        private Color _stringColor = Color.Black;
        [DisplayName("String Color")]
        public Color StringColor 
        {
            get { return _stringColor; }
            set
            {
                if (value != _stringColor)
                {
                    _stringColor = value;
                    this.onChanged();
                }
            }
        }

        private bool _useSharps = false;
        [DisplayName("Use Sharps")]
        public bool UseSharps
        {
            get { return _useSharps; }
            set
            {
                if (value != _useSharps)
                {
                    _useSharps = value;
                    this.onChanged();
                }
            }
        }

        private Notes[] _baseNotes;
        [DisplayName("Base Notes")]
        public Notes[] BaseNotes 
        {
            get { return _baseNotes; }
        }

        [Browsable(false)]
        public NoteStyle[] NoteStyles { get; set; }

        [Browsable(false)]
        public Color[] NoteColors { get; set; }

        [Browsable(false)]
        public Color[] NoteTextColors { get; set; }

        [Browsable(false)]
        public int RootNoteIndex { get; set; }

        public Fretboard()
        {
            this.NoteStyles = new NoteStyle[] 
            { 
                NoteStyle.Square, NoteStyle.None, NoteStyle.None, 
                NoteStyle.None, NoteStyle.None, NoteStyle.None, 
                NoteStyle.None, NoteStyle.None, NoteStyle.None, 
                NoteStyle.None, NoteStyle.None, NoteStyle.None, 
            };

            this.NoteColors = new Color[] 
            { 
                Color.Black, Color.Black, Color.Black,
                Color.Black, Color.Black, Color.Black,
                Color.Black, Color.Black, Color.Black,
                Color.Black, Color.Black, Color.Black
            };

            this.NoteTextColors = new Color[] 
            { 
                Color.White, Color.White, Color.White,
                Color.White, Color.White, Color.White,
                Color.White, Color.White, Color.White,
                Color.White, Color.White, Color.White
            };

            this.RootNoteIndex = -1;
            ApplyBackgroundPalette();

            _baseNotes = BuildDefaultTuning(_strings);
        }

        private void ApplyBackgroundPalette()
        {
            BackgroundPalette palette = GetBackgroundPalette(this.FingerboardBackground);
            _fretColor = palette.FretColor;
            _nutColor = palette.NutColor;
            _stringColor = palette.StringColor;
            _dotColor = palette.DotColor;
        }

        private static BackgroundPalette GetBackgroundPalette(FingerboardBackgroundKind background)
        {
            switch (background)
            {
                case FingerboardBackgroundKind.Ebony:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(28, 24, 22), SecondaryColor = Color.FromArgb(42, 37, 33), GrainColor = Color.FromArgb(70, 60, 52), UseTexture = true, FretColor = Color.FromArgb(205, 210, 216), NutColor = Color.FromArgb(240, 240, 232), StringColor = Color.FromArgb(196, 202, 209), DotColor = Color.FromArgb(188, 192, 184) };
                case FingerboardBackgroundKind.Maple:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(214, 178, 121), SecondaryColor = Color.FromArgb(243, 215, 167), GrainColor = Color.FromArgb(173, 131, 76), UseTexture = true, FretColor = Color.FromArgb(86, 72, 57), NutColor = Color.FromArgb(248, 243, 225), StringColor = Color.FromArgb(108, 92, 74), DotColor = Color.FromArgb(126, 89, 53) };
                case FingerboardBackgroundKind.Walnut:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(78, 54, 41), SecondaryColor = Color.FromArgb(105, 74, 56), GrainColor = Color.FromArgb(138, 104, 74), UseTexture = true, FretColor = Color.FromArgb(208, 202, 190), NutColor = Color.FromArgb(238, 231, 214), StringColor = Color.FromArgb(192, 187, 177), DotColor = Color.FromArgb(222, 205, 167) };
                case FingerboardBackgroundKind.PauFerro:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(96, 62, 42), SecondaryColor = Color.FromArgb(142, 95, 63), GrainColor = Color.FromArgb(191, 153, 110), UseTexture = true, FretColor = Color.FromArgb(223, 216, 203), NutColor = Color.FromArgb(244, 236, 217), StringColor = Color.FromArgb(207, 202, 193), DotColor = Color.FromArgb(229, 194, 144) };
                case FingerboardBackgroundKind.Charcoal:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(54, 57, 63), SecondaryColor = Color.FromArgb(36, 40, 46), GrainColor = Color.FromArgb(75, 80, 88), UseTexture = false, FretColor = Color.FromArgb(215, 220, 226), NutColor = Color.FromArgb(245, 245, 240), StringColor = Color.FromArgb(200, 206, 214), DotColor = Color.FromArgb(141, 150, 162) };
                case FingerboardBackgroundKind.MidnightBlue:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(33, 52, 88), SecondaryColor = Color.FromArgb(18, 28, 48), GrainColor = Color.FromArgb(61, 88, 140), UseTexture = false, FretColor = Color.FromArgb(220, 228, 236), NutColor = Color.FromArgb(242, 240, 232), StringColor = Color.FromArgb(202, 210, 221), DotColor = Color.FromArgb(164, 184, 212) };
                case FingerboardBackgroundKind.SageGreen:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(76, 101, 84), SecondaryColor = Color.FromArgb(53, 73, 59), GrainColor = Color.FromArgb(112, 144, 113), UseTexture = false, FretColor = Color.FromArgb(220, 224, 214), NutColor = Color.FromArgb(241, 238, 225), StringColor = Color.FromArgb(202, 206, 196), DotColor = Color.FromArgb(184, 198, 167) };
                case FingerboardBackgroundKind.Burgundy:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(104, 43, 54), SecondaryColor = Color.FromArgb(70, 24, 34), GrainColor = Color.FromArgb(142, 74, 88), UseTexture = false, FretColor = Color.FromArgb(230, 220, 210), NutColor = Color.FromArgb(247, 241, 229), StringColor = Color.FromArgb(214, 203, 194), DotColor = Color.FromArgb(219, 181, 171) };
                case FingerboardBackgroundKind.Sandstone:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(181, 160, 123), SecondaryColor = Color.FromArgb(156, 136, 101), GrainColor = Color.FromArgb(128, 112, 83), UseTexture = false, FretColor = Color.FromArgb(80, 67, 53), NutColor = Color.FromArgb(248, 241, 228), StringColor = Color.FromArgb(99, 83, 66), DotColor = Color.FromArgb(111, 93, 72) };
                default:
                    return new BackgroundPalette { BaseColor = Color.FromArgb(74, 47, 35), SecondaryColor = Color.FromArgb(101, 67, 49), GrainColor = Color.FromArgb(140, 102, 74), UseTexture = true, FretColor = Color.FromArgb(214, 208, 196), NutColor = Color.FromArgb(244, 238, 223), StringColor = Color.FromArgb(201, 196, 188), DotColor = Color.FromArgb(214, 202, 175) };
            }
        }

        private Brush CreateFingerboardBrush(RectangleF fingerboardRect)
        {
            BackgroundPalette palette = GetBackgroundPalette(this.FingerboardBackground);

            if (!palette.UseTexture)
            {
                return new LinearGradientBrush(fingerboardRect, palette.BaseColor, palette.SecondaryColor, 90f);
            }

            TextureBrush textureBrush = new TextureBrush(GetTextureTile(this.FingerboardBackground), WrapMode.TileFlipXY);
            textureBrush.ScaleTransform(1.5f, 1.0f);
            return textureBrush;
        }

        private static Bitmap GetTextureTile(FingerboardBackgroundKind background)
        {
            Bitmap tile;

            if (!BackgroundTextureCache.TryGetValue(background, out tile))
            {
                tile = CreateTextureTile(GetBackgroundPalette(background), (int)background + 1);
                BackgroundTextureCache.Add(background, tile);
            }

            return tile;
        }

        private static Bitmap CreateTextureTile(BackgroundPalette palette, int seed)
        {
            Bitmap bitmap = new Bitmap(320, 120, PixelFormat.Format32bppArgb);

            for (int y = 0; y < bitmap.Height; y++)
            {
                float band = 0.5f + 0.25f * (float)Math.Sin((y + seed * 7) * 0.19f) + 0.12f * (float)Math.Sin((y + seed * 11) * 0.47f);

                for (int x = 0; x < bitmap.Width; x++)
                {
                    float streak = 0.08f * (float)Math.Sin((x + seed * 17) * 0.035f) + 0.04f * (float)Math.Sin((x + y + seed * 29) * 0.08f);
                    float noise = ((GetNoise(x, y, seed) & 255) / 255f - 0.5f) * 0.08f;
                    float blend = Clamp01(band + streak + noise);
                    Color pixel = BlendColors(palette.BaseColor, palette.SecondaryColor, blend);

                    if (((x + seed * 13) % 67) < 3)
                    {
                        pixel = BlendColors(pixel, palette.GrainColor, 0.45f);
                    }

                    bitmap.SetPixel(x, y, pixel);
                }
            }

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Pen grainPen = new Pen(Color.FromArgb(45, palette.GrainColor), 2f))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                for (int y = 14; y < bitmap.Height; y += 22)
                {
                    graphics.DrawBezier(grainPen, 0, y, 80, y - 6, 220, y + 8, bitmap.Width, y - 2);
                }
            }

            return bitmap;
        }

        private static int GetNoise(int x, int y, int seed)
        {
            int value = unchecked(x * 73856093 ^ y * 19349663 ^ seed * 83492791);
            value = unchecked((value << 13) ^ value);
            return unchecked(value * (value * value * 15731 + 789221) + 1376312589);
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static Color BlendColors(Color from, Color to, float amount)
        {
            amount = Clamp01(amount);
            return Color.FromArgb(
                255,
                (int)(from.R + (to.R - from.R) * amount),
                (int)(from.G + (to.G - from.G) * amount),
                (int)(from.B + (to.B - from.B) * amount));
        }

        private static Notes[] BuildDefaultTuning(short stringCount)
        {
            Notes[] notes = { Notes.E, Notes.A, Notes.D, Notes.G, Notes.B, Notes.E };
            Notes[] tuning = new Notes[stringCount];

            for (int i = 0; i < stringCount; i++)
            {
                tuning[i] = notes[i % notes.Length];
            }

            return tuning;
        }

        public void ApplyTuning(TuningDefinition tuning)
        {
            if (tuning == null)
            {
                throw new ArgumentNullException("tuning");
            }

            if (tuning.BaseNotes == null || tuning.BaseNotes.Length < 1 || tuning.BaseNotes.Length > 12)
            {
                throw new ArgumentOutOfRangeException("tuning", "Tuning must define between 1 and 12 strings.");
            }

            _strings = (short)tuning.BaseNotes.Length;
            _baseNotes = (Notes[])tuning.BaseNotes.Clone();
            this.onChanged();
        }

        public void SetBaseNotes(params Notes[] baseNotes)
        {
            if (baseNotes == null)
            {
                throw new ArgumentNullException("baseNotes");
            }

            if (baseNotes.Length < 1 || baseNotes.Length > 12)
            {
                throw new ArgumentOutOfRangeException("baseNotes", "Base notes must define between 1 and 12 strings.");
            }

            _strings = (short)baseNotes.Length;
            _baseNotes = (Notes[])baseNotes.Clone();
            this.onChanged();
        }

        protected void onChanged()
        {
            if (this.Changed != null)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        public Bitmap Render()
        {
            // Compute size
            float width = _padding * 2 + _noteWidth + _nutWidth + _fretboardLength;
            float topAreaPadding = _padding + _topJustifyPadding;
            float headerHeight = topAreaPadding + _titleFont.Size;
            float fingerboardHeight = (_strings - 1) * _stringSpacing;
            float fretNumberHeight = _showFretNumbers ? Math.Max(18f, _noteFont.Size * 0.8f) : 0f;
            float height = topAreaPadding + _padding + headerHeight + fingerboardHeight + fretNumberHeight;

            float fretOffset;
            float neckTop = headerHeight + _padding * 0.35f;
            float neckCenter = neckTop + fingerboardHeight / 2;
            float dotRadius = this.DotWidth / 2.0f;
            float noteRadius = this.NoteWidth / 2.0f;
            float[] fretOffsets = new float[this.Frets + 1];
            float[] noteXOffsets = new float[this.Frets + 1];
            float[] noteYOffsets = new float[this.Strings];
            float scaleLength = _fretboardLength * 1.3333333f;
            float fingerboardTop = neckTop;
            float fingerboardBottom = fingerboardTop + fingerboardHeight;
            RectangleF fingerboardRect = new RectangleF(_padding + _noteWidth, fingerboardTop, _nutWidth + _fretboardLength, fingerboardHeight);

            Bitmap b = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.Clear(this.BackgroundColor);

                using (Brush fingerboardBrush = CreateFingerboardBrush(fingerboardRect))
                {
                    g.FillRectangle(fingerboardBrush, fingerboardRect);
                }

                Brush fretBrush = new SolidBrush(this.FretColor);
                Pen fretPen = new Pen(this.FretColor, this.FretWidth);
                Brush nutBrush = new SolidBrush(this.NutColor);
                Brush titleBrush = new SolidBrush(this.TitleColor);

                // Draw title horizontally, centered in the reserved space above the fretboard.
                float titleAreaTop = _padding;
                float titleAreaBottom = fingerboardTop - Math.Max(8f, _padding * 0.15f);
                float titleAreaHeight = Math.Max(0f, titleAreaBottom - titleAreaTop);

                if (titleAreaHeight > 0f)
                {
                    StringFormat titleFormat = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                    titleFormat.Alignment = StringAlignment.Near;
                    titleFormat.LineAlignment = StringAlignment.Center;

                    float titleAreaLeft = _padding;
                    float titleAreaWidth = width - (_padding * 2f);
                    g.DrawString(_title, _titleFont, titleBrush, new RectangleF(titleAreaLeft, titleAreaTop, titleAreaWidth, titleAreaHeight), titleFormat);

                    titleFormat.Dispose();
                }

                // Draw nut
                g.DrawRectangle(fretPen, _padding + _noteWidth, fingerboardTop, _nutWidth,  fingerboardHeight);
                g.FillRectangle(nutBrush, _padding + _noteWidth, fingerboardTop, _nutWidth,  fingerboardHeight);

                // Draw frets
                for (int fret = 0; fret <= _frets; fret++)
                {
                    fretOffset = FretFactors[fret] * scaleLength + _padding + _noteWidth + _nutWidth;
                    fretOffsets[fret] = fretOffset;
                    g.DrawLine(fretPen, fretOffset, fingerboardTop, fretOffset, fingerboardBottom);
                }

                fretPen.Dispose();
                fretBrush.Dispose();
                nutBrush.Dispose();
                titleBrush.Dispose();

                if(_displayDots) 
                {
                    // Fret dots
                    Brush dotBrush = new SolidBrush(this.DotColor);
                
                    // Third fret dot
                    if (_frets >= 3)
                    {
                        fretOffset = (fretOffsets[2] + fretOffsets[3]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // Fifth fret dot
                    if (_frets >= 5)
                    {
                        fretOffset = (fretOffsets[4] + fretOffsets[5]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // Seventh fret dot
                    if (_frets >= 7)
                    {
                        fretOffset = (fretOffsets[6] + fretOffsets[7]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // Ninth fret dot
                    if (_frets >= 9)
                    {
                        fretOffset = (fretOffsets[8] + fretOffsets[9]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // Twelth fret dots
                    if (_frets >= 12)
                    {
                        fretOffset = (fretOffsets[11] + fretOffsets[12]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - _stringSpacing - dotRadius, this.DotWidth, this.DotWidth);
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter + _stringSpacing - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // 15th fret dot
                    if (_frets >= 15)
                    {
                        fretOffset = (fretOffsets[14] + fretOffsets[15]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // 17th fret dot
                    if (_frets >= 17)
                    {
                        fretOffset = (fretOffsets[16] + fretOffsets[17]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // 19th fret dot
                    if (_frets >= 19)
                    {
                        fretOffset = (fretOffsets[18] + fretOffsets[19]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // 21st fret dot
                    if (_frets >= 21)
                    {
                        fretOffset = (fretOffsets[20] + fretOffsets[21]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    // 24th fret dots
                    if (_frets >= 24)
                    {
                        fretOffset = (fretOffsets[23] + fretOffsets[24]) / 2.0f;
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter - _stringSpacing - dotRadius, this.DotWidth, this.DotWidth);
                        g.FillEllipse(dotBrush, fretOffset - dotRadius, neckCenter + _stringSpacing - dotRadius, this.DotWidth, this.DotWidth);
                    }

                    dotBrush.Dispose();
                }


                // Draw strings
                Pen stringPen = new Pen(this.StringColor, this.StringWidth);
                float y = fingerboardTop;
                for(short i = 0; i < this.Strings; i++) {
                    g.DrawLine(stringPen, (float) _padding + _noteWidth, y, (float) width - _padding, y);
                    y += _stringSpacing;
                }
                stringPen.Dispose();

                // Compute note positions
                y = fingerboardTop;
                for (short s = 0; s < this.Strings; s++)
                {
                    noteYOffsets[s] = y;
                    y += _stringSpacing;
                }
                float x;
                for (short f = 0; f <= this.Frets; f++)
                {
                    if (f == 0)
                    {
                        x = _padding + this.NoteWidth - this.NoteWidth / 2;
                    }
                    else
                    {
                        x = (fretOffsets[f - 1] + fretOffsets[f]) / 2.0f;
                    }
                    noteXOffsets[f] = x;
                }

                // Render notes
                Brush[] noteBrushes = new Brush[12];
                Brush[] noteTextBrushes = new Brush[12];
                for (int i = 0; i < 12; i++)
                {
                    noteBrushes[i] = new SolidBrush(this.NoteColors[i]);
                    noteTextBrushes[i] = new SolidBrush(this.NoteTextColors[i]);
                }
                Pen noteOutlinePen = new Pen(this.NoteOutlineColor, Math.Max(1f, this.NoteOutlineWidth));
                Pen rootHighlightPen = new Pen(this.RootHighlightColor, Math.Max(2f, this.NoteOutlineWidth + 1.5f));
                int noteIndex = 0;
                RectangleF layoutRect;
                StringFormat sf = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.None;

                // Render each string
                for (short i = 0; i < this.Strings; i++)
                {
                    // Render each fret
                    for (short j = 0; j <= this.Frets; j++)
                    {
                        x = noteXOffsets[j];
                        y = noteYOffsets[i];
                        noteIndex = (int)(this.BaseNotes[this.Strings - i - 1] + j) % 12;
                        switch (NoteStyles[noteIndex])
                        {
                            case NoteStyle.Square:
                                g.FillRectangle(noteBrushes[noteIndex], x - noteRadius, y - noteRadius, this.NoteWidth, this.NoteWidth);
                                g.DrawRectangle(noteOutlinePen, x - noteRadius, y - noteRadius, this.NoteWidth, this.NoteWidth);
                                if (this.RootNoteIndex == noteIndex)
                                {
                                    g.DrawRectangle(rootHighlightPen, x - noteRadius - rootHighlightPen.Width / 2f, y - noteRadius - rootHighlightPen.Width / 2f, this.NoteWidth + rootHighlightPen.Width, this.NoteWidth + rootHighlightPen.Width);
                                }
                                layoutRect = new RectangleF(x - noteRadius, y - noteRadius, this.NoteWidth, this.NoteWidth);
                                if (this.UseSharps)
                                {
                                    g.DrawString(NotesSharped[noteIndex], this.NoteFont, noteTextBrushes[noteIndex], layoutRect, sf);
                                }
                                else
                                {
                                    g.DrawString(NotesFlatted[noteIndex], this.NoteFont, noteTextBrushes[noteIndex], layoutRect, sf);
                                }
                                break;

                            case NoteStyle.Circle:
                                g.FillEllipse(noteBrushes[noteIndex], x - noteRadius, y - noteRadius, this.NoteWidth, this.NoteWidth);
                                g.DrawEllipse(noteOutlinePen, x - noteRadius, y - noteRadius, this.NoteWidth, this.NoteWidth);
                                if (this.RootNoteIndex == noteIndex)
                                {
                                    g.DrawEllipse(rootHighlightPen, x - noteRadius - rootHighlightPen.Width / 2f, y - noteRadius - rootHighlightPen.Width / 2f, this.NoteWidth + rootHighlightPen.Width, this.NoteWidth + rootHighlightPen.Width);
                                }
                                layoutRect = new RectangleF(x - noteRadius, y - noteRadius, this.NoteWidth, this.NoteWidth);
                                if (this.UseSharps)
                                {
                                    g.DrawString(NotesSharped[noteIndex], this.NoteFont, noteTextBrushes[noteIndex], layoutRect, sf);
                                }
                                else
                                {
                                    g.DrawString(NotesFlatted[noteIndex], this.NoteFont, noteTextBrushes[noteIndex], layoutRect, sf);
                                }
                                break;

                            case NoteStyle.Triangle:
                                PointF[] points = new PointF[3];
                                points[0] = new PointF(x, y - this.NoteWidth / 2);
                                points[1] = new PointF(x + this.NoteWidth / 2, y + this.NoteWidth / 2);
                                points[2] = new PointF(x - this.NoteWidth / 2, y + this.NoteWidth / 2);
                                g.FillPolygon(noteBrushes[noteIndex], points);
                                g.DrawPolygon(noteOutlinePen, points);
                                if (this.RootNoteIndex == noteIndex)
                                {
                                    PointF[] rootPoints = new PointF[3];
                                    float rootOffset = rootHighlightPen.Width / 2f;
                                    rootPoints[0] = new PointF(x, y - this.NoteWidth / 2 - rootOffset);
                                    rootPoints[1] = new PointF(x + this.NoteWidth / 2 + rootOffset, y + this.NoteWidth / 2 + rootOffset);
                                    rootPoints[2] = new PointF(x - this.NoteWidth / 2 - rootOffset, y + this.NoteWidth / 2 + rootOffset);
                                    g.DrawPolygon(rootHighlightPen, rootPoints);
                                }
                                layoutRect = new RectangleF(x - noteRadius, y - noteRadius * 0.75f, this.NoteWidth, this.NoteWidth * 0.9f);
                                if (this.UseSharps)
                                {
                                    g.DrawString(NotesSharped[noteIndex], this.NoteFont, noteTextBrushes[noteIndex], layoutRect, sf);
                                }
                                else
                                {
                                    g.DrawString(NotesFlatted[noteIndex], this.NoteFont, noteTextBrushes[noteIndex], layoutRect, sf);
                                }
                                break;
                        }

                    }
                }

                if (this.ShowFretNumbers)
                {
                    using (Font fretNumberFont = new Font(this.NoteFont.FontFamily, Math.Max(12f, this.NoteFont.Size * 0.5f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (Brush fretNumberBrush = new SolidBrush(Color.FromArgb(this.FretColor.A, this.FretColor.R / 2, this.FretColor.G / 2, this.FretColor.B / 2)))
                    {
                        float fretNumberY = headerHeight + _padding + fingerboardHeight + Math.Max(8f, fretNumberFont.Size * 0.2f);

                        for (short fret = 1; fret <= this.Frets; fret++)
                        {
                            float fretCenter = (fretOffsets[fret - 1] + fretOffsets[fret]) / 2.0f;
                            RectangleF fretRect = new RectangleF(fretCenter - 28f, fretNumberY, 56f, fretNumberFont.Height + 8f);
                            g.DrawString(fret.ToString(), fretNumberFont, fretNumberBrush, fretRect, sf);
                        }
                    }
                }

                sf.Dispose();
                noteOutlinePen.Dispose();
                rootHighlightPen.Dispose();
                for (int i = 0; i < 12; i++)
                {
                    noteBrushes[i].Dispose();
                    noteTextBrushes[i].Dispose();
                }
            }

            return b;
        }
    }
}
