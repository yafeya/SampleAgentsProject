using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keysight.KCE.IOSamples
{
    internal class SampleIOAgentComparer : IComparer<string>
    {
        private string _prifix = "SAMP";
        public SampleIOAgentComparer(string prifix)
        {
            _prifix = prifix;
        }
        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            var prifix1 = x.ToUpper();
            var prifix2 = y.ToUpper();
            var index1 = prifix1.GetIndex(_prifix);
            var index2 = prifix2.GetIndex(_prifix);
            return index1 - index2;
        }

        #endregion
    }
}
