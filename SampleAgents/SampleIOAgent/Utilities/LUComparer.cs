using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keysight.KCE.IOSamples
{
    internal class LUComparer : IComparer<string>
    {
        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            int result = -1;
            int lu1, lu2;
            if (int.TryParse(x, out lu1) && int.TryParse(y, out lu2))
            {
                result = lu1 - lu2;
            }
            else
            {
                result = -1;
            }
            return result;
        }

        #endregion
    }
}
