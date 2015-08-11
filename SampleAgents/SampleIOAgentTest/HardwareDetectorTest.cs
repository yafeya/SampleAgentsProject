using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Keysight.KCE.IOSamples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleIOAgentTest
{
    [TestClass]
    public class HardwareDetectorTest
    {
        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample1.xml",@"Samples")]
        public void DetectTest_Sample1()
        {
            var fileName = @"Samples\sample1.xml";
            Assert.IsTrue(File.Exists(fileName));
            var detector = new SampleHardwareDetector(fileName);
            var all = detector.GetAll();
            var intfcs = detector.GetConnectedInterfaces();
            var intfc = intfcs.FirstOrDefault();
            var instruments = detector.GetConnectedInstruments(intfc);
            var instrument = instruments.FirstOrDefault();

            Assert.IsTrue(all.Where(e => e is ModelInterfaceSample).Count() == 1);
            Assert.IsTrue(intfcs.Count() == 1);
            Assert.IsTrue(intfc.VisaInterfaceId == "SAMP0");
            Assert.IsTrue(intfc.SiclInterfaceId == "samp");
            Assert.IsTrue(intfc.ConnectionTimeout == 10000);
            Assert.IsTrue(intfc.BusAddress == 1);
            Assert.IsTrue(intfc.LogicalUnit == "100");
            Assert.IsTrue(instruments.Count() == 1);
            Assert.IsTrue(instrument.Manufacturer == "KT");
            Assert.IsTrue(instrument.ModelNumber == "DEV001");
            Assert.IsTrue(instrument.SerialNumber == "X01234");
            Assert.IsTrue(instrument.FirmwareRevision == "1.0.0.1");
            Assert.IsTrue(instrument.IPAddress == "192.168.56.128");
            Assert.IsTrue(instrument.DeviceName == "inst0");
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample2.xml", @"Samples")]
        public void DetectTest_Sample2()
        {
            var fileName = @"Samples\sample2.xml";
            Assert.IsTrue(File.Exists(fileName));
            var detector = new SampleHardwareDetector(fileName);
            var all = detector.GetAll();
            var intfcs = detector.GetConnectedInterfaces();
            var intfc1 = intfcs.FirstOrDefault();
            var intfc2 = intfcs.LastOrDefault();
            var instruments1 = detector.GetConnectedInstruments(intfc1);
            var instrument1 = instruments1.FirstOrDefault();
            var instruments2 = detector.GetConnectedInstruments(intfc2);
            var instrument2 = instruments2.FirstOrDefault();

            Assert.IsTrue(all.Where(e => e is ModelInterfaceSample).Count() == 2);
            Assert.IsTrue(intfcs.Count() == 2);
            Assert.IsTrue(intfc1.VisaInterfaceId == "SAMP0");
            Assert.IsTrue(intfc1.SiclInterfaceId == "samp");
            Assert.IsTrue(intfc1.ConnectionTimeout == 15000);
            Assert.IsTrue(intfc1.BusAddress == 10);
            Assert.IsTrue(intfc1.LogicalUnit == "100");
            Assert.IsTrue(instruments1.Count() == 1);
            Assert.IsTrue(instrument1.Manufacturer == "Keysight");
            Assert.IsTrue(instrument1.ModelNumber == "Sample001");
            Assert.IsTrue(instrument1.SerialNumber == "X01234");
            Assert.IsTrue(instrument1.FirmwareRevision == "1.0.0.1");
            Assert.IsTrue(instrument1.IPAddress == "192.168.56.128");
            Assert.IsTrue(instrument1.DeviceName == "inst0");

            Assert.IsTrue(intfc2.VisaInterfaceId == "SAMP1");
            Assert.IsTrue(intfc2.SiclInterfaceId == "samp1");
            Assert.IsTrue(intfc2.ConnectionTimeout == 5000);
            Assert.IsTrue(intfc2.BusAddress == 0);
            Assert.IsTrue(intfc2.LogicalUnit == "101");
            Assert.IsTrue(instruments2.Count() == 1);
            Assert.IsTrue(instrument2.Manufacturer == "Keysight");
            Assert.IsTrue(instrument2.ModelNumber == "Sample002");
            Assert.IsTrue(instrument2.SerialNumber == "X01234");
            Assert.IsTrue(instrument2.FirmwareRevision == "1.0.0.1");
            Assert.IsTrue(instrument2.IPAddress == "192.168.56.130");
            Assert.IsTrue(instrument2.DeviceName == "inst0");
        }

        [TestMethod()]
        [TestCategory("SampleIOAgent")]
        [DeploymentItem(@"Samples\sample3.xml", @"Samples")]
        public void DetectTest_Sample3()
        {
            var fileName = @"Samples\sample3.xml";
            Assert.IsTrue(File.Exists(fileName));
            var detector = new SampleHardwareDetector(fileName);
            var all = detector.GetAll();
            var intfcs = detector.GetConnectedInterfaces();
            var intfc = intfcs.FirstOrDefault();
            var instruments = detector.GetConnectedInstruments(intfc);
            var instrument = instruments.FirstOrDefault();

            Assert.IsTrue(all.Where(e => e is ModelInterfaceSample).Count() == 1);
            Assert.IsTrue(intfcs.Count() == 1);
            Assert.IsTrue(intfc.VisaInterfaceId == "SAMP0");
            Assert.IsTrue(intfc.SiclInterfaceId == "samp");
            Assert.IsTrue(intfc.ConnectionTimeout == 5000);
            Assert.IsTrue(intfc.BusAddress == 0);
            Assert.IsTrue(intfc.LogicalUnit == "100");
            Assert.IsTrue(instruments.Count() == 1);
            Assert.IsTrue(instrument.Manufacturer == "Keysight");
            Assert.IsTrue(instrument.ModelNumber == "Sample001");
            Assert.IsTrue(instrument.SerialNumber == "X01234");
            Assert.IsTrue(instrument.FirmwareRevision == "1.0.0.1");
            Assert.IsTrue(instrument.IPAddress == "192.168.56.128");
            Assert.IsTrue(instrument.DeviceName == "inst0");
        }
    }
}
