using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.ViewModels;

namespace Aldurcraft.WurmAssistantLauncher2.Views
{
    /// <summary>
    /// Interaction logic for ManageWaBackups.xaml
    /// </summary>
    public partial class ManageWaBackups : Window
    {
        public ManageWaBackups(WaBackupsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid) sender;
            var firstCol = grid.Columns.First();
            firstCol.SortDirection = ListSortDirection.Descending;
            grid.Items.SortDescriptions.Add(new SortDescription(firstCol.SortMemberPath, ListSortDirection.Descending));
        }
    }
}
