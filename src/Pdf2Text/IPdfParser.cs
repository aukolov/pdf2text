namespace Pdf2Text
{
    public interface IPdfParser
    {
        PdfModel Parse(string filePath);
    }
}