using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;
using Agilent.ACE2.SharedNames;
using Connectivity.CommonOsAbstractions;

namespace Keysight.KCE.IOSamples
{
    public class HwConfigSample : IConfigDll
    {
        private const string SAMPLE_FILENAME = "sample.xml";
        private string _tulipDriverParams = string.Empty;
        /*
         * Below is an example of the tulip driver parameters:
         * LU,Name,Interface,Slot,BusAddr,sicl_infinity,lan_delta,sigio,log_errors,reserved,lanProtocol,ConnectTimeoutMs,TcpPortmapRequest,LanDeltaMs
         */
        private INonconfigDataManager _nonconfigDataManager = null;
        private IAceLog _log = null;
        private IIntegrationService _integrationSvc = null;
        private bool _tulipParmsInitialized = false;

        public HwConfigSample(INonconfigDataManager nonconfigDataManager, IIntegrationService integrationSvc, IAceLog log)
        {
            _nonconfigDataManager = nonconfigDataManager;
            _integrationSvc = integrationSvc;
            _log = log;
            SampleFileName = SAMPLE_FILENAME;
        }
        public string SampleFileName { get; set; }

        private SampleHardwareDetector GenerateHardwareDetector()
        {
            SampleHardwareDetector detector = null;
            var fileName = Path.Combine(_integrationSvc.GetIOAgentIntegrationPath(), SampleFileName);
            if (File.Exists(fileName))
            {
                detector = new SampleHardwareDetector(fileName);
            }
            return detector;
        }

        private void InitializeUnmanagedDll()
        {
            //ToDo: If you have some unmanaged Dll, please initialized here.
        }
        /// <summary>
        /// Add the TULIP params entry for this interface to persistent storage.
        ///    For USB this is:
        ///       Name       Value
        ///       =========  ===================================================
        ///       agUsb488   LU,Name,Interface,Slot,BusAddr,TimeoutCPRThreshold
        ///       Where 'Name' is the TULIP driver name without the trailing '32.dll'
        /// </summary>
        private void AddParamsEntry(NonconfigData dataManager)
        {
            //ToDo: If you have the tulip driver, add the parameters here.
            // dataManager.AddTulipParam(TulipDriverName, _tulipDriverParams);
        }
        private void VerfiyInterface(ModelInterfaceSample intfc)
        {
            ModelInterface[] availableInterfaces = GetAvailableInterfaces();
            if (IsInterfacePresentProc(intfc, availableInterfaces))
            {
                intfc.Verified = true;
                intfc.Failed = false;
            }
            else
            {
                intfc.FailedReason = AgentKeys.FailedReason_InterfaceNotPresent;
            }
        }
        private void VerifyDevice(ModelDeviceSample device)
        {
            var parentElement = device.Parent;
            if (parentElement != null)
            {
                var parentInterface = parentElement as ModelInterfaceSample;
                if (parentInterface != null)
                {
                    // Check if the device is still present
                    if (IsDevicePresentProc(device, parentInterface))
                    {
                        device.Verified = true;
                        device.Failed = false;
                    }
                    else
                    {
                        device.FailedReason = AgentKeys.FailedReason_DeviceNotVerified;
                    }
                }
                else
                {
                    device.FailedReason = AgentKeys.FailedReason_NoVisaInterface;
                }
            }
        }
        private bool IsInterfacePresentProc(ModelInterfaceSample interfaceToCheck, ModelInterface[] availableInterfaces)
        {
            return interfaceToCheck.IsInterfacePresent(availableInterfaces);
        }
        private bool IsDevicePresentProc(ModelDeviceSample deviceToCheck, ModelInterfaceSample parentInterface)
        {
            bool isPresent = false;
            var hwDetector = GenerateHardwareDetector();
            if (hwDetector != null)
            {
                var availableDevices = hwDetector.GetConnectedInstruments(parentInterface);
                isPresent = deviceToCheck.IsDevicePresent(availableDevices);
            }
            return isPresent;
        }

        #region IConfigDll Members

        public string AgentId
        {
            get { return Consts.SAMPLE_IOAGENT_ID; }
        }

        public string TulipDriverName
        {
            get { return Consts.TUILIPDRIVER_NAME; }
        }

        public int GetAgentVersion()
        {
            return 0;
        }

        public ModelInterface[] GetAvailableInterfaces()
        {
            var result = new ModelInterface[] { };
            var hwDetector = GenerateHardwareDetector();
            if (hwDetector != null)
            {
                var intfcs = hwDetector.GetConnectedInterfaces();
                result = intfcs.ToArray();
            }
            return result;
        }

        public ModelDevice[] GetAvailableDevices(ModelInterface modelInterface)
        {
            var result = new ModelDevice[] { };
            var hwDetector = GenerateHardwareDetector();
            if (hwDetector != null)
            {
                var intfc = modelInterface as ModelInterfaceSample;
                if (intfc != null)
                {
                    var devices = hwDetector.GetConnectedInstruments(intfc);
                    result = devices.ToArray();
                }
            }
            return result;
        }

        public bool InitializeDll()
        {
            /*
             * This method is to initialize the non-config data.
             * Non-Config Data is a container to hold the data from our persistent layer.
             * Like Favorite, VisaInfo, etc.
            */
            bool success = false;
            if (_tulipParmsInitialized == false)
            {
                if (_nonconfigDataManager != null)
                {
                    NonconfigData dataManager = _nonconfigDataManager.GetNonconfigData();
                    if (dataManager != null)
                    {
                        AddParamsEntry(dataManager);
                        _nonconfigDataManager.SaveNonconfigData(dataManager);
                        _tulipParmsInitialized = true;
                    }
                }
            }
            InitializeUnmanagedDll();
            return success;
        }

        public void UninitializeDll()
        {
            // IOAGENT_PORT: UninitializeDll- Call into unmanaged DLL if needed here
        }

        public bool UpdateHardwareParameters(ModelElement modelElement, ModelElement updatedElement)
        {
            /*
             * modelElment is the element in the model, updated Element is the element discovered in the dicovery process.
             * If they are in the different state, you can update model element at here.
             * Whether this method is call is on your decision. 
             * In our code it always not be called, sometimes it will be called at the interface verify process.
            */
            return true;
        }

        public void VerifyElement(ModelElement element)
        {
            element.Verified = false; // True because we have performed the verify step
            element.Failed = true;   // Presume failure unless we determine otherwise
            if (element is ModelInterfaceSample)
            {
                var intfc = element as ModelInterfaceSample;
                VerfiyInterface(intfc);
            }
            else if (element is ModelDeviceSample)
            {
                var device = element as ModelDeviceSample;
                VerifyDevice(device);
            }
            else
            {
                element.FailedReason = AgentKeys.FailedReason_InvalidElementType;
            }
        }

        #endregion
    }
}
