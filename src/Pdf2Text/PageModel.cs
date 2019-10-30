using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pdf2Text
{
    public class PageModel
    {
        private const int YTolerance = 1;

        public PageModel(int pageNumber, List<BlockModel> blocks)
        {
            PageNumber = pageNumber;
            Blocks = blocks;
            Sentences = blocks
                .SelectMany(bm => bm.Sentences)
                .OrderBy(model => model.Bottom, new FuzzyYComparer())
                .ThenBy(model => model.Left)
                .ToList();
            for (var i = 0; i < Sentences.Count; i++)
            {
                Sentences[i].PageIndex = i;
            }
        }

        [DataMember]
        public int PageNumber { get; private set; }
        [DataMember]
        public List<BlockModel> Blocks { get; private set; }
        [DataMember]
        public List<SentenceModel> Sentences { get; private set; }

        public IEnumerable<LocationRange> Find(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }
            var startBlock = 0;
            while (startBlock < Blocks.Count)
            {
                var startSentence = 0;
                while (startSentence < Blocks[startBlock].Sentences.Count)
                {
                    int endBlock;
                    int endSentence;
                    if (IsMatch(startBlock, startSentence, text, out endBlock, out endSentence))
                    {
                        yield return new LocationRange(
                            new Location(PageNumber, startBlock, startSentence),
                            new Location(PageNumber, endBlock, endSentence));
                        startBlock = endBlock;
                        startSentence = endSentence + 1;
                    }
                    else
                    {
                        startSentence++;
                    }
                }
                startBlock++;
            }
        }

        private bool IsMatch(int startBlock, int startSentence, string text, out int endBlock, out int endSentence)
        {
            endBlock = 0;
            endSentence = 0;

            var i = 0;
            var currentBlock = startBlock;
            var currentSentence = startSentence;
            var j = 0;

            SkipSpaces(ref currentBlock, ref currentSentence, ref j);
            while (i < text.Length)
            {
                var ch = text[i];
                if (IsWhiteSpace(ch))
                {
                    i = SkipSpaces(text, i);
                    if (i < text.Length)
                    {
                        SkipSpaces(ref currentBlock, ref currentSentence, ref j);
                    }
                }
                else
                {
                    if (j >= Blocks[currentBlock].Sentences[currentSentence].Text.Length)
                    {
                        return false;
                    }
                    if (ch != Blocks[currentBlock].Sentences[currentSentence].Text[j])
                    {
                        return false;
                    }
                    i++;
                    j++;
                }
            }
            j = SkipSpaces(Blocks[currentBlock].Sentences[currentSentence].Text, j);
            if (j < Blocks[currentBlock].Sentences[currentSentence].Text.Length)
            {
                return false;
            }
            endBlock = currentBlock;
            endSentence = currentSentence;
            return true;
        }

        public SentenceModel Above(SentenceModel sentence)
        {
            var i = GetIndex(sentence);
            if (i < 0)
            {
                return null;
            }

            i -= 1;
            while (i >= 0)
            {
                var candidate = Sentences[i];
                if (candidate.Bottom < sentence.Bottom)
                {
                    if (AreInSameColumn(sentence, candidate))
                    {
                        return candidate;
                    }
                }
                i -= 1;
            }
            return null;
        }

        private bool AreInSameColumn(SentenceModel main, SentenceModel secondary)
        {
            var isMainRightMost = !Right(main).Any();
            var isSecondaryRightMost = !Right(secondary).Any();

            if (isMainRightMost && isSecondaryRightMost)
            {
                var leftOfMain = Left(main).FirstOrDefault();
                var leftOfSecondary = Left(secondary).FirstOrDefault();

                if (leftOfMain?.Right < main.Left
                    && leftOfMain.Right < secondary.Left
                    && leftOfSecondary?.Right < main.Left
                    && leftOfSecondary.Right < secondary.Left
                    )
                {
                    return true;
                }
            }

            var midX = main.Left + main.Width / 2;
            var secondaryLeft = (Left(secondary).FirstOrDefault()?.Right + secondary.Left) / 2 ?? secondary.Left - 100;
            var candidateRight = (Right(secondary).FirstOrDefault()?.Left + secondary.Right) / 2 ?? secondary.Right + 100;
            var areInSameColumn = secondaryLeft <= midX && midX <= candidateRight;
            return areInSameColumn;
        }

        public IEnumerable<SentenceModel> Below(SentenceModel sentence)
        {
            var i = GetIndex(sentence);
            if (i < 0)
            {
                yield break;
            }

            i += 1;
            while (i < Sentences.Count)
            {
                var candidate = Sentences[i];
                if (candidate.Bottom > sentence.Bottom)
                {
                    if (AreInSameColumn(sentence, candidate))
                    {
                        yield return candidate;
                    }
                }
                i += 1;
            }
        }

        public IEnumerable<SentenceModel> Left(SentenceModel sentence)
        {
            var i = GetIndex(sentence);
            if (i < 0)
            {
                yield break;
            }

            i -= 1;
            while (i > 0)
            {
                var candidate = Sentences[i];
                if (Math.Abs(candidate.Bottom - sentence.Bottom) < YTolerance)
                {
                    if (candidate.Left < sentence.Left)
                    {
                        yield return candidate;
                    }
                    else
                    {
                        yield break;
                    }
                }
                i -= 1;
            }
        }

        public IEnumerable<SentenceModel> Right(SentenceModel sentence)
        {
            var i = GetIndex(sentence);
            if (i < 0)
            {
                yield break;
            }

            i += 1;
            while (i < Sentences.Count)
            {
                var candidate = Sentences[i];
                if (Math.Abs(candidate.Bottom - sentence.Bottom) < YTolerance)
                {
                    if (candidate.Right > sentence.Right)
                    {
                        yield return candidate;
                    }
                    else
                    {
                        yield break;
                    }
                }
                i += 1;
            }
        }

        private int GetIndex(SentenceModel sentence)
        {
            var index = sentence.PageIndex;
            if (index < 0) return -1;
            if (index > Sentences.Count) return -1;
            return Sentences[index] == sentence ? index : -1;
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        private static int SkipSpaces(string text, int startIndex)
        {
            var i = startIndex;
            while (i < text.Length && IsWhiteSpace(text[i]))
            {
                i++;
            }
            return i;
        }

        private void SkipSpaces(ref int currentBlock, ref int currentSentence, ref int index)
        {
            while (currentBlock < Blocks.Count)
            {
                while (currentSentence < Blocks[currentBlock].Sentences.Count)
                {
                    var sentence = Blocks[currentBlock].Sentences[currentSentence];

                    while (index < sentence.Text.Length)
                    {
                        if (!IsWhiteSpace(sentence.Text[index]))
                        {
                            return;
                        }
                        index++;
                    }
                    index = 0;
                    currentSentence++;
                }
                currentBlock++;
                currentSentence = 0;
                index = 0;
            }
        }
    }
}