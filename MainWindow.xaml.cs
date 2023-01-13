using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
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

namespace roddb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new(webView2, Dispatcher);
            DataContext = _viewModel;
            _viewModel.MainWindow = this;
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await webView2.EnsureCoreWebView2Async();
            webView2.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged; ;
        }

        private void CoreWebView2_HistoryChanged(object? sender, object e)
        {
            _viewModel.CanGoForward = webView2.CanGoForward;
            _viewModel.CanGoBack = webView2.CanGoBack;
        }
    }
}
