using System;

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
}
