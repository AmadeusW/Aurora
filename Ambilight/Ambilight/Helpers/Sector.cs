namespace AmadeusW.Ambilight.Helpers
{
    public class Sector
    {
        public enum ScreenEdge
        {
            Top,
            Right,
            Bottom,
            Left
        };

        public System.Drawing.Rectangle Area;
        public int Index;
        public bool SpecialMiddleSector;
        public int Anchor;
        public ScreenEdge Edge;
    }
}
