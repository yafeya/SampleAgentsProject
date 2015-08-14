using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keysight.KCE.IOSamples
{
    internal class Consts
    {
        #region IOAgent Keys
        public const string SAMPLE_IOAGENT_ID = "IoAgentSample";
        public const string TUILIPDRIVER_NAME = "ktsampl";
        public const string CONNECTION_TIMEOUT_KEY = "connTimeout";
        public const string BUS_ADDRESS_KEY = "busAddr";
        public const string IPADDRESS_KEY = "ipAddr";
        public const string DEVICENAME_KEY = "devName";
        public const string DEFAULT_LU = "100";
        public const string SAMPLE_INSTRUMENT = "Sample Instrument";
        public const string SAMPLE_INTERFACE = "Sample Interface";
        public const string EDITABLE_FORMATE = "{0} {1}";
        public const string DEFAULT_INTFC_VISANAME = "SAMP0";
        public const string VISA_PRIFIX = "SAMP";
        public const string SICL_PRIFIX = "samp";
        #endregion

        #region HwDetector Keys
        public const string INFO_ELEMENT = "sampleInfo";
        public const string INTERFACE_ELEMENT = "sampleInterface";
        public const string INSTRUMENT_ELEMENT = "sampleConnection";

        public const string CONNECTION_TIMEOUT_ATTR = "connectionTimeout";
        public const string AUTO_DISCOVER = "autoDiscover";
        public const string BUS_ADDRESS_ATTR = "busAddress";
        public const string VISA_NAME_ATTR = "VisaName";
        public const string SICL_NAME_ATTR = "SiclName";
        public const string LU_ATTR = "LU";

        public const string MANUFACTURE_ATTR = "manufacture";
        public const string MODEL_ATTR = "model";
        public const string SERIAL_NUMBER_ATTR = "sn";
        public const string FIRMWARE_VERSION_ATTR = "firmwareVersion";
        public const string IPADDRESS_ATTR = "ipAddress";
        public const string DEVICE_NAME_ATTR = "deviceName";
        #endregion
    }
}
