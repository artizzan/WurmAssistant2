using System;
using System.Windows;
using System.Windows.Controls;

namespace Aldurcraft.WurmAssistantLauncher2.Views
{
    /// <summary>
    /// Interaction logic for WaRoadmapView.xaml
    /// </summary>
    public partial class WaBrowserView : UserControl
    {
        private Uri source;
        public WaBrowserView()
        {
            InitializeComponent();
        }

        public string NavigateUrl
        {
            set
            {
                source = new Uri(value); 
            }
        }

        private void WaBrowserView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WebCtrl.Source = this.IsVisible ? source : null;
        }
    }
}
