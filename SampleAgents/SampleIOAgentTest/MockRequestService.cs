using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;
using Agilent.ACE2.Shared.Interface_Definitions;
using Microsoft.Practices.Unity;

namespace SampleIOAgentTest
{
    public class MockRequestService : IRequestService
    {
        #region IRequestService Members

        public string GetVisaAddressByAliasName(string alias)
        {
            return string.Empty;
        }

        public Agilent.ACE2.Shared.VisaAliases GetVisaAliases()
        {
            return new VisaAliases();
        }

        public void RequestAutoConfig(Agilent.ACE2.Shared.IInterfaceAgent agent)
        { }

        public Microsoft.Practices.Unity.IUnityContainer RequestContainer()
        {
            return new UnityContainer();
        }

        public void RequestDataUpdate(Agilent.ACE2.Shared.Discovery.WorkItem item)
        { }

        public void RequestDiscovery(Agilent.ACE2.Shared.Discovery.WorkItem work)
        { }

        public void RequestDiscovery(Agilent.ACE2.Shared.ModelElement element)
        { }

        public Agilent.ACE2.Shared.AceModelRestricted RequestModel()
        {
            return new AceModel();
        }

        public void RequestNonTraditionalDiscovery()
        { }

        public void RequestPublishServerMessage(Agilent.ACE2.Shared.MessageRecord record)
        { }

        public void RequestVerify(Agilent.ACE2.Shared.ModelElement element)
        { }

        public bool UpdateVisaAlias(Agilent.ACE2.Shared.ModelDevice beforeUpdate, Agilent.ACE2.Shared.ModelDevice afterUpdate)
        {
            return false;
        }

        #endregion
    }
}
