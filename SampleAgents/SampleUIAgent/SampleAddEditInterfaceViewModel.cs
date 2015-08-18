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
using System.Windows;
using Agilent.Tlo.Kf.Comm;
using Agilent.ACE2.SharedNames;
using ACE2_UI.Infrastructure;
using ACE2_UI.Infrastructure.Commands;

namespace Keysight.KCE.UISamples
{
    public class SampleAddEditInterfaceViewModel : UiAgentBaseViewModel, IHelper
    {
        private List<string> _errorList = new List<string>();

        public SampleAddEditInterfaceViewModel(IUnityContainer container, Ace2ApiServiceModule api,
            ILoggerFacade log, IEventAggregator eventAggregator)
            : base(container, api, log, eventAggregator)
        { }

        #region Properites

        public Visibility AddConnectionAddressesVisibility { get; set; }

        public Visibility EditConnectionAddressesVisibility { get; set; }

        public int ConnectionTimeout
        {
            get { return _editData.GetInt(Consts.CONNECTION_TIMEOUT_KEY); }
            set
            {
                _editData.SetInt(Consts.CONNECTION_TIMEOUT_KEY, value);
                SetDirty();
                OnPropertyChanged("ConnectionTimeout");
            }
        }

        public int BusAddress
        {
            get { return _editData.GetInt(Consts.BUS_ADDRESS_KEY); }
            set
            {
                _editData.SetInt(Consts.BUS_ADDRESS_KEY, value);
                SetDirty();
                OnPropertyChanged("BusAddress");
            }
        }

        #endregion

        #region Methods

        #region Override Methods

        public override void LocalInit()
        {
            AddConnectionAddressesVisibility = IsEdit() ? Visibility.Collapsed : Visibility.Visible;
            EditConnectionAddressesVisibility = IsEdit() ? Visibility.Visible : Visibility.Collapsed;
            Header = IsEdit() ? SR.GetString("HeaderEditSampleInterface") : SR.GetString("HeaderAddSampleInterface");
            if (Api == null)
            {
                AvailableSiclIds.Clear();
                AvailableVisaIds.Clear();
                return;
            }

            Api.GetAllUsedInterfaceIdsAsync(arg =>
                {
                    if (arg == null) return;
                    var allIds = ExtractUsedInterfaceIdsFromArgMap(arg, Consts.SAMPLE_VISA_PREFIX);
                    AvailableVisaIds = GenerateAvailableInterfaceList(Consts.SAMPLE_VISA_PREFIX, InfrastructureConstants.MinVisaIDRange,
                        InfrastructureConstants.MaxVisaIDCount, allIds.Item1, VisaInterfaceId);
                    AvailableSiclIds = GenerateAvailableInterfaceList(Consts.SAMPLE_SICL_PREFIX, InfrastructureConstants.MinSiclIDRange, InfrastructureConstants.MaxSiclIDCount, allIds.Item2, SiclInterfaceId);
                }, 
                msg => _errorList.Add(msg));

            Api.GetAllUsedLusAsync(arg =>
                {
                    if (arg == null) return;
                    var rawLuList = arg.GetArgList(Ace2ApiConstants.LuAssignments_Parameter);
                    if (rawLuList == null) return;
                    AvailableLus = GenerateAvailableLuList(InfrastructureConstants.MinLURange, InfrastructureConstants.MaxLUCount, rawLuList, LogicalUnit);
                },
                msg => _errorList.Add(msg));

            base.LocalInit();
        }

        public override bool CanDoDelete()
        {
            bool canDelete = true;
            if (IsEdit() && DeviceName.EndsWith("SAMP0"))
            {
                canDelete = false;
            }
            return canDelete;
        }

        public override void DoDelete()
        {
            var persistentID = GetPropertyDataString(AgentKeys.PersistentId);
            GlobalCommands.DeleteInterfaceCommand.Execute(persistentID);
            base.DoDelete();
        }

        #endregion

        #region Private Methods
        private static Tuple<List<string>, List<string>> ExtractUsedInterfaceIdsFromArgMap(ArgMap data, string visaTypeFilter)
        {
            List<string> visas = new List<string>();
            List<string> sicls = new List<string>();
            var parsed = new Tuple<List<string>, List<string>>(visas, sicls);
            if (data == null) return parsed;
            ArgList v = data.GetArgList(Ace2ApiConstants.VisaInterfaceIdAssignments_Parameter);
            ArgList s = data.GetArgList(Ace2ApiConstants.SiclInterfaceIdAssignments_Parameter);
            if (v == null || s == null) return parsed;

            for (int i = 0; i < v.Count; i++)
            {
                if (v[i].ToString().StartsWith(visaTypeFilter, StringComparison.InvariantCultureIgnoreCase))
                {
                    visas.Add(v[i].ToString());
                    sicls.Add(s[i].ToString());     // The SICL interface list MUST be 1-for-1 to the VISA interfaces
                }
            }
            return parsed;
        }
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
