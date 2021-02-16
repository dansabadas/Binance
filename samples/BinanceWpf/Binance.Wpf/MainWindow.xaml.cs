using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace Binance.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly BinanceApi binanceApi;
        readonly Timer _timer;
        private SynchronizationContext _uiContext = SynchronizationContext.Current;
        public MainWindow()
        {
            InitializeComponent();
            binanceApi = new BinanceApi();

            _timer = new Timer(2000) { Enabled = false, AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
        }


        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string symbol = null;
            Dispatcher.Invoke(() => symbol = FilteredComboBox1.Text);

            SymbolPrice price = null;
            try
            {
                price = await binanceApi.GetPriceAsync(symbol);
            }
            catch (ArgumentException)
            {
                Dispatcher.Invoke(() => PriceLabel.Content = $"Wrong Ticker code: {symbol}");
                return;
            }
            catch(BinanceHttpException ex) {
                if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Dispatcher.Invoke(() => PriceLabel.Content = $"Wrong Ticker code: {symbol}");
                    return;
                }
            }
            Dispatcher.Invoke(() => PriceLabel.Content = $"{symbol}: {price.Value}");
        }

        IEnumerable<SymbolPrice> _allPrices;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_allPrices == null)
            {
                _allPrices = await binanceApi.GetPricesAsync();

                FilteredComboBox1.IsEditable = true;
                FilteredComboBox1.IsTextSearchEnabled = false;
                FilteredComboBox1.ItemsSource = _allPrices.Select(price => price.Symbol).ToList();
                FilteredComboBox1.SelectedValue = "ETHBTC";
            }

            if ((string)PriceButton.Content == "Get Price")
            {
                _timer.Enabled = true;
                PriceButton.Content = "Stop Binance queries";
            }
            else
            {
                PriceButton.Content = "Get Price";
                _timer.Enabled = false;
            }
            // https://www.wpftutorial.net/GridLayout.html https://www.tutorialspoint.com/wpf/wpf_layouts.htm
        }
    }
}
