using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Agilent.ACE2.Shared;
using Agilent.ACE2.Shared.Interface_Definitions;
using Microsoft.Practices.Unity;

namespace Keysight.KCE.IOSamples
{
    public class SampleDiscoveryWatcher
    {
        private const int DEFAULT_INTERVAL = 300;
        private IUnityContainer _container = null;
        private AceModelRestricted _model = null;
        private IRequestService _requestSvr = null;

        public SampleDiscoveryWatcher(IUnityContainer container, AceModelRestricted model, IRequestService rqstSvr)
        {
            _container = container;
            _model = model;
            _requestSvr = rqstSvr;
            RequestDiscoveryAction = RequestDiscovery;
            WaitingInterval = DEFAULT_INTERVAL;
        }

        public Action<ModelInterface> RequestDiscoveryAction { get; set; }
        public int WaitingInterval { get; set; }

        public void Start(CancellationToken token)
        {
            var lastTime = DateTime.Now;
            while (true)
            {
                var currentTime = DateTime.Now;
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if ((currentTime - lastTime).TotalSeconds >= WaitingInterval)
                {
                    var hwconfig = _container.Resolve<IConfigDll>(Consts.SAMPLE_IOAGENT_ID);
                    if (hwconfig != null)
                    {
                        hwconfig.InitializeDll();
                        DiscoverInterfaces(hwconfig);
                        hwconfig.UninitializeDll();
                    }
                }
            }
        }

        private void DiscoverInterfaces(IConfigDll hwconfig)
        {
            var intfcs = _model.GetAllElements().Where(e => e is ModelInterfaceSample).Select(i => i as ModelInterfaceSample);
            var unconfigedIntfcs = _model.GetUnconfigedSampleInterfaces(hwconfig);
            var intfcList = new List<ModelInterfaceSample>();
            intfcList.AddRange(intfcs);
            intfcList.AddRange(unconfigedIntfcs);
            var checkList = intfcList.Where(i => i.AutoDiscoverDevices);
            foreach (var intfc in checkList)
            {
                RequestDiscoveryAction(intfc);
            }
        }

        private void RequestDiscovery(ModelInterface intfc)
        {
            if (_requestSvr != null)
            {
                _requestSvr.RequestDiscovery(intfc);
            }
        }
    }
}
