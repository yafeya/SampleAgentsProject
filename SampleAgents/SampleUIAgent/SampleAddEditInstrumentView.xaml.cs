using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ACE2_UI.Model;
using ACE2_UI.Services;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Unity;

namespace Keysight.KCE.UISamples
{
    /// <summary>
    /// Interaction logic for SampleAddEditInstrumentView.xaml
    /// </summary>
    public partial class SampleAddEditInstrumentView : UserControl
    {
        private SampleAddEditInstrumentViewModel _vm = null;

        public SampleAddEditInstrumentView(IUnityContainer container, Ace2ApiServiceModule api, ILoggerFacade log, 
            UiModel model, IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _vm = new SampleAddEditInstrumentViewModel(container, api, log, model, eventAggregator);
            this.DataContext = _vm;
        }
    }
}
