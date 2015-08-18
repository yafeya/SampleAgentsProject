using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Unity;
using ACE2_UI.Interface_Definitions;
using ACE2_UI.Model;
using ACE2_UI.ModulesUiAgents.UiAgentBase;
using ACE2_UI.Services;
using Agilent.ACE2.SharedNames;
using System.Collections.ObjectModel;
using ACE2_UI.Infrastructure.Commands;
using ACE2_UI.Utilities;
using System.Windows;
using System.Windows.Threading;

namespace Keysight.KCE.UISamples
{
    public class SampleAddEditInstrumentViewModel : UiAgentBaseViewModel, IHelper
    {
        private ObservableCollection<string> _usableVisaInterfaceIds = new ObservableCollection<string>();

        public SampleAddEditInstrumentViewModel(IUnityContainer container, Ace2ApiServiceModule api, ILoggerFacade log,
            UiModel model, IEventAggregator eventAggregator)
            : base(container, api, log, eventAggregator)
        { }

        #region Properties
        public string IPAddress
        {
            get
            {
                string hostname = _editData.GetString(Consts.IPADDRESS_KEY);
                return string.IsNullOrEmpty(hostname) ? string.Empty : hostname;
            }
            set
            {
                _editData.SetString(Consts.IPADDRESS_KEY, value);
                SetDirty();
                GeneratedVisaAddress = GenerateVisaAddress();
                ResetTestMessage();
                UpdateTestStatus();
                OnPropertyChanged("IPAddress");
            }
        }
        public string InstrumentName
        {
            get
            {
                string instrumentName = _editData.GetString(Consts.DEVICENAME_KEY);
                return string.IsNullOrEmpty(instrumentName) ? string.Empty : instrumentName;
            }
            set
            {
                _editData.SetString(Consts.DEVICENAME_KEY, value);
                SetDirty();
                GeneratedVisaAddress = GenerateVisaAddress();
                ResetTestMessage();
                UpdateTestStatus();
                OnPropertyChanged("InstrumentName");
            }
        }
        public ObservableCollection<string> UsableVisaInterfaceIds
        {
            get { return _usableVisaInterfaceIds; }
        }
        public string ParentVisaInterfaceId
        {
            get
            {
                string vId = GetPropertyDataString(AgentKeys.ParentVisaInterfaceId);
                if (vId == null) vId = string.Empty;
                return vId;
            }
            set
            {
                if (value == null) return; // invalid value
                SetPropertyDataString(AgentKeys.ParentVisaInterfaceId, value);
                // set the NewVisaInterfaceId so the server will note the parent change.
                SetPropertyDataString(AgentKeys.NewVisaInterfaceId, value);
                SetDirty();
                GeneratedVisaAddress = GenerateVisaAddress();
                ResetTestMessage();
                UpdateTestStatus();
                OnPropertyChanged("ParentVisaInterfaceId");
            }
        }
        #endregion

        #region Methods

        #region Override Methods
        public override void LocalInit()
        {
            Header = IsEdit() ? SR.GetString("HeaderEditSampleDevice") : SR.GetString("HeaderAddSampleDevice");
            if (Api == null)
            {
                UsableVisaInterfaceIds.Clear();
                UsableVisaInterfaceIds.Add(ParentVisaInterfaceId);
            }
            else
            {
                Api.GetAllUsedInterfaceIdsAsync(arg =>
                {
                    Action update = () =>
                    {
                        var allIds = ExtractUsedInterfaceIdsFromArgMap(arg, Consts.SAMPLE_VISA_PREFIX);
                        UsableVisaInterfaceIds.Clear();
                        foreach (var id in allIds.Item1.OrderBy(x => x, new NaturalStringComparer()))
                        {
                            UsableVisaInterfaceIds.Add(id);
                        }
                    };
                    Application.Current.Dispatcher.BeginInvoke(update, DispatcherPriority.Normal);
                },
                msg =>
                {
                    if (Log != null) Log.Log(string.Format("Unable to get Sample instruments IDs: {0}", msg), Category.Warn, Priority.Low);
                });
            }
            base.LocalInit();
        }
        public override string GenerateVisaAddress()
        {
            var builder = new StringBuilder();
            builder.Append(ParentVisaInterfaceId)
                .Append(Consts.VISA_CONNECTOR)
                .Append(IPAddress)
                .Append(Consts.VISA_CONNECTOR)
                .Append(InstrumentName)
                .Append(Consts.VISA_CONNECTOR)
                .Append(Consts.VISA_INSTRUMENT);
            return builder.ToString();
        }
        public override void DoDelete()
        {
            string address = GetPropertyDataString(AgentKeys.VisaAddress);
            GlobalCommands.DeleteInstrumentCommand.Execute(address);
            base.DoDelete();
        }
        public override bool CanDoDelete()
        {
            string address = GetPropertyDataString(AgentKeys.VisaAddress);
            if (!string.IsNullOrEmpty(address))
                return GlobalCommands.DeleteInstrumentCommand.CanExecute(address);
            return base.CanDoDelete();
        }
        public override bool CanDoAccept()
        {
            return base.CanDoAccept()
                && !string.IsNullOrEmpty(IPAddress)
                && !string.IsNullOrEmpty(ParentVisaInterfaceId);
        }
        #endregion

        #region Private Methods
        #endregion

        #endregion

        #region IHelper Members

        public string GetHelpLink()
        {
            var builder = new StringBuilder();
            builder.Append(Ace2ApiConstants.MmiCustomHelpPrefix).Append(Consts.DEFAULT_HELP_PAGE);
            return builder.ToString();
        }

        #endregion
    }
}
