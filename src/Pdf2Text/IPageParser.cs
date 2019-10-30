using System.Collections.Generic;

namespace Pdf2Text
{
    public interface IPageParser
    {
        List<List<SentenceModel>> Parse(string pdfFilePath, int pageNumber);
    }
}