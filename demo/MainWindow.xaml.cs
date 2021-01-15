using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GroupedObservableCollection.Demo.ViewModels;

namespace GroupedObservableCollection.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _model;
        public MainWindow()
        {
            InitializeComponent();
            _model = (MainWindowViewModel)Resources["Model"];
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _model.BeginLoadingSampleData();
        }
    }
}
