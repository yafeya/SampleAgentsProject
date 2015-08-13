using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;

namespace Keysight.KCE.IOSamples
{
    public static class ModelElementExtensions
    {
        public static bool IsInterfacePresent(this ModelInterfaceSample interfaceToCheck, IEnumerable<ModelInterface> availableInterfaces)
        {
            bool isPresent = false;
            foreach (var availableInterface in availableInterfaces)
            {
                var specificInterface = availableInterface as ModelInterfaceSample;
                if (specificInterface != null
                    && specificInterface.IsEquivalent(interfaceToCheck))
                {
                    isPresent = true;
                    break;
                }
            }
            return isPresent;
        }
        public static bool IsDevicePresent(this ModelDeviceSample deviceToCheck, IEnumerable<ModelDevice> availableDevices)
        {
            bool isPresent = false;
            // If we didn't get a ModelDeviceTcpip or it doesn't have a VisaName, it's not present
            if (deviceToCheck == null || string.IsNullOrWhiteSpace(deviceToCheck.VisaName))
                return false;

            foreach (var availableDevice in availableDevices)
            {
                if (availableDevice.IsEquivalent(deviceToCheck))
                {
                    isPresent = true;
                    break;
                }
            }
            return isPresent;
        }
        public static IEnumerable<ModelInterfaceSample> GetUnconfigedSampleInterfaces(this AceModelRestricted model, IConfigDll hwconfig)
        {
            var unconfigured = Enumerable.Empty<ModelInterfaceSample>();
            if ((hwconfig != null) && (model != null))
            {
                hwconfig.InitializeDll();
                //ModelElement[] configuredInterfaceElements = model.GetInterfacesOfVisaType(this.VisaIntfTypeString);  // EOS: unused, removed.
                var availableInterfaces = hwconfig.GetAvailableInterfaces().Where(i => i is ModelInterfaceSample).Select(i => i as ModelInterfaceSample);
                // TCPIP only configures 1 interface by default, but this pattern supports any number
                unconfigured = (from i in availableInterfaces
                                where model.FindEquivalentElement(i) == null
                                select i);

                hwconfig.UninitializeDll();
            }
            return unconfigured;
        }
    }
}
