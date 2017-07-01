using System;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;


namespace DemoApplication
{
    /// <summary>
    /// This demonstration for the ADCDAC Pi shows how to read from the ADC and write to the DAC
    /// </summary>
    public sealed partial class ADCDACPi : Page
    {
        // create an instance of the ADCDAC class
        ABElectronics_Win10IOT_Libraries.ADCDACPi adcdac = new ABElectronics_Win10IOT_Libraries.ADCDACPi();       

        // variables for storing the ADC values
        double ADC1_value = 0;
        double ADC2_value = 0;

        // A timer for reading from the ADC
        Timer _timer;

        // set a time interval for reading from the ADC
        int TIME_INTERVAL_IN_MILLISECONDS = 50;

        public ADCDACPi()
        {
            this.InitializeComponent();
        }

        
        private void bt_Connect_Click(object sender, RoutedEventArgs e)
        {
            // when the connect button is clicked set the ADC reference voltage, create an event handler for the Connected event and connect to the ADCDAC Pi.
            adcdac.SetADCrefVoltage(3.3);
            adcdac.Connected += Adcdac_Connected;
            adcdac.Connect();
        }

        private void Adcdac_Connected(object sender, EventArgs e)
        {
            // The ADCDAC Pi is connected to start the timer to read from the ADC channels
            _timer = new Timer(ReadADC, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        private async void ReadADC(object state)
        {
            // Get the values from both ADC channels and store them in two variables.
            ADC1_value = adcdac.ReadADCVoltage(1);
            ADC2_value = adcdac.ReadADCVoltage(2);

            // use a dispatcher event to update the textboxes on the page with the saved values
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {                
                txt_ADC1.Text = ADC1_value.ToString("F3");
                txt_ADC2.Text = ADC2_value.ToString("F3");
            });

            // reset the timer so it will trigger again after the set period
            _timer = new Timer(ReadADC, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        

        private void DAC1SliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // get the new value from slider 1 and use it to update the DAC channel 1
            double dac_value = slider_Channel1.Value;
            adcdac.SetDACVoltage(1, dac_value);
        }

        private void DAC2SliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // get the new value from slider 2 and use it to update the DAC channel 2
            double dac_value = slider_Channel2.Value;
            adcdac.SetDACVoltage(2, dac_value);
        }

        private void bt_Back_Clicked(object sender, RoutedEventArgs e)
        {
            // go back to the main page
            adcdac.Dispose();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }


    }
}
