using System.Collections.Generic;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace Pdf2Text
{
    internal class Text7BasedPageParser : IText7BasedPageParser
    {
        public List<List<SentenceModel>> Parse(string filePath, int pageNumber)
        {
            using (var pdfReader = new PdfReader(filePath))
            {
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    var pdfPage = pdfDocument.GetPage(pageNumber);
                    var strategy = new SmartTextExtractionStrategy();
                    PdfTextExtractor.GetTextFromPage(pdfPage, strategy);

                    return strategy.BlockSentences;
                }
            }
        }
    }
}