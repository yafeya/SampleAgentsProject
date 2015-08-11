using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;

namespace Keysight.KCE.IOSamples
{
    public class ModelDeviceSample : ModelDevice
    {
        private const string DEFAULT_DEVICENAME = "inst0";

        public string IPAddress { get; set; }
        public string DeviceName { get; set; }

        public ModelDeviceSample()
        {
            AgentName = SampleIOAgent.GetAgentId();

            IPAddress = string.Empty;
            DeviceName = DEFAULT_DEVICENAME;
        }

        public override bool IsEquivalent(ModelElement element)
        {
            var device = element as ModelDeviceSample;
            var isEqule = (device != null) ? (device.IPAddress == IPAddress && device.DeviceName == DeviceName) : false;
            return isEqule;
        }
        public override bool IsSameState(ModelElement element)
        {
            var device = element as ModelDeviceSample;
            var isSameState = (device != null) ? (device.IPAddress == IPAddress && device.DeviceName == DeviceName) : false;
            return isSameState;
        }
        public override ModelElement MakeCopy()
        {
            var device = new ModelDeviceSample();
            CopyDataTo(device);
            return device;
        }
        protected override void CopyDataTo(ModelElement element)
        {
            base.CopyDataTo(element);
            var device = element as ModelDeviceSample;
            device.IPAddress = IPAddress;
            device.DeviceName = DeviceName;
        }
    }
}
