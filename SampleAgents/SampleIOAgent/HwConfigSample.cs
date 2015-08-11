using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;

namespace Keysight.KCE.IOSamples
{
    public class HwConfigSample : IConfigDll
    {
        #region IConfigDll Members

        public string AgentId
        {
            get { return SampleIOAgent.GetAgentId(); }
        }

        public string TulipDriverName
        {
            get { return SampleIOAgent.GetTulipDriverName(); }
        }

        public int GetAgentVersion()
        {
            return 0;
        }

        public ModelDevice[] GetAvailableDevices(ModelInterface modelInterface)
        {
            throw new NotImplementedException();
        }

        public ModelInterface[] GetAvailableInterfaces()
        {
            throw new NotImplementedException();
        }

        public bool InitializeDll()
        {
            throw new NotImplementedException();
        }

        public void UninitializeDll()
        {
            throw new NotImplementedException();
        }

        public bool UpdateHardwareParameters(ModelElement modelElement, ModelElement updatedElement)
        {
            throw new NotImplementedException();
        }

        public void VerifyElement(ModelElement element)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
