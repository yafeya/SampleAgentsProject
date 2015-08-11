using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agilent.ACE2.Shared;

namespace Keysight.KCE.IOSamples
{
    public class ModelInterfaceSample : ModelInterface
    {
        public int ConnectionTimeout { get; set; }
        public int BusAddress { get; set; }

        public ModelInterfaceSample()
        {
            AgentName = SampleIOAgent.GetAgentId();
            TulipDriverName = SampleIOAgent.GetTulipDriverName();
            AutoDiscoverDevices = true;
            AutoPollForDevices = false;
            ConnectionTimeout = 5000;
            BusAddress = 0;
        }

        #region override
        public override string VisaInterfaceId
        {
            get { return base.VisaInterfaceId; }
            set
            {
                _visaInterfaceId = value.ToUpperInvariant();
                MainBoardNumber = -1;

                // The MainBoardNumber will track the VisaInterfaceid number
                if (!string.IsNullOrEmpty(value))
                {
                    VisaName = _visaInterfaceId;
                    int prefixLength = Consts.VISA_PRIFIX.Length;
                    string boardNumberString = _visaInterfaceId.Substring(prefixLength);
                    int boardNum = -1;
                    if (int.TryParse(boardNumberString, out boardNum)) MainBoardNumber = boardNum;
                }
            }
        }

        public override bool IsEquivalent(ModelElement element)
        {
            var intfc = element as ModelInterfaceSample;
            bool isEqual = (intfc != null) ? VisaName == intfc.VisaName : false;
            return isEqual;
        }
        public override bool IsSameState(ModelElement element)
        {
            var intfc = element as ModelInterfaceSample;
            bool isSameState = base.IsSameState(element);
            isSameState &= (intfc != null) ?
                (ConnectionTimeout == intfc.ConnectionTimeout && BusAddress == intfc.BusAddress)
                : false;
            return isSameState;
        }
        public override ModelElement MakeCopy()
        {
            var intfc = new ModelInterfaceSample();
            CopyDataTo(intfc);
            return intfc;
        }
        protected override void CopyDataTo(ModelElement element)
        {
            base.CopyDataTo(element);
            var intfc = element as ModelInterfaceSample;
            if (intfc == null) return;
            intfc.ConnectionTimeout = ConnectionTimeout;
            intfc.BusAddress = BusAddress;
        }
        #endregion
    }
}
