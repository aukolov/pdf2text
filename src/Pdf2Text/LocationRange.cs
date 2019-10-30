namespace Pdf2Text
{
    public class LocationRange
    {
        public LocationRange(Location start, Location end)
        {
            Start = start;
            End = end;
        }

        public Location Start { get; }
        public Location End { get; }
    }
}