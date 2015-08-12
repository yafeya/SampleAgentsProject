using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Connectivity.CommonOsAbstractions;

namespace SampleIOAgentTest
{
    internal class MockAceLog : IAceLog
    {

        #region IAceLog Members

        public void Message(string message, string category)
        {
        }

        public void Message(string message)
        {
        }

        public void Error(string message, string category)
        {
        }

        public void Indent()
        {
        }

        public void Unindent()
        {
        }

        #endregion
    }
}
