using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;

namespace DemoApplication
{
    /// <summary>
    /// This demonstration shows how to read from the ADC Pi and update the bit rate and gain.
    /// </summary>
    public sealed partial class ADCPi : Page
    {
        // create an instance of the ADCPi class called adc
        ABElectronics_Win10IOT_Libraries.ADCPi adc = new ABElectronics_Win10IOT_Libraries.ADCPi();

        // set the time interval for sampling from the ADC and create a timer
        int TIME_INTERVAL_IN_MILLISECONDS = 10;
        Timer _timer;

        // this will be used to measure the sample rate from the ADC Pi
        DateTime startTime;

        // used to start and stop the sampling
        bool run = false;

        // temporary variables where the ADC values will be stored
        double channel1_value = 0;
        double channel2_value = 0;
        double channel3_value = 0;
        double channel4_value = 0;
        double channel5_value = 0;
        double channel6_value = 0;
        double channel7_value = 0;
        double channel8_value = 0;

        public ADCPi()
        {
            this.InitializeComponent();
        }

        private async void bt_Connect_Click(object sender, RoutedEventArgs e)
        {
            // when the connect button is clicked update the ADC i2c addresses with the values in the textboxes on the page
            try
            {
                adc.Address1 = Convert.ToByte(txt_Address1.Text.Replace("0x", ""), 16);
                adc.Address2 = Convert.ToByte(txt_Address2.Text.Replace("0x", ""), 16);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create a Connected event handler and connect to the ADC Pi.
            adc.Connected += Adc_Connected;
            await adc.Connect();
        }

        private void Adc_Connected(object sender, EventArgs e)
        {
            // The ADC Pi is connected

            // set the initial bit rate to 16
            adc.SetBitRate(16);
            radio_bitrate12.IsChecked = false;
            radio_bitrate14.IsChecked = false;
            radio_bitrate16.IsChecked = true;
            radio_bitrate18.IsChecked = false;

            // set the gain to 1
            adc.SetPGA(1);
            radio_Gain1.IsChecked = true;
            radio_Gain2.IsChecked = false;
            radio_Gain4.IsChecked = false;
            radio_Gain8.IsChecked = false;

            // set the startTime to be now and start the timer
            startTime = DateTime.Now;
            _timer = new Timer(ReadADC, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);

        }

        private async void ReadADC(object state)
        {
            // check to see if the run is true
            if (run)
            {
                // get the voltage values from all 8 ADC channels
                channel1_value = adc.ReadVoltage(1);
                channel2_value = adc.ReadVoltage(2);
                channel3_value = adc.ReadVoltage(3);
                channel4_value = adc.ReadVoltage(4);
                channel5_value = adc.ReadVoltage(5);
                channel6_value = adc.ReadVoltage(6);
                channel7_value = adc.ReadVoltage(7);
                channel8_value = adc.ReadVoltage(8);

                // use a dispatcher event to update the textboxes on the page
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    txt_Channel1.Text = channel1_value.ToString("F4");
                    txt_Channel2.Text = channel2_value.ToString("F4");
                    txt_Channel3.Text = channel3_value.ToString("F4");
                    txt_Channel4.Text = channel4_value.ToString("F4");
                    txt_Channel5.Text = channel5_value.ToString("F4");
                    txt_Channel6.Text = channel6_value.ToString("F4");
                    txt_Channel7.Text = channel7_value.ToString("F4");
                    txt_Channel8.Text = channel8_value.ToString("F4");
                });

                // calculate how long it has been since the last reading and use that to work out the sample rate
                TimeSpan duration = DateTime.Now.Subtract(startTime);
                startTime = DateTime.Now;
                WriteMessage((1000 / (duration.TotalMilliseconds / 8)).ToString("F2") + "sps");

                // reset the timer so it will run again after the preset period
                _timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            }
        }

        private void radio_bitrate_clicked(object sender, RoutedEventArgs e)
        {
            // find out which radio button was clicked and use that to update the bit rate for the ADC
            RadioButton cb = (RadioButton)sender;
            if (adc.IsConnected == true)
            {
                adc.SetBitRate(Convert.ToByte(cb.Content.ToString()));
            }
            else
            {
                WriteMessage("ADC not connected.");
            }
        }

        private void radio_gain_clicked(object sender, RoutedEventArgs e)
        {
            // find out which radio button was clicked and use that to update the gain for the ADC
            RadioButton cb = (RadioButton)sender;
            if (adc.IsConnected == true)
            {
                adc.SetPGA(Convert.ToByte(cb.Content.ToString()));
            }
            else
            {
                WriteMessage("ADC not connected.");
            }
        }

        private void bt_Back_Clicked(object sender, RoutedEventArgs e)
        {
            // go back to the main page
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        
        private async void WriteMessage(string message)
        {
            // WriteMessage is used to update the message box on the page
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txt_Message.Text = message;
            });
        }

        private void bt_Start_Click(object sender, RoutedEventArgs e)
        {
            // set run to be true and call ReadADC to start the ADC reading
            run = true;
            ReadADC(null);
        }

        private void bt_Stop_Click(object sender, RoutedEventArgs e)
        {
            // set run to false to stop the ADC from reading
            run = false;
        }
    }
}
