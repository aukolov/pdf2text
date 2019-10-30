using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Pdf2Text.Tests
{
    [TestFixture]
    public class PageModelTests
    {
        private static BlockModel CreateBlockModel(int blockNumber, params string[] sentences)
        {
            var sentencesList = new List<SentenceModel>();
            sentencesList.AddRange(sentences.Select(s => new SentenceModel(s, 0, 0, 0, 0)));
            return new BlockModel(blockNumber, sentencesList);
        }

        private static PageModel CreatePageModel(int pageNumber, params BlockModel[] blockModels)
        {
            return new PageModel(pageNumber, blockModels.ToList());
        }

        private static void AssertLocation(Location location, int page, int block, int sentence)
        {
            Assert.AreEqual(page, location.Page, "Page");
            Assert.AreEqual(block, location.Block, "Block");
            Assert.AreEqual(sentence, location.Sentence, "Sentence");
        }

        [Test]
        public void MatchesFirstBlockFirstSentence()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa", "bbb"));

            var locationRanges = pageModel.Find("abc").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 0);
        }

        [Test]
        public void MatchesFirstBlockSecondSentence()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "ccc", "abc", "bbb"));

            var locationRanges = pageModel.Find("abc").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void MatchesSecondBlock()
        {
            var pageModel = CreatePageModel(0,
                CreateBlockModel(0, "ccc", "bbb"),
                CreateBlockModel(1, "ccc", "ddd", "abc", "bbb"));

            var locationRanges = pageModel.Find("abc").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 1, 2);
            AssertLocation(locationRange.End, 0, 1, 2);
        }

        [Test]
        public void ReturnsCorrectPageNumber()
        {
            var pageModel = CreatePageModel(42, CreateBlockModel(0, "abc", "aaa", "bbb"));

            var locationRanges = pageModel.Find("abc").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 42, 0, 0);
            AssertLocation(locationRange.End, 42, 0, 0);
        }

        [Test]
        public void MatchesTwoSentences()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa", "bbb"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 2);
        }

        [Test]
        public void MatchesSentenceIfStartsFromSpaces()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "   aaa", "bbb"));

            var locationRanges = pageModel.Find("aaa").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void DoesNotMatchSentenceIfContinuesWithNonWhitespace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaab", "bbb"));

            var locationRanges = pageModel.Find("aaa").ToArray();

            Assert.AreEqual(0, locationRanges.Length);
        }


        [Test]
        public void MatchesSentenceIfEndsWithSpaces()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa   ", "bbb"));

            var locationRanges = pageModel.Find("aaa").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void TreatsMultipleSpacesInSearchPatternAsOne()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa", "bbb"));

            var locationRanges = pageModel.Find("aaa       bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 2);
        }



        [Test]
        public void TreatsMultipleSpacesInSentenceAsOne()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa              bbb"));

            var locationRanges = pageModel.Find("aaa       bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void SkipsAllWhitespaceSentencesOnSpaceInText()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa", "   ", " ", "      ", "bbb"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 5);
        }

        [Test]
        public void SkipsAllEmptySentencesOnSpaceInText()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "abc", "aaa", "", "", "", "bbb"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 0, 5);
        }

        [Test]
        public void SkipsAllEmptyAndWhitespaceBlocksOnSpaceInText()
        {
            var pageModel = CreatePageModel(0,
                CreateBlockModel(0, "abc", "aaa"),
                CreateBlockModel(1, "    ", "", " "),
                CreateBlockModel(2, "", "", ""),
                CreateBlockModel(3, "", "", "   "),
                CreateBlockModel(4, "bbb", "ccc"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 1);
            AssertLocation(locationRange.End, 0, 4, 0);
        }

        [Test]
        public void DoesNotJumpToNewSentenceIfSearchPatternEndsWithSpaced()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "aaa        ", "bbb"));

            var locationRanges = pageModel.Find("aaa   ").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 0);
        }

        [Test]
        public void DoesNotMatchIfTextEndsBeforePattern()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "aa"));

            var locationRanges = pageModel.Find("aaa").ToArray();

            Assert.AreEqual(0, locationRanges.Length);
        }

        [Test]
        public void ProcessesCorrectlyManySpacesInPattern()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "aaa", "bbb"));

            var locationRanges = pageModel.Find(" aaa  bbb   ").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void TreatsTabsInPatternLikeSpace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "aaa", "bbb"));

            var locationRanges = pageModel.Find("\taaa\t\tbbb\t\t\t").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void TreatsNewLinesInPatternLikeSpace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "aaa", "bbb"));

            var locationRanges = pageModel.Find("\naaa\n\nbbb\n\n\n").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void TreatsCarriageReturnsInPatternLikeSpace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "aaa", "bbb"));

            var locationRanges = pageModel.Find("\raaa\r\rbbb\r\r\r").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 1);
        }

        [Test]
        public void TreatsTabsInTextLikeSpace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "\t\taaa\t\t", "\t\t\t", "\t\tbbb\t"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 2);
        }

        [Test]
        public void TreatsNewLinesInTextLikeSpace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "\n\naaa\n\n", "\n\n\n", "\n\nbbb\n"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 2);
        }

        [Test]
        public void TreatsCarriageReturnsInTextLikeSpace()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "\r\raaa\r\r", "\r\r\r", "\r\rbbb\r"));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 2);
        }

        [Test]
        public void ProcessesCorrectlyMultipleSpacesInText()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "  aaa  ", "   ", "  bbb "));

            var locationRanges = pageModel.Find("aaa bbb").ToArray();

            Assert.AreEqual(1, locationRanges.Length);
            var locationRange = locationRanges[0];
            AssertLocation(locationRange.Start, 0, 0, 0);
            AssertLocation(locationRange.End, 0, 0, 2);
        }

        [Test]
        public void DoesNotMatchPatternConsistingOnlyOfWhitespaces()
        {
            var pageModel = CreatePageModel(0, CreateBlockModel(0, "  aaa  ", "   ", "  bbb "));

            var locationRanges = pageModel.Find("       ").ToArray();

            Assert.AreEqual(0, locationRanges.Length);
        }
    }
}