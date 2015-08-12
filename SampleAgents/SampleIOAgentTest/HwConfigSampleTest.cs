using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared.DataStore;
using Keysight.KCE.IOSamples;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleIOAgentTest
{
    [TestClass()]
    public class HwConfigSampleTest
    {
        private const string SAMPLES_DIR = "Samples";
        private static string _testDirectory = string.Empty;

        [ClassInitialize]
        public static void InitializeContext(TestContext context)
        {
            _testDirectory = context.TestDeploymentDir;
        }

        private static HwConfigSample GenerateHwConfig()
        {
            var container = new UnityContainer();
            var nonconfigManager = new NonconfigDataManagerRegistry(container);
            var integrationSvr = new MockIntegrationService { AgentModulesPath = Path.Combine(_testDirectory, SAMPLES_DIR) };
            var log = new MockAceLog();
            var hwconfig = new HwConfigSample(nonconfigManager, integrationSvr, log);
            return hwconfig;
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        public void InitializeDllTest()
        {
            var hwconfig = GenerateHwConfig();
            hwconfig.InitializeDll();
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample.xml", "Samples")]
        public void GetAvailableInterfacesTest()
        {
            var hwconfig = GenerateHwConfig();
            var intfcs = hwconfig.GetAvailableInterfaces();
            var intfc = intfcs.FirstOrDefault() as ModelInterfaceSample;

            Assert.IsTrue(intfcs.Count() == 1);
            Assert.IsTrue(intfc.VisaInterfaceId == "SAMP0");
            Assert.IsTrue(intfc.SiclInterfaceId == "samp");
            Assert.IsTrue(intfc != null);
            Assert.IsTrue(intfc.ConnectionTimeout == 10000);
            Assert.IsTrue(intfc.BusAddress == 1);
            Assert.IsTrue(intfc.LogicalUnit == "100");
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample.xml", "Samples")]
        public void GetAvailableDevicesTest()
        {
            var hwconfig = GenerateHwConfig();
            var intfcs = hwconfig.GetAvailableInterfaces();
            var intfc = intfcs.FirstOrDefault();
            var devices = hwconfig.GetAvailableDevices(intfc);
            var device = devices.FirstOrDefault() as ModelDeviceSample;

            Assert.IsTrue(devices.Count() == 1);
            Assert.IsTrue(device != null);
            Assert.IsTrue(device.Manufacturer == "KT");
            Assert.IsTrue(device.ModelNumber == "DEV001");
            Assert.IsTrue(device.SerialNumber == "X01234");
            Assert.IsTrue(device.FirmwareRevision == "1.0.0.1");
            Assert.IsTrue(device.IPAddress == "192.168.56.128");
            Assert.IsTrue(device.DeviceName == "inst0");
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample.xml", "Samples")]
        public void VerifyElementTestInterface()
        {
            var intfc1 = new ModelInterfaceSample { VisaInterfaceId = "SAMP0", SiclInterfaceId = "samp", LogicalUnit = "100" };
            var hwconfig1 = GenerateHwConfig();
            hwconfig1.VerifyElement(intfc1);
            Assert.IsTrue(!intfc1.Failed);
            Assert.IsTrue(intfc1.Verified);

            var intfc2 = new ModelInterfaceSample { VisaInterfaceId = "SAMP1", SiclInterfaceId = "samp1", LogicalUnit = "101" };
            var hwconfig2 = GenerateHwConfig();
            hwconfig2.VerifyElement(intfc2);
            Assert.IsTrue(intfc2.Failed);
            Assert.IsTrue(!intfc2.Verified);
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample.xml", "Samples")]
        public void VerifyElementTestDevice()
        {
            var intfc1 = new ModelInterfaceSample { VisaInterfaceId = "SAMP0", SiclInterfaceId = "samp", LogicalUnit = "100" };
            var instrument1 = new ModelDeviceSample 
            { 
                Manufacturer = "KT", 
                ModelNumber = "DEV001", 
                SerialNumber = "X01234", 
                FirmwareRevision = "1.0.0.1", 
                IPAddress = "192.168.56.128", 
                DeviceName = "inst0" 
            };
            instrument1.Parent = intfc1;
            instrument1.ParentId = intfc1.PersistentId;
            var hwconfig1 = GenerateHwConfig();
            hwconfig1.VerifyElement(instrument1);
            Assert.IsTrue(!instrument1.Failed);
            Assert.IsTrue(instrument1.Verified);

            var intfc2 = new ModelInterfaceSample { VisaInterfaceId = "SAMP1", SiclInterfaceId = "samp1", LogicalUnit = "101" };
            var instrument2 = new ModelDeviceSample
            {
                Manufacturer = "KT",
                ModelNumber = "DEV001",
                SerialNumber = "X01234",
                FirmwareRevision = "1.0.0.1",
                IPAddress = "192.168.56.130",
                DeviceName = "inst0"
            };
            instrument2.Parent = intfc2;
            instrument2.ParentId = intfc2.PersistentId;
            var hwconfig2 = GenerateHwConfig();
            hwconfig2.VerifyElement(instrument2);
            Assert.IsTrue(instrument2.Failed);
            Assert.IsTrue(!instrument2.Verified);
        }
    }
}
