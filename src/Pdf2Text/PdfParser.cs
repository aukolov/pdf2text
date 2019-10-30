using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Kernel.Pdf;

// ReSharper disable SuggestBaseTypeForParameter

namespace Pdf2Text
{
    public class PdfParser : IPdfParser
    {
        private readonly IPageParser[] _parsers;

        public PdfParser()
        {
            _parsers = new IPageParser[]
            {
                new Text7BasedPageParser() 
            };
        }

        public PdfModel Parse(string filePath)
        {
            int numberOfPages;
            using (var reader = new PdfReader(filePath))
            {
                using (var pdfDocument = new PdfDocument(reader))
                {
                    numberOfPages = pdfDocument.GetNumberOfPages();
                }
            }

            var pageModels = ParallelEnumerable.Range(1, numberOfPages)
                .Select(i => ParsePage(filePath, i))
                .ToList();

            var pdfModel = new PdfModel(Path.GetFileName(filePath), pageModels);

            return pdfModel;
        }

        private PageModel ParsePage(string filePath, int i)
        {
            List<List<SentenceModel>> sentenceModels = null;
            foreach (var parser in _parsers)
            {
                var list = parser.Parse(filePath, i);
                if (list.Any())
                {
                    sentenceModels = list;
                    break;
                }
            }
            var blockModels = (sentenceModels ?? new List<List<SentenceModel>>())
                .Select((sentences, j) => new BlockModel(j, sentences))
                .ToList();

            return new PageModel(i - 1, blockModels);
        }
    }
}