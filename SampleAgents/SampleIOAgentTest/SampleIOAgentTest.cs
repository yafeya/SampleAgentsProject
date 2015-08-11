using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Agilent.ACE2.Shared;
using Keysight.KCE.IOSamples;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleIOAgentTest
{
    [TestClass]
    public class SampleIOAgentTest
    {
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
            var sampleInterface = new ModelInterfaceSample();
            sampleInterface.VisaInterfaceId = "SAMP0";
            sampleInterface.SiclInterfaceId = "samp";
            sampleInterface.LogicalUnit = "100";
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
    }
}
