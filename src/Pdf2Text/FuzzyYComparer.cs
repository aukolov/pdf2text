using System;
using System.Collections.Generic;

namespace Pdf2Text
{
    public class FuzzyYComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            if (Math.Abs(x - y) < 5)
            {
                return 0;
            }
            if (x < y)
            {
                return -1;
            }
            return 1;
        }
    }
}