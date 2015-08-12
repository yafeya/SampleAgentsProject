using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared.DataStore;
using Keysight.KCE.IOSamples;
using Microsoft.Practices.Unity;
using SampleIOAgentTest;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            var nonconfigManager = new NonconfigDataManagerRegistry(container);
            var hwconfig = GenerateHwConfig();
            hwconfig.InitializeDll();
        }

        private static HwConfigSample GenerateHwConfig()
        {
            var container = new UnityContainer();
            var nonconfigManager = new NonconfigDataManagerRegistry(container);
            var integrationSvr = new MockIntegrationService { AgentModulesPath = Path.Combine("Modules") };
            var log = new MockAceLog();
            var hwconfig = new HwConfigSample(nonconfigManager, integrationSvr, log);
            return hwconfig;
        }
    }
}
