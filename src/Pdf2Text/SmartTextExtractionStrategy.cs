using System;
using System.Collections.Generic;
using System.Linq;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Pdf2Text
{
    internal class SmartTextExtractionStrategy : ITextExtractionStrategy
    {
        private readonly BricksToSentencesTranslator _bricksToSentencesTranslator;
        private readonly List<List<TextRenderInfo>> _blocks;
        private List<TextRenderInfo> _currentBlock;
        private double _maxY;

        public List<List<SentenceModel>> BlockSentences { get; private set; }

        public SmartTextExtractionStrategy()
        {
            _blocks = new List<List<TextRenderInfo>>();
            _bricksToSentencesTranslator = new BricksToSentencesTranslator(
                xDelta: 1,
                yDelta: 1,
                addSpaces: false);
        }

        public string GetResultantText()
        {
            var brickLists = _blocks
                .Select(b => b.Select(InfoToBrick).ToArray())
                .ToArray();

            var blockSentences = _bricksToSentencesTranslator.Translate(brickLists);
            BlockSentences = blockSentences;

            return "";
        }

        private IBrick InfoToBrick(TextRenderInfo info)
        {
            var baseRectangle = info.GetBaseline().GetBoundingRectangle();
            var accentRectangle = info.GetAscentLine().GetBoundingRectangle();

            var text = info.GetText();

            var brick = new Brick(text,
                baseRectangle.GetX(),
                _maxY - baseRectangle.GetY(),
                baseRectangle.GetWidth(),
                accentRectangle.GetY() - baseRectangle.GetY());
            return brick;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            switch (type)
            {
                case EventType.BEGIN_TEXT:
                    _currentBlock = new List<TextRenderInfo>();
                    break;
                case EventType.RENDER_TEXT:
                    var textRenderInfo = (TextRenderInfo)data;
                    textRenderInfo.PreserveGraphicsState();
                    _currentBlock.Add(textRenderInfo);
                    var rectangle = textRenderInfo.GetAscentLine().GetBoundingRectangle();
                    _maxY = Math.Max(_maxY, rectangle.GetY());

                    break;
                case EventType.END_TEXT:
                    _blocks.Add(_currentBlock);
                    _currentBlock = null;
                    break;
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new List<EventType>
            {
                EventType.BEGIN_TEXT,
                EventType.END_TEXT,
                EventType.RENDER_TEXT
            };
        }
    }
}