namespace Pdf2Text
{
    public class Location
    {
        public Location(int page, int block, int sentence)
        {
            Page = page;
            Block = block;
            Sentence = sentence;
        }

        public int Page { get; }
        public int Block { get; }
        public int Sentence { get; }
    }
}