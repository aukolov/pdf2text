using System;
using System.Text;

namespace Pdf2Text
{
    internal class SentenceModelBuilder
    {
        private readonly bool _addSpaces;
        private readonly StringBuilder _sb = new StringBuilder();

        public SentenceModelBuilder(bool addSpaces)
        {
            _addSpaces = addSpaces;
            IsEmpty = true;
        }

        private double Left { get; set; }
        private double Top { get; set; }
        private double Right { get; set; }
        private double Bottom { get; set; }

        public bool IsEmpty { get; private set; }

        public void Append(string text, double x, double y, double width, double height)
        {
            if (IsEmpty)
            {
                Left = x;
                Top = y - height;
                Right = x + width;
                Bottom = y;

                IsEmpty = false;
            }
            else
            {
                Left = Math.Min(Left, x);
                Top = Math.Min(Top, y - height);
                Right = Math.Max(Right, x + width);
                Bottom = Math.Max(Bottom, y);
                if (_addSpaces)
                {
                    _sb.Append(" ");
                }
            }
            _sb.Append(text);
        }

        public SentenceModel Build()
        {
            return new SentenceModel(
                _sb.ToString().Trim(),
                Left,
                Bottom,
                Right - Left,
                Bottom - Top);
        }
    }
}