using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using ACE2_UI.Interface_Definitions.UiAgent;
using ACE2_UI.ModulesUiAgents;
using ACE2_UI.Infrastructure;

namespace Keysight.KCE.UISamples
{
    [Module(ModuleName = "UiAgentSampleInit")]
    public class UiAgentSampleInit : IUiAgent, IProvideManualEntryViews
    {
        IUnityContainer _container;
        IRegionManager _rm;
        SampleAddEditInterfaceView _addInterfaceView;
        SampleAddEditInterfaceView _editInterfaceView;
        SampleAddEditInstrumentView _addInstrumentView;
        SampleAddEditInstrumentView _editInstrumentView;

        public UiAgentSampleInit(IUnityContainer container, IRegionManager rm)
        {
            _container = container;
            _rm = rm;
        }

        #region IUiAgent Members

        public string AgentId
        {
            get { return Consts.SAMPLE_IOAGENT_ID; }
        }

        public void Initialize()
        {
            _container.RegisterInstance<IUiAgent>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterInstance<IProvideManualEntryViews>(AgentId, this, new ContainerControlledLifetimeManager());
            _container.RegisterType<object, LanAddEditInstrumentView>(new PerResolveLifetimeManager());

            _addInterfaceView = _container.Resolve<SampleAddEditInterfaceView>();
            _editInterfaceView = _container.Resolve<SampleAddEditInterfaceView>();
            _addInstrumentView = _container.Resolve<SampleAddEditInstrumentView>();
            _editInstrumentView = _container.Resolve<SampleAddEditInstrumentView>();

            if (_rm != null)
            {
                _rm.AddToRegion(InfrastructureConstants.EditDeviceConfigurationRegion, _editInterfaceView);
                _rm.AddToRegion(InfrastructureConstants.AddDeviceConfigurationRegion, _addInterfaceView);
                _rm.AddToRegion(InfrastructureConstants.EditDeviceConfigurationRegion, _editInstrumentView);
                _rm.AddToRegion(InfrastructureConstants.AddDeviceConfigurationRegion, _addInstrumentView);
            }
        }

        #endregion

        #region IProvideManualEntryViews Members

        public ContentControl GetManualAddView(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (name.Equals(Consts.SAMPLE_INSTRUMENT, StringComparison.InvariantCultureIgnoreCase)) return _addInstrumentView;
            else if (name.Equals(Consts.SAMPLE_INTERFACE, StringComparison.InvariantCultureIgnoreCase)) return _addInterfaceView;

            return null;    // catchall
        }

        public ContentControl GetManualEditView(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (name.StartsWith(Consts.SAMPLE_INSTRUMENT, StringComparison.InvariantCultureIgnoreCase)) return _editInstrumentView;
            else if (name.StartsWith(Consts.SAMPLE_INTERFACE, StringComparison.InvariantCultureIgnoreCase)) return _editInterfaceView;

            return null;
        }

        #endregion
    }
}
