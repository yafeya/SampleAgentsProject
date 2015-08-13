using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Agilent.ACE2.Shared;
using Agilent.ACE2.Shared.DataStore;
using Agilent.ACE2.Shared.Interface_Definitions;
using Connectivity.CommonOsAbstractions;
using Keysight.KCE.IOSamples;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleIOAgentTest
{
    [TestClass]
    public class SampleIOAgentTest
    {
        private const string AGENTS_SUBFOLDER = "Samples";
        private static string _TestDirectory;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _TestDirectory = context.TestDeploymentDir;
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        public void GetAddingElementSingleInterfaceTest()
        {
            var sampleInterface = new ModelInterfaceSample();
            sampleInterface.VisaInterfaceId = "SAMP0";
            sampleInterface.SiclInterfaceId = "samp";
            sampleInterface.LogicalUnit = "100";
            var model = GenerateModel(sampleInterface);
            var intfc = GenerateNewSampleInterface(model);

            Assert.IsNotNull(intfc);
            Assert.IsTrue(intfc.VisaInterfaceId == "SAMP1");
            Assert.IsTrue(intfc.SiclInterfaceId == "samp1");
            Assert.IsTrue(intfc.LogicalUnit == "101");
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        public void GetAddingElementMultipleInterfaceTest()
        {
            var sampleInterface1 = new ModelInterfaceSample();
            sampleInterface1.VisaInterfaceId = "SAMP0";
            sampleInterface1.SiclInterfaceId = "samp";
            sampleInterface1.LogicalUnit = "100";
            var sampleInterface2 = new ModelInterfaceSample();
            sampleInterface2.VisaInterfaceId = "SAMP1";
            sampleInterface2.SiclInterfaceId = "samp1";
            sampleInterface2.LogicalUnit = "101";

            var model = GenerateModel(sampleInterface1, sampleInterface2);
            var intfc = GenerateNewSampleInterface(model);

            Assert.IsNotNull(intfc);
            Assert.IsTrue(intfc.VisaInterfaceId == "SAMP2");
            Assert.IsTrue(intfc.SiclInterfaceId == "samp2");
            Assert.IsTrue(intfc.LogicalUnit == "102");
        }

        private static AceModel GenerateModel(params ModelInterfaceSample[] sampleInterfaces)
        {
            var model = new AceModel();
            if (sampleInterfaces != null)
            {
                foreach (var intfc in sampleInterfaces)
                {
                    model.AddElement(intfc);
                }
            }
            return model;
        }

        private static ModelInterfaceSample GenerateNewSampleInterface(AceModel model)
        {
            var container = new UnityContainer();
            var sampleAgent = new SampleIOAgent(container);
            var getAddingElementsMethod = new PrivateObject(sampleAgent);
            var result = getAddingElementsMethod.Invoke("GetAddingElement", BindingFlags.NonPublic | BindingFlags.Instance, "Sample Interface", model);
            var intfc = result as ModelInterfaceSample;
            return intfc;
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        public void GetAddingElementDeviceWithInterfaceTest()
        {
            var sampleInterface = GenerateSampleInterface("SAMP0", "samp", "100");
            var model = GenerateModel(sampleInterface);
            var device = GenerateNewSampleDevice(model);

            Assert.IsNotNull(device);
            Assert.IsTrue(device.Parent is ModelInterface);
            Assert.IsTrue(((ModelInterface)device.Parent).VisaInterfaceId == "SAMP0");
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        public void GetAddingElementDeviceWithoutInterfaceTest()
        {
            var model = GenerateModel();
            var device = GenerateNewSampleDevice(model);

            Assert.IsNotNull(device);
            Assert.IsTrue(device.Parent is ModelInterface);
            Assert.IsTrue(((ModelInterface)device.Parent).VisaInterfaceId == "SAMP0");
        }

        private static ModelDeviceSample GenerateNewSampleDevice(AceModel model)
        {
            var container = new UnityContainer();
            var sampleAgent = new SampleIOAgent(container);
            var getAddingElementsMethod = new PrivateObject(sampleAgent);
            var result = getAddingElementsMethod.Invoke("GetAddingElement", BindingFlags.NonPublic | BindingFlags.Instance, "Sample Device", model);
            var device = result as ModelDeviceSample;
            return device;
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample.xml", "Samples")]
        public void DoDiscoveryTest()
        {
            var container = GenerateContainer();
            var agent = new SampleIOAgent(container);
            var sampleInterface = GenerateSampleInterface("SAMP0", "samp", "100");
            var model = container.Resolve<AceModelRestricted>();
            var elementList = agent.DoDiscovery(sampleInterface, model);
            var intfc = elementList.Where(e => e is ModelInterfaceSample).Select(i => i as ModelInterfaceSample).FirstOrDefault();
            var device = elementList.Where(e => e is ModelDeviceSample).Select(d => d as ModelDeviceSample).FirstOrDefault();

            Assert.IsTrue(elementList.Count == 2);
            Assert.IsTrue(intfc != null);
            Assert.IsTrue(device != null);
            Assert.IsTrue(intfc.VisaInterfaceId == "SAMP0");
            Assert.IsTrue(intfc.SiclInterfaceId == "samp");
            Assert.IsTrue(intfc.LogicalUnit == "100");
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
        public void DoDiscoveryTest_StaticDefined()
        {
            var container1 = GenerateContainer();
            var agent1 = new SampleIOAgent(container1);
            var sampleInterface1 = GenerateSampleInterface("SAMP1", "samp1", "101");
            var model1 = container1.Resolve<AceModelRestricted>();
            var elementList1 = agent1.DoDiscovery(sampleInterface1, model1);
            Assert.IsTrue(elementList1.Where(e =>
                {
                    var intfc = e as ModelInterfaceSample;
                    return intfc != null
                        && intfc.VisaInterfaceId == "SAMP1"
                        && intfc.SiclInterfaceId == "samp1"
                        && intfc.LogicalUnit == "101";
                }).FirstOrDefault() == null);

            var container2 = GenerateContainer();
            var agent2 = new SampleIOAgent(container2);
            var sampleInterface2 = GenerateSampleInterface("SAMP1", "samp1", "101", true);
            var model2 = container1.Resolve<AceModelRestricted>();
            var elementList2 = agent2.DoDiscovery(sampleInterface2, model2);
            Assert.IsTrue(elementList2.Where(e =>
            {
                var intfc = e as ModelInterfaceSample;
                return intfc != null
                    && intfc.VisaInterfaceId == "SAMP1"
                    && intfc.SiclInterfaceId == "samp1"
                    && intfc.LogicalUnit == "101";
            }).FirstOrDefault() != null);
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample.xml", "Samples")]
        public void DoVerifyTest()
        {
            var container = GenerateContainer();
            var agent = new SampleIOAgent(container);
            var sampleInterface1 = GenerateSampleInterface("SAMP0", "samp", "100");
            var sampleDevice1 = GenerateSampleDeice("KT", "DEV001", "X01234", "1.0.0.1", "192.168.56.128", "inst0");
            sampleDevice1.Parent = sampleInterface1;
            var model1 = container.Resolve<AceModelRestricted>();
            var verifiedInterface1 = agent.DoVerify(sampleInterface1, model1);
            var verifiedDevice1 = agent.DoVerify(sampleDevice1, model1);

            var sampleInterface2 = GenerateSampleInterface("SAMP0", "samp", "100");
            var sampleDevice2 = GenerateSampleDeice("KT", "DEV002", "X012356", "1.0.0.1", "192.168.56.45", "inst0");
            sampleDevice2.Parent = sampleInterface2;
            var model2 = container.Resolve<AceModelRestricted>();
            var verifiedInterface2 = agent.DoVerify(sampleInterface2, model2);
            var verifiedDevice2 = agent.DoVerify(sampleDevice2, model2);

            var sampleInterface3 = GenerateSampleInterface("SAMP1", "samp1", "101");
            var sampleDevice3 = GenerateSampleDeice("KT", "DEV001", "X01234", "1.0.0.1", "192.168.56.128", "inst0");
            sampleDevice3.Parent = sampleInterface3;
            var model3 = container.Resolve<AceModelRestricted>();
            var verifiedInterface3 = agent.DoVerify(sampleInterface3, model3);
            var verifiedDevice3 = agent.DoVerify(sampleDevice3, model3);

            var sampleInterface4 = GenerateSampleInterface("SAMP1", "samp1", "101");
            var sampleDevice4 = GenerateSampleDeice("KT", "DEV002", "X012356", "1.0.0.1", "192.168.56.45", "inst0");
            sampleDevice4.Parent = sampleInterface4;
            var model4 = container.Resolve<AceModelRestricted>();
            var verifiedInterface4 = agent.DoVerify(sampleInterface4, model4);
            var verifiedDevice4 = agent.DoVerify(sampleDevice4, model4);

            Assert.IsTrue(verifiedInterface1.Verified);
            Assert.IsTrue(!verifiedInterface1.Failed);

            Assert.IsTrue(verifiedInterface2.Verified);
            Assert.IsTrue(!verifiedInterface2.Failed);

            Assert.IsTrue(!verifiedInterface3.Verified);
            Assert.IsTrue(verifiedInterface3.Failed);

            Assert.IsTrue(!verifiedInterface4.Verified);
            Assert.IsTrue(verifiedInterface4.Failed);

            Assert.IsTrue(verifiedDevice1.Verified);
            Assert.IsTrue(!verifiedDevice1.Failed);

            Assert.IsTrue(!verifiedDevice2.Verified);
            Assert.IsTrue(verifiedDevice2.Failed);

            Assert.IsTrue(!verifiedDevice3.Verified);
            Assert.IsTrue(verifiedDevice3.Failed);

            Assert.IsTrue(!verifiedDevice4.Verified);
            Assert.IsTrue(verifiedDevice4.Failed);
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample4.xml", "Samples")]
        public void StartBackgroundProcessingTest()
        {
            var container = GenerateContainer();
            var agent = new SampleIOAgent(container);
            agent.Initialize();
            IConfigDll config = container.Resolve<IConfigDll>("IoAgentSample");
            var watcher = container.Resolve<SampleDiscoveryWatcher>();
            var discoveredInterfaceList=new List<string>();
            watcher.WaitingInterval = 1;
            watcher.RequestDiscoveryAction = i =>
                {
                    discoveredInterfaceList.Add(i.VisaInterfaceId);
                };
            var hwconfig = config as HwConfigSample;
            hwconfig.SampleFileName = "sample4.xml";
            agent.BackgroundWaiting = 1;

            var cts = new CancellationTokenSource();
            agent.StartBackgroundProcessing(cts.Token);
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(1100);
            }
            cts.Cancel();

            Assert.IsTrue(discoveredInterfaceList.Contains("SAMP0"));
            Assert.IsTrue(discoveredInterfaceList.Count >= 2);
        }

        private static ModelDeviceSample GenerateSampleDeice(string manufacture, string model, string serialNumber, 
            string firmwareVersion, string ipAddress, string deviceName)
        {
            var sampleDevice = new ModelDeviceSample();
            sampleDevice.Manufacturer = manufacture;
            sampleDevice.ModelNumber = model;
            sampleDevice.SerialNumber = serialNumber;
            sampleDevice.FirmwareRevision = firmwareVersion;
            sampleDevice.IPAddress = ipAddress;
            sampleDevice.DeviceName = deviceName;
            return sampleDevice;
        }

        private static ModelInterfaceSample GenerateSampleInterface(string visaID, string siclID, string lu, bool staticDefailed = false)
        {
            var sampleInterface = new ModelInterfaceSample();
            sampleInterface.VisaInterfaceId = visaID;
            sampleInterface.SiclInterfaceId = siclID;
            sampleInterface.LogicalUnit = lu;
            sampleInterface.StaticallyDefined = staticDefailed;
            return sampleInterface;
        }

        private IUnityContainer GenerateContainer()
        {
            var integrationService = new MockIntegrationService { AgentModulesPath = Path.Combine(_TestDirectory, "Samples") };
            var log = new MockAceLog();
            var container = new UnityContainer();
            var goldenModel = new AceModel();
            var modelCopy = new AceModel();
            var nonconfigDataManager = new NonconfigDataManagerRegistry(container);
            var hwconfig = new HwConfigSample(nonconfigDataManager, integrationService, log);
            container.RegisterInstance<IIntegrationService>(integrationService);
            container.RegisterInstance<IAceLog>(log);
            container.RegisterInstance<AceModel>(goldenModel);
            container.RegisterInstance<AceModelRestricted>(modelCopy);
            container.RegisterInstance<IConfigDll>("IoAgentSample", hwconfig);
            container.RegisterInstance<IRequestService>(new MockRequestService());
            return container;
        }
    }
}
