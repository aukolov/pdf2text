using System;
using System.Collections.Generic;
using System.Linq;

namespace Pdf2Text
{
    public class BricksToSentencesTranslator : IBricksToSentensesTranslator
    {
        private readonly double _xDelta;
        private readonly double _yDelta;
        private readonly bool _addSpaces;

        public BricksToSentencesTranslator(double xDelta, double yDelta, bool addSpaces)
        {
            _xDelta = xDelta;
            _yDelta = yDelta;
            _addSpaces = addSpaces;
        }
        
        public List<List<SentenceModel>> Translate(IEnumerable<IBrick[]> brickLists)
        {
            var blockSentences = new List<List<SentenceModel>>();
            foreach (var brickList in brickLists)
            {
                var sentences = new List<SentenceModel>();
                var builder = new SentenceModelBuilder(_addSpaces);

                for (var i = 0; i < brickList.Length; i++)
                {
                    var brick = brickList[i];
                    if (builder.IsEmpty)
                    {
                        Append(builder, brick);
                    }
                    else
                    {
                        var prevBrick = brickList[i - 1];
                        if (RoughEqual(brick.Y, prevBrick.Y, _yDelta)
                            && RoughEqual(prevBrick.X + prevBrick.Width, brick.X, _xDelta))
                        {
                            Append(builder, brick);
                        }
                        else
                        {
                            sentences.Add(builder.Build());
                            builder = new SentenceModelBuilder(_addSpaces);
                            Append(builder, brick);
                        }
                    }
                    if (i == brickList.Length - 1)
                    {
                        sentences.Add(builder.Build());
                    }
                }
                if (sentences.Any())
                {
                    blockSentences.Add(sentences);
                }
            }
            return blockSentences;
        }

        private static void Append(SentenceModelBuilder builder, IBrick brick)
        {
            builder.Append(
                brick.Text,
                brick.X,
                brick.Y,
                brick.Width,
                brick.Height);
        }

        private static bool RoughEqual(double currentRectY, double prevRectX, double delta)
        {
            return Math.Abs(currentRectY - prevRectX) < delta;
        }
    }
}