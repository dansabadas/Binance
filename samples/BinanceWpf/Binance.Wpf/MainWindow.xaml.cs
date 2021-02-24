using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
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
        private decimal _targetPrice;
        private decimal _currentPrice;
        private bool _isPriceSearchAscending;

        private MediaPlayer _mediaPlayerAscending = new MediaPlayer();
        private MediaPlayer _mediaPlayerDescending = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            binanceApi = new BinanceApi();
            _targetPrice = -1;

            _timer = new Timer(2000) { Enabled = false, AutoReset = true };
            _timer.Elapsed += _timer_Elapsed;
            _mediaPlayerAscending.Open(new Uri("08 - The End Complete.mp3", UriKind.Relative));
            _mediaPlayerDescending.Open(new Uri("08 - The End Complete.mp3", UriKind.Relative));//"01 - Internal Bleeding.mp3"
        }


        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string symbol = null;
            Dispatcher.Invoke(() => symbol = AllPricesComboBox.Text);

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
            Dispatcher.Invoke(() => TimeLabel.Content = $"{DateTime.Now.ToString("F", CultureInfo.InvariantCulture)}");

            _currentPrice = price.Value;
            if (_isPriceSearchAscending)
            {
                if (_targetPrice <= _currentPrice)
                {
                    await PlayAlarm(_mediaPlayerAscending);
                }
            }
            else
            {
                if (_targetPrice >= _currentPrice)
                {
                    await PlayAlarm(_mediaPlayerDescending);
                }
            }

            if (_targetPrice > 0)
            {
                _isPriceSearchAscending = _currentPrice < _targetPrice;
            }
        }

        private async Task PlayAlarm(MediaPlayer mediaPlayer)
        {
            var musicPlayTask = Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() => mediaPlayer.Play());
            });
            await Task.Delay(15000).ContinueWith((t) => Dispatcher.Invoke(() => mediaPlayer.Stop()));
        }

        IEnumerable<SymbolPrice> _allPrices;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_allPrices == null)
            {
                _allPrices = await binanceApi.GetPricesAsync();

                AllPricesComboBox.IsEditable = true;
                AllPricesComboBox.IsTextSearchEnabled = false;
                AllPricesComboBox.ItemsSource = _allPrices.Select(price => price.Symbol).ToList();
                AllPricesComboBox.SelectedValue = "ETHBTC";
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

        private void AlertPriceTextBlock_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(!decimal.TryParse(AlertPriceTextBlock.Text, out _targetPrice))
            {
                _targetPrice = -1;
            }
        }
    }
}
