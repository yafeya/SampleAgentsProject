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
        private const string VISA_INSTRUMENT = "INSTR";
        private const string VISA_CONNECTOR = "::";
        private const string SICL_CONNECTOR = ":";
        private const string LEFT_BRACKET = "[";
        private const string RIGHT_BRACKET = "]";

        public string IPAddress { get; set; }
        public string DeviceName { get; set; }

        public override string VisaName
        {
            get 
            {
                if (string.IsNullOrEmpty(base.VisaName))
                {
                    base.VisaName = BuildVISAAddress();
                }
                return base.VisaName;
            }
        }
        public override string SiclAddress
        {
            get
            {
                if (string.IsNullOrEmpty(base.SiclAddress))
                {
                    base.SiclAddress = BuildSICLAddress();
                }
                return base.SiclAddress;
            }
        }

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

        private string BuildVISAAddress()
        {
            var parentIntfc = Parent as ModelInterfaceSample;
            if (parentIntfc == null) throw new ArgumentNullException();
            var builder = new StringBuilder();
            builder.Append(parentIntfc.VisaInterfaceId)
                .Append(VISA_CONNECTOR)
                .Append(IPAddress)
                .Append(VISA_CONNECTOR)
                .Append(DeviceName)
                .Append(VISA_CONNECTOR)
                .Append(VISA_INSTRUMENT);
            return builder.ToString();
        }
        private string BuildSICLAddress()
        {
            var parentIntfc = Parent as ModelInterfaceSample;
            if (parentIntfc == null) throw new ArgumentNullException();
            var builder = new StringBuilder();
            builder.Append(parentIntfc.SiclInterfaceId)
                .Append(LEFT_BRACKET)
                .Append(IPAddress)
                .Append(RIGHT_BRACKET)
                .Append(SICL_CONNECTOR)
                .Append(DeviceName);
            return builder.ToString();
        }
    }
}
