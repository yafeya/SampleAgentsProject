using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Agilent.ACE2.Shared;

namespace Keysight.KCE.IOSamples
{
    public class SampleHardwareDetector
    {
        private List<ModelElement> _modelList = new List<ModelElement>();
        private const string PRIFIX_FILENAME = "~";

        public SampleHardwareDetector(string fileName)
        {
            DetectHardware(fileName);
        }
        public IEnumerable<ModelElement> GetAll()
        {
            var models = Enumerable.Empty<ModelElement>();
            Monitor.Enter(_modelList);
            models = _modelList.ToArray();
            Monitor.Exit(_modelList);
            return models;
        }
        public IEnumerable<ModelInterfaceSample> GetConnectedInterfaces()
        {
            var availableIntfcs = Enumerable.Empty<ModelInterfaceSample>();
            Monitor.Enter(_modelList);
            availableIntfcs = _modelList.Where(e => e is ModelInterfaceSample).Select(e => (ModelInterfaceSample)e);
            Monitor.Exit(_modelList);
            return availableIntfcs;
        }
        public IEnumerable<ModelDeviceSample> GetConnectedInstruments(ModelInterfaceSample intfc)
        {
            var availableIntfcs = Enumerable.Empty<ModelDeviceSample>();
            Monitor.Enter(_modelList);
            availableIntfcs = _modelList.Where(e => e is ModelDeviceSample && ((ModelDeviceSample)e).Parent.IsEquivalent(intfc))
                .Select(e => (ModelDeviceSample)e);
            Monitor.Exit(_modelList);
            return availableIntfcs;
        }

        private void DetectHardware(string fileName)
        {
            var tempPath = Path.GetTempPath();
            var builder = new StringBuilder();
            builder.Append(PRIFIX_FILENAME).Append(Path.GetFileName(fileName));
            var tempFileName = Path.Combine(tempPath, builder.ToString());
            File.Copy(fileName, tempFileName, true);
            var document = XDocument.Load(tempFileName);
            var infoElement = document.Element(Consts.INFO_ELEMENT);
            if (infoElement == null) return;
            var intfcElements = infoElement.Elements(Consts.INTERFACE_ELEMENT);
            foreach (var intfcElement in intfcElements)
            {
                var intfc = ConstructInterface(intfcElement);
                if (AddElement(intfc))
                {
                    var instrumentElements = intfcElement.Elements(Consts.INSTRUMENT_ELEMENT);
                    foreach (var instrumentElement in instrumentElements)
                    {
                        var instrument = ConstructInstrument(instrumentElement);
                        instrument.Parent = intfc;
                        instrument.ParentId = intfc.PersistentId;
                        AddElement(instrument);
                    }
                }
            }
            File.Delete(tempFileName);
        }
        private ModelInterfaceSample ConstructInterface(XElement intfcElement)
        {
            ModelInterfaceSample intfc = null;
            var connectionTimeoutAttr = intfcElement.Attribute(Consts.CONNECTION_TIMEOUT_ATTR);
            var busAddressAttr = intfcElement.Attribute(Consts.BUS_ADDRESS_ATTR);
            var visaNameAttr = intfcElement.Attribute(Consts.VISA_NAME_ATTR);
            var siclNameAttr = intfcElement.Attribute(Consts.SICL_NAME_ATTR);
            var luAttr = intfcElement.Attribute(Consts.LU_ATTR);
            if (connectionTimeoutAttr != null
                && busAddressAttr != null
                && visaNameAttr != null
                && siclNameAttr != null
                && luAttr != null)
            {
                int connectionTimeout, busAddress, lu;
                intfc = new ModelInterfaceSample();
                intfc.VisaInterfaceId = visaNameAttr.Value;
                intfc.SiclInterfaceId = siclNameAttr.Value;
                intfc.ConnectionTimeout = int.TryParse(connectionTimeoutAttr.Value, out connectionTimeout) ? connectionTimeout : 5000;
                intfc.BusAddress = int.TryParse(busAddressAttr.Value, out busAddress) ? busAddress : 0;
                intfc.LogicalUnit = int.TryParse(luAttr.Value, out lu) ? luAttr.Value : "100";
            }
            return intfc;
        }
        private ModelDeviceSample ConstructInstrument(XElement instrumentElement)
        {
            ModelDeviceSample device = null;
            var manufactureAttr = instrumentElement.Attribute(Consts.MANUFACTURE_ATTR);
            var modelAttr = instrumentElement.Attribute(Consts.MODEL_ATTR);
            var serialNumberAttr = instrumentElement.Attribute(Consts.SERIAL_NUMBER_ATTR);
            var firmwareVersionAttr = instrumentElement.Attribute(Consts.FIRMWARE_VERSION_ATTR);
            var ipAddressAttr = instrumentElement.Attribute(Consts.IPADDRESS_ATTR);
            var deviceNameAttr = instrumentElement.Attribute(Consts.DEVICE_NAME_ATTR);
            if (manufactureAttr != null
                && modelAttr != null
                && serialNumberAttr != null
                && firmwareVersionAttr != null
                && ipAddressAttr != null
                && deviceNameAttr != null)
            {
                device = new ModelDeviceSample();
                device.Manufacturer = manufactureAttr.Value;
                device.ModelNumber = modelAttr.Value;
                device.SerialNumber = serialNumberAttr.Value;
                device.FirmwareRevision = firmwareVersionAttr.Value;
                device.IPAddress = ipAddressAttr.Value;
                device.DeviceName = deviceNameAttr.Value;
            }
            return device;
        }
        private bool AddElement(ModelElement element)
        {
            bool success = false;
            if (element != null && !Contains(element))
            {
                Monitor.Enter(_modelList);
                _modelList.Add(element);
                Monitor.Exit(_modelList);
                success = true;
            }
            return success;
        }
        private bool Contains(ModelElement element)
        {
            bool isContains = false;
            Monitor.Enter(_modelList);
            var existed = _modelList.Where(e => e.IsEquivalent(element)).FirstOrDefault();
            isContains = existed != null;
            Monitor.Exit(_modelList);
            return isContains;
        }
    }
}
