using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;

namespace SampleIOAgentTest
{
    public class MockIntegrationService : IIntegrationService
    {
        public string AgentModulesPath { get; set; }

        #region IIntegrationService Members

        public string GetIOAgentIntegrationPath()
        {
            return AgentModulesPath;
        }

        public string GetUIAgentIntegrationPath()
        {
            return AgentModulesPath;
        }

        #endregion
    }
}
