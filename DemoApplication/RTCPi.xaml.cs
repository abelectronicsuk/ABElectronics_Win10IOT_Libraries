using System;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;



namespace DemoApplication
{
    /// <summary>
    /// This demonstration shows how to use the RTC Pi class to read and set a date and time
    /// </summary>
    public sealed partial class RTCPi : Page
    {
        // create an instance of the RTCPi class called rtc
        ABElectronics_Win10IOT_Libraries.RTCPi rtc = new ABElectronics_Win10IOT_Libraries.RTCPi();

        // a timer will be used to read from the RTC Pi at 1 second intervals
        Timer _timer;

        public RTCPi()
        {
            this.InitializeComponent();

        }

        private async void bt_Connect_Click(object sender, RoutedEventArgs e)
        {
            // when the connect button is clicked check that the RTC Pi is not already connected before creating a Connected event handler and connecting to the RTC Pi
            if (!rtc.IsConnected)
            {
                rtc.Connected += Rtc_Connected;
                await rtc.Connect();
            }
        }

        private void Rtc_Connected(object sender, EventArgs e)
        {
            // a connection has been established so start the timer to read the date from the RTC Pi
            _timer = new Timer(Timer_Tick, null, 1000, Timeout.Infinite);
        }

        private async void Timer_Tick(object state)
        {
            // check that the RTC Pi is still connected
            if (rtc.IsConnected)
            {
                try {
                    // read the current date and time from the RTC Pi into a DateTime object
                    DateTime date = rtc.ReadDate();

                    // invoke a dispatcher to update the date textbox on the page
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        txt_Date.Text = date.ToString("d MMMM yyyy hh:mm:ss tt");
                    });
                }
                catch
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        txt_Date.Text = "Error reading date";
                    });
                }
            }
            _timer.Change(1000, Timeout.Infinite);
        }

        private void bt_SetDate_Click(object sender, RoutedEventArgs e)
        {
            if (rtc.IsConnected)
            {
                // create a new DateTime object using the values from the Date and Time pickers
                DateTime newdate = new DateTime(picker_NewDate.Date.Year, picker_NewDate.Date.Month, picker_NewDate.Date.Day, picker_NewTime.Time.Hours, picker_NewTime.Time.Minutes, picker_NewTime.Time.Seconds);

                // update the RTC Pi with the new DateTime object
                rtc.SetDate(newdate);
            }
        }

        private void cb_sqw_clicked(object sender, RoutedEventArgs e)
        {
            // check the value for the SQW checkbox and enable or disable the square wave output pin
            if (rtc.IsConnected)
            {
                if (cb_sqw.IsChecked == true)
                {
                    radio_frequency_clicked(null, null);
                    rtc.EnableOutput();
                }
                else
                {
                    rtc.DisableOutput();
                }

            }
        }

        private void radio_frequency_clicked(object sender, RoutedEventArgs e)
        {
            // check which frequency radio button has been clicked and update the frequency for the square wave output
            if (rtc.IsConnected)
            {
                if (radio_frequency1.IsChecked == true)
                {
                    rtc.SetFrequency(1);
                }
                if (radio_frequency2.IsChecked == true)
                {
                    rtc.SetFrequency(2);
                }
                if (radio_frequency3.IsChecked == true)
                {
                    rtc.SetFrequency(3);
                }
                if (radio_frequency4.IsChecked == true)
                {
                    rtc.SetFrequency(4);
                }

            }
        }


        private void bt_Back_Clicked(object sender, RoutedEventArgs e)
        {
            // dispose of the rtc object and go back to the main page
            try
            {
                rtc.Dispose();
            }
            catch { }
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        
    }
}
