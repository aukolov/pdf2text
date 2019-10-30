namespace Pdf2Text
{
    public interface IBrick
    {
        string Text { get; }
        double X { get; }
        double Y { get; }
        double Width { get; }
        double Height { get; }
    }

    public class Brick : IBrick
    {
        public Brick(string text, double x, double y, double width, double height)
        {
            Text = text;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public string Text { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
    }
}