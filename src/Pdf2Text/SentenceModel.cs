using System.Runtime.Serialization;

namespace Pdf2Text
{
    public class SentenceModel
    {
        public SentenceModel(
            string text,
            double left,
            double bottom,
            double width,
            double height)
        {
            Text = text;
            Left = left;
            Bottom = bottom;
            Width = width;
            Height = height;
        }

        [DataMember]
        public string Text { get; private set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public double Left { get; private set; }
        [DataMember]
        public double Bottom { get; private set; }
        [DataMember]
        public double Width { get; private set; }
        [DataMember]
        public double Height { get; private set; }

        public double Right => Left + Width;
        public double Top => Bottom - Height;
    }
}