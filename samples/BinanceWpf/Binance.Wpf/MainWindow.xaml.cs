using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Binance.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly BinanceApi binanceApi;
        readonly System.Timers.Timer _timer;
        private SynchronizationContext _uiContext = SynchronizationContext.Current;
        public MainWindow()
        {
            InitializeComponent();
            binanceApi = new BinanceApi();

            this.Loaded += MainWindow_Loaded;
            _timer = new System.Timers.Timer(2000) { Enabled = false };
            _timer.Elapsed += _timer_Elapsed;
        }


        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            string symbol = null;
            Dispatcher.Invoke(new Action(() => symbol = SymbolTextBox.Text));

            var price = await binanceApi.GetPriceAsync(symbol);

            Dispatcher.Invoke(new Action(() => PriceLabel.Content = price.Value.ToString()));
            _timer.Enabled = true;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var allPrices = await binanceApi.GetPricesAsync();
            DataGrid1.ItemsSource = allPrices;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _timer.Enabled = true;
            // get price
            var price = await binanceApi.GetPriceAsync(this.SymbolTextBox.Text);
            Dispatcher.Invoke(() =>
            {
                PriceLabel.Content = price.Value.ToString();
            });
        }
    }
}
