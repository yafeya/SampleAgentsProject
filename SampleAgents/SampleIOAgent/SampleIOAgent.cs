using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Practices.Unity;
using Connectivity.CommonOsAbstractions;
using Agilent.ACE2.Shared;
using Agilent.ACE2.Shared.IoAgents;
using Agilent.Tlo.Kf.Comm;
using Agilent.ACE2.Shared.Interface_Definitions;
using Agilent.ACE2.SharedNames;
using Agilent.ACE2.Shared.Utilities;
using Agilent.ACE2.CommonUtilities;
using System.Text.RegularExpressions;
using Agilent.ACE2.Shared.RemoteAPI;
using System.Threading.Tasks;

namespace Keysight.KCE.IOSamples
{
    public class SampleIOAgent : IoAgentBase, IBasicAgent, IDiscoveryStrategy, IVerifyStrategy, IInterfaceAgent, IManuallyAdd, IDiscoveryBackgroundProcess
    {
        private static readonly string MODELINTERFACE_RECORDTYPE = typeof(ModelInterfaceSample).Name;
        private static readonly string MODELDEVICE_RECORDTYPE = typeof(ModelDeviceSample).Name;

        private IUnityContainer _container = null;
        private IAceLog _log = null;

        public SampleIOAgent(IUnityContainer container)
        {
            _container = container;
        }

        public int BackgroundWaiting { get; set; }

        public static string GetAgentId()
        {
            return Consts.SAMPLE_IOAGENT_ID;
        }
        public static int GetAutoConfigPriority()
        {
            //Used to prioritize the order of AutoConfig. Lower Priority when the number is less.
            return -900;
        }
        public static string GetTulipDriverName()
        {
            return Consts.TUILIPDRIVER_NAME;
        }
        private static void AddInfoToInterface(InterfaceDataRecord raw, ModelInterfaceSample specificIntfc)
        {
            raw.AddProperty(Consts.CONNECTION_TIMEOUT_KEY, specificIntfc.ConnectionTimeout.ToString());
            raw.AddProperty(Consts.BUS_ADDRESS_KEY, specificIntfc.BusAddress.ToString());
        }
        private static void AddInfoToDevice(DeviceDataRecord raw, ModelDeviceSample specificDevice)
        {
            raw.AddProperty(Consts.IPADDRESS_KEY, specificDevice.IPAddress);
            raw.AddProperty(Consts.DEVICENAME_KEY, specificDevice.DeviceName);
        }
        
        private ModelInterface[] GetUnconfigedInterfaces(AceModelRestricted model)
        {
            IConfigDll hwconfig = _container.Resolve<IConfigDll>(AgentName);
            if ((hwconfig != null) || (model != null)) return Enumerable.Empty<ModelInterface>().ToArray();
            return model.GetUnconfigedSampleInterfaces(hwconfig).ToArray();
        }
        private ModelElement GetAddingElement(string deviceName, AceModelRestricted model)
        {
            ModelElement element = null;
            if (deviceName.StartsWith(Consts.SAMPLE_INSTRUMENT, StringComparison.InvariantCultureIgnoreCase))
            {
                element = GenerateAddingDevice(model);
            }
            else if (deviceName.StartsWith(Consts.SAMPLE_INTERFACE, StringComparison.InvariantCultureIgnoreCase))
            {
                element = GenerateAddingInterface(model);
            }
            return element;
        }
        private ModelElement GenerateAddingDevice(AceModelRestricted model)
        {
            var intfc = model.FindElementByVisaName(Consts.DEFAULT_INTFC_VISANAME) as ModelInterfaceSample;
            if (intfc == null)
            {
                intfc = new ModelInterfaceSample();
                intfc.VisaInterfaceId = Consts.DEFAULT_INTFC_VISANAME;
            }
            var device = new ModelDeviceSample();
            device.Parent = intfc;
            device.ParentId = intfc.PersistentId;
            return device;
        }
        private ModelElement GenerateAddingInterface(AceModelRestricted model)
        {
            var intfc = new ModelInterfaceSample();
            var activeInterfaces = model.GetInterfaces().Where(i => i.AgentName == AgentId).Select(e => e as ModelInterfaceSample)
                .OrderBy(i => i.VisaInterfaceId.ToUpper(), new SampleIOAgentComparer(Consts.VISA_PRIFIX));
            var latestVisaInterface = activeInterfaces.LastOrDefault();
            var visaIndex = latestVisaInterface.VisaInterfaceId.ToUpper().GetIndex(Consts.VISA_PRIFIX);

            var siclInterfaces = activeInterfaces.OrderBy(i => i.SiclInterfaceId, new SampleIOAgentComparer(Consts.SICL_PRIFIX));
            var latestSiclInterface = siclInterfaces.LastOrDefault();
            var siclIndex = (latestSiclInterface != null) ? latestVisaInterface.SiclInterfaceId.ToLower().GetIndex(Consts.SICL_PRIFIX) : -1;
            visaIndex++;
            siclIndex = siclIndex >= 0 ? ++siclIndex : 1;
            var visaIntfID = Consts.VISA_PRIFIX.GenerateNewPrifix(visaIndex);
            var siclIntfID = Consts.SICL_PRIFIX.GenerateNewPrifix(siclIndex);

            var luInterfaces = activeInterfaces.OrderBy(i => i.LogicalUnit, new LUComparer());
            var latestLUIntf = luInterfaces.LastOrDefault();
            var lu = Consts.DEFAULT_LU;
            if (latestLUIntf != null)
            {
                int luValue;
                if (int.TryParse(latestLUIntf.LogicalUnit, out luValue))
                {
                    luValue++;
                    lu = luValue.ToString();
                }
                else
                {
                    lu = Consts.DEFAULT_LU;
                }
            }
            else
            {
                lu = Consts.DEFAULT_LU;
            }
            intfc.VisaInterfaceId = visaIntfID;
            intfc.SiclInterfaceId = siclIntfID;
            intfc.LogicalUnit = lu;
            return intfc;
        }
        private ArgMap GetElementRepresentation(ModelElement element)
        {
            var argmap = new ArgMap();
            if (element != null)
            {
                Dictionary<string, string> properties = null;
                if (element is ModelDeviceSample)
                {
                    var device = element as ModelDevice;
                    var deviceData = CreateDeviceRepresentation(device);
                    properties = deviceData.Properties;
                }
                else if (element is ModelDeviceSample)
                {
                    var intfc = element as ModelInterface;
                    var intfcData = CreateInterfaceRepresentation(intfc);
                    properties = intfcData.Properties;
                }
                argmap = ApiDataRepresentation.PropertyDictionaryToArgMap(properties);
            }
            return argmap;
        }
        private ModelElement GetEditintElement(string deviceName, AceModelRestricted model)
        {
            string visa = string.Empty;
            if (deviceName.StartsWith(Consts.SAMPLE_INSTRUMENT, StringComparison.InvariantCultureIgnoreCase))
            {
                visa = GetEditingDeviceVisaName(deviceName);
            }
            else if (deviceName.StartsWith(Consts.SAMPLE_INTERFACE, StringComparison.InvariantCultureIgnoreCase))
            {
                visa = GetEditingInterfaceVisaName(deviceName);
            }
            var element = model.FindElementByVisaName(visa);
            return element;
        }
        private string GetEditingDeviceVisaName(string deviceName)
        {
            var visa = deviceName.Replace(Consts.SAMPLE_INSTRUMENT, string.Empty).Trim();
            return visa;
        }
        private string GetEditingInterfaceVisaName(string deviceName)
        {
            var visa = deviceName.Replace(Consts.SAMPLE_INTERFACE, string.Empty).Trim();
            return visa;
        }
        private ModelElement ConstructDevice(string persistentID, Dictionary<string, string> rawProperties)
        {
            var deviceRecord = new DeviceDataRecord(persistentID);
            deviceRecord.Properties = rawProperties;
            var element = ReconstituteDevice(deviceRecord);
            return element;
        }
        private ModelElement ConstructInterface(string persistentID, Dictionary<string, string> rawProperties)
        {
            var intfcRecord = new InterfaceDataRecord(persistentID);
            intfcRecord.Properties = rawProperties;
            var element = ReconstituteInterface(intfcRecord);
            return element;
        }
        private void FindAndUpdateDevices(IConfigDll hwconfig, List<ModelElement> discoveredList, ModelInterfaceSample intfc, AceModelRestricted model)
        {
            var connectedDevices = hwconfig.GetAvailableDevices(intfc);
            discoveredList.AddRange(connectedDevices);
        }

        #region IBasicAgent Members

        public string AgentId
        {
            get { return GetAgentId(); }
        }

        #endregion

        #region IModule Members

        public void Initialize()
        {
            if (_container == null) return;
            if (_container.IsRegistered<IAceLog>()) _log = _container.Resolve<IAceLog>();

            if (!_container.IsRegistered<IConfigDll>(AgentId))
            {
                _container.RegisterType<IConfigDll, HwConfigSample>(AgentId, new ContainerControlledLifetimeManager());
            }

            _container.RegisterInstance<IBasicAgent>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IDiscoveryStrategy>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IInterfaceAgent>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IVerifyStrategy>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IManuallyAdd>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IDiscoveryBackgroundProcess>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterType<SampleDiscoveryWatcher>(new ContainerControlledLifetimeManager());
        }

        #endregion

        #region IDiscoveryBackgroundProcess Members

        public void StartBackgroundProcessing(CancellationToken token)
        {
            Task backgroundTask = Task.Factory.StartNew(() =>
                {
                    var watcher = _container.Resolve<SampleDiscoveryWatcher>();
                    watcher.Start(token);
                }, token);
        }

        #endregion

        #region IDiscoveryStrategy Members

        public List<ModelElement> DoDiscovery(ModelElement discoveryRoot, AceModelRestricted model)
        {
            // Discover devices on interfaces
            // If discoveryRoot is null or empty discovery devices on all interfaces associated with this agent.
            // If discoveryRoot is specified, verify the presence of the interface and discover all devices on it.
            // - The model paramenter contains elements that are currently known - treat it as read-only
            // - Add only newly discovered or deleted elements to the output list.
            // - Notes:
            //      - Only devices are discovered here. Interfaces are discovered and added to the model in AutoConfig().
            //      - Elements in the output list will later be verified using DoVerify().
            //      - If we are not confident that a discovered device is the same as one already in the model,
            //        then we should copy it to the output List so it will later be verified.

            //string doDiscoveryTitle = string.Format("DoDiscovery of {0}", (discoveryRoot != null) ? discoveryRoot.ToString() : "<null>");
            //LogUtils.LogElements(_log, this, doDiscoveryTitle, model.GetInterfaces()); // Verbose
            if (_log != null && discoveryRoot != null)
            {
                var id = discoveryRoot.PersistentId;
                string msg = string.Format("Begin discovery of '{0}'", id);
                _log.Message(msg, LogUtils.MethodName(this));
            }
            var list = new List<ModelElement>();
            var intfc = discoveryRoot as ModelInterfaceSample;
            if (intfc != null)
            {
                IConfigDll hwconfig = _container.Resolve<IConfigDll>(AgentName);
                hwconfig.InitializeDll();
                var intfcCopy = intfc.MakeCopy() as ModelInterfaceSample;
                var availableIntfcs = hwconfig.GetAvailableInterfaces()
                    .Where(i => i is ModelInterfaceSample)
                    .Select(i => i as ModelInterfaceSample);
                if (intfcCopy.IsInterfacePresent(availableIntfcs) 
                    || intfc.StaticallyDefined)
                {
                    list.Add(intfcCopy);
                    FindAndUpdateDevices(hwconfig, list, intfc, model);
                }
                hwconfig.UninitializeDll();
            }
            return list;
        }

        #endregion

        #region IVerifyStrategy Members

        public ModelElement DoVerify(ModelElement element, AceModelRestricted model)
        {
            // For interface that are not PnP and can come and go, verify that they are still present.
            //   (e.g. RemoteGPIB, etc.)
            // In most cases, nothing will need to be done to verify an interface.
            // Return a copy of the input element.
            // The following values need to be set in the returned element:
            //   Verified = true means it has been through this method
            //   Failed = true / false
            if (element == null) return null;
            var elementCopy = element.MakeCopy();
            IConfigDll hwconfig = _container.Resolve<IConfigDll>(AgentName);
            hwconfig.VerifyElement(elementCopy);
            return elementCopy;
        }

        #endregion

        #region IInterfaceAgent Members

        public string AgentName
        {
            get { return GetAgentId(); }
        }

        public int AutoConfigurePriority
        {
            get { return GetAutoConfigPriority(); }
        }

        public bool CanReconstituteThis(InterfaceDataRecord raw)
        {
            string recordType = raw.GetString(AgentKeys.RecordType);
            string agent = raw.GetString(AgentKeys.AgentName);
            if (agent == string.Empty) agent = AgentId;
            bool canHandle = false;
            if (agent == AgentId && recordType == MODELINTERFACE_RECORDTYPE)
            {
                canHandle = true;
            }
            return canHandle;
        }

        public bool CanReconstituteThis(DeviceDataRecord raw)
        {
            string recordType = raw.GetString(AgentKeys.RecordType);
            string agent = raw.GetString(AgentKeys.AgentName);
            bool canHandle = false;
            if (agent == AgentId && recordType == MODELDEVICE_RECORDTYPE)
            {
                canHandle = true;
            }
            return canHandle;
        }

        public InterfaceDataRecord CreateInterfaceRepresentation(ModelInterface intfc)
        {
            if (intfc == null) return null;
            InterfaceDataRecord raw = new InterfaceDataRecord(intfc.PersistentId);

            if (intfc is ModelInterface)
            {
                // Common parameters
                AddCommonInterfacePropertiesAsNVPairs(intfc, raw);
                raw.AddProperty(AgentKeys.AgentName, AgentId);    // mark it as mine
                raw.AddProperty(AgentKeys.VisaIntfType, VisaIntfTypeString);
            }

            // Unique parameters
            var specificIntfc = intfc as ModelInterfaceSample;
            if (specificIntfc != null)
            {
                AddInfoToInterface(raw, specificIntfc);
            }
            return raw;
        }

        public DeviceDataRecord CreateDeviceRepresentation(ModelDevice device)
        {
            if (device == null) return null;
            DeviceDataRecord raw = new DeviceDataRecord(device.ParentId);
            if (device is ModelDeviceTcpip)
            {
                AddCommonDevicePropertiesAsNVPairs(device, raw);
                raw.AddProperty(AgentKeys.AgentName, AgentId);    // mark it as mine

                var specificDevice = device as ModelDeviceSample;
                if (specificDevice != null)
                {
                    AddInfoToDevice(raw, specificDevice);
                }
            }
            return raw;
        }

        public ModelInterface ReconstituteInterface(InterfaceDataRecord raw)
        {
            if (raw == null) return null;
            if (!CanReconstituteThis(raw)) return null;

            var intfc = new ModelInterfaceSample();
            ExtractCommonInterfacePropertiesFromNVPairs(raw, intfc);
            intfc.ConnectionTimeout = raw.GetInt(Consts.CONNECTION_TIMEOUT_KEY);
            intfc.BusAddress = raw.GetInt(Consts.BUS_ADDRESS_KEY);
            intfc.Dirty = false;

            return intfc;
        }

        public ModelDevice ReconstituteDevice(DeviceDataRecord raw)
        {
            if (raw == null) return null;
            if (!CanReconstituteThis(raw)) return null;

            var device = new ModelDeviceSample();
            ExtractCommonDevicePropertiesFromNVPairs(raw, device);
            device.IPAddress = raw.GetString(Consts.IPADDRESS_KEY);
            device.DeviceName = raw.GetString(Consts.DEVICENAME_KEY);
            device.Dirty = false;

            return device;
        }

        public ModelInterface[] AutoConfig(AceModelRestricted model)
        {
            var interfacces = GetUnconfigedInterfaces(model);
            return interfacces;
        }

        public bool EnableExportDefault
        {
            get { return true; }
        }

        public string VisaIntfTypeString
        {
            get { return Consts.VISA_PRIFIX; }
        }

        #endregion

        #region IManuallyAdd Members

        public List<string> GetManuallyAddableDevices(AceModelRestricted model)
        {
            var availableList = new List<string>();
            availableList.Add(Consts.SAMPLE_INSTRUMENT);
            availableList.Add(Consts.SAMPLE_INTERFACE);
            return availableList;
        }

        public List<string> GetManuallyEditableDevices(AceModelRestricted model)
        {
            List<string> editableList = new List<string>();
            var manualDevs = (from e in model.GetDevices()
                              where (e is ModelDeviceSample) && (e.StaticallyDefined == true)
                              select string.Format(Consts.EDITABLE_FORMATE, Consts.SAMPLE_INSTRUMENT, e.VisaName)).ToList<string>();
            var manualIntfcs = (from e in model.GetInterfaces()
                                where (e is ModelInterfaceSample)
                                select string.Format(Consts.EDITABLE_FORMATE, Consts.SAMPLE_INTERFACE, e.VisaName)).ToList<string>();

            editableList.AddRange(manualDevs);
            editableList.AddRange(manualIntfcs);
            return editableList;
        }

        public ArgMap GetManualAddParameters(string deviceName, AceModelRestricted model)
        {
            var element = GetAddingElement(deviceName, model);
            var argmap = GetElementRepresentation(element);
            return argmap;
        }

        public ArgMap GetEditableParameters(string deviceName, AceModelRestricted model)
        {
            var element = GetEditintElement(deviceName, model);
            var argmap = GetElementRepresentation(element);
            return argmap;
        }

        public ModelElement ConstructElement(string name, ArgMap parameters, AceModelRestricted model)
        {
            ModelElement element = null;
            Dictionary<string, string> rawProperties = ApiDataRepresentation.ArgMapToPropertyDictionary(parameters);
            string id = rawProperties[Ace2ApiConstants.PersistentId_Parameter];

            if (name.StartsWith(Consts.SAMPLE_INSTRUMENT)
                || name.Contains("::"))
            {
                element = ConstructDevice(id, rawProperties);
            }
            else
            {
                element = ConstructInterface(id, rawProperties);
            }

            if (element != null && element.Parent == null && element.ParentId.Length > 0)
            {
                element.Parent = model.FindElementById(element.ParentId);
            }
            return element;
        }

        public List<ModelElement> DeleteDevice(string name, AceModelRestricted model)
        {
            return DefaultDeleteAction.DefaultDeleteDevice(name, model);
        }

        #endregion
    }
}
