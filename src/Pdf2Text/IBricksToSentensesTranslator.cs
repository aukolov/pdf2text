using System.Collections.Generic;

namespace Pdf2Text
{
    internal interface IBricksToSentensesTranslator
    {
        List<List<SentenceModel>> Translate(IEnumerable<IBrick[]> brickLists);
    }
}