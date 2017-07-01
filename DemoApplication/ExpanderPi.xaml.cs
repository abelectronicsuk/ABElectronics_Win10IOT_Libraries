using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using ABElectronics_Win10IOT_Libraries;


namespace DemoApplication
{
    /// <summary>
    /// This demonstration shows how to use the Expander Pi class to write to and read from the analogue and digital inputs
    /// </summary>
    public sealed partial class ExpanderPi : Page
    {

        public ABElectronics_Win10IOT_Libraries.ExpanderPi expi = new ABElectronics_Win10IOT_Libraries.ExpanderPi();

        // create two timers.  _timer 1 reads from the IO and ADC.  _timer2 updates the date from the RTC at 1 second intervals
        int TIMER1_INTERVAL_IN_MILLISECONDS = 200;
        int TIMER2_INTERVAL_IN_MILLISECONDS = 1000;
        Timer _timer1;
        Timer _timer2;

        // used to set the expander pi direction.  true = read, false = write.
        bool IO_Direction = true;

        public ExpanderPi()
        {
            this.InitializeComponent();
            // initialise the timers with the preset period
            _timer1 = new Timer(Timer1_Tick, null, TIMER1_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            _timer2 = new Timer(Timer2_Tick, null, TIMER2_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);

        }

        private async void bt_expi_Connect_Click(object sender, RoutedEventArgs e)
        {
            // when the connect button is clicked check that the RTC Pi is not already connected before creating a Connected event handler and connecting to the RTC Pi
            if (!expi.IsConnected)
            {
                expi.Connected += expi_Connected;
                await expi.Connect();
            }           
        }

        private void bt_Back_Clicked(object sender, RoutedEventArgs e)
        {
            // dispose of the expander pi and go back to the main page
            expi.Dispose();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }


        private void expi_Connected(object sender, EventArgs e)
        {
            // expander pi 1 is connected so update the message box, read from the expander pi and start the timer
            WriteMessage("expander pi Connected");

            // set the ADC reference voltage to 4.096V
            expi.ADCSetRefVoltage(4.096);

            radio_IO_Read.IsChecked = true;
            radio_DACGain1.IsChecked = true;

            RefeshDisplay();
            _timer1.Change(TIMER1_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            _timer2.Change(TIMER2_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        private void Timer1_Tick(Object state)
        {
            // on timer tick check if the expander pi is connected and read from the expander pi before resetting the timer
            if (expi.IsConnected)
            {
                RefeshDisplay();
                _timer1.Change(TIMER1_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);                
            }
        }

        private async void Timer2_Tick(Object state)
        {
            // on timer 2 tick check if the expander pi is connected and read from the expander pi before resetting the timer
            if (expi.IsConnected)
            {
                try
                {
                    // read the current date and time from the RTC Pi into a DateTime object
                    DateTime date = expi.RTCReadDate();

                    // invoke a dispatcher to update the date textbox on the page
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        txt_ClockOut.Text = date.ToString("d MMMM yyyy hh:mm:ss tt");
                    });
                }
                catch
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        txt_ClockOut.Text = "Error reading date";
                    });
                }
                _timer2.Change(TIMER2_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            }
        }

        private bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        private async void WriteMessage(string message)
        {
            // this method updates the Message textbox on the page
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txt_Message.Text = message;
            });
        }

        private async void RefeshDisplay()
        {
            // check that the expander pi is connected
            if (expi.IsConnected)
            {
                try
                {
                    // invoke the dispatcher to update the checkboxes for expander pi IO ports with the values read from each pin
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        // check if the IO Pi is in read mode

                        if (radio_IO_Read.IsChecked == true)
                        {

                            // read the values from ports 0 and 1 into two byte variables and check 
                            // the status of each variable to find the pin status
                            // this is faster than reading each pin individually
                            byte port0 = expi.IOReadPort(0);
                            byte port1 = expi.IOReadPort(1);

                            chk_IO_Channel_01.IsChecked = GetBit(port0, 0);
                            chk_IO_Channel_02.IsChecked = GetBit(port0, 1);
                            chk_IO_Channel_03.IsChecked = GetBit(port0, 2);
                            chk_IO_Channel_04.IsChecked = GetBit(port0, 3);
                            chk_IO_Channel_05.IsChecked = GetBit(port0, 4);
                            chk_IO_Channel_06.IsChecked = GetBit(port0, 5);
                            chk_IO_Channel_07.IsChecked = GetBit(port0, 6);
                            chk_IO_Channel_08.IsChecked = GetBit(port0, 7);
                            chk_IO_Channel_09.IsChecked = GetBit(port1, 0);
                            chk_IO_Channel_10.IsChecked = GetBit(port1, 1);
                            chk_IO_Channel_11.IsChecked = GetBit(port1, 2);
                            chk_IO_Channel_12.IsChecked = GetBit(port1, 3);
                            chk_IO_Channel_13.IsChecked = GetBit(port1, 4);
                            chk_IO_Channel_14.IsChecked = GetBit(port1, 5);
                            chk_IO_Channel_15.IsChecked = GetBit(port1, 6);
                            chk_IO_Channel_16.IsChecked = GetBit(port1, 7);

                        }

                        // read the adc values and update the textblocks
                        txt_ADC1.Text = expi.ADCReadVoltage(1, 0).ToString("#.###");
                        txt_ADC2.Text = expi.ADCReadVoltage(2, 0).ToString("#.###");
                        txt_ADC3.Text = expi.ADCReadVoltage(3, 0).ToString("#.###");
                        txt_ADC4.Text = expi.ADCReadVoltage(4, 0).ToString("#.###");
                        txt_ADC5.Text = expi.ADCReadVoltage(5, 0).ToString("#.###");
                        txt_ADC6.Text = expi.ADCReadVoltage(6, 0).ToString("#.###");
                        txt_ADC7.Text = expi.ADCReadVoltage(7, 0).ToString("#.###");
                        txt_ADC8.Text = expi.ADCReadVoltage(8, 0).ToString("#.###");


                    }
                    );

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private async void UpdateClock()
        {
            // Updates the clock label from the RTC clock value

            // check that the expander pi is connected
            if (expi.IsConnected)
            {
                try
                {
                    // invoke the dispatcher to update the checkboxes for expander pi IO ports with the values read from each pin
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {

                    }
                    );

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        private void IO_SetDirection(object sender, RoutedEventArgs e)
        {
            // read the values from the direction radio buttons and update the expander pi 2 ports using the SetPortDirection method
            if (radio_IO_Read.IsChecked == true)
            {
                if (expi.IsConnected)
                {
                    expi.IOSetPortDirection(0, 0xFF);
                    expi.IOSetPortDirection(1, 0xFF);
                    IO_Direction = true;

                    WriteMessage("IO Reading");
                }
                else
                {
                    radio_IO_Read.IsChecked = false;
                    WriteMessage("expander pi not connected");
                }
            }
            if (radio_IO_Write.IsChecked == true)
            {
                if (expi.IsConnected)
                {
                    expi.IOSetPortDirection(0, 0x00);
                    expi.IOSetPortDirection(1, 0x00);
                    IO_Direction = false;

                    WriteMessage("IO Writing");
                }
                else
                {
                    radio_IO_Write.IsChecked = false;
                    WriteMessage("expander pi not connected");
                }
            }
        }


        private void IO_EnablePullups(object sender, RoutedEventArgs e)
        {
            // get the value from the pull-ups checkbox and set the port pull-ups to the required state using the SetPortPullups method
            CheckBox cb = (CheckBox)sender;
            if (expi.IsConnected)
            {
                if (cb.IsChecked == true)
                {
                    expi.IOSetPortPullups(0, 0xFF);
                    expi.IOSetPortPullups(1, 0xFF);
                }
                else
                {
                    expi.IOSetPortPullups(0, 0x00);
                    expi.IOSetPortPullups(1, 0x00);
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("expander pi not connected");
            }
        }

        private void IO_InvertPort(object sender, RoutedEventArgs e)
        {
            // get the value from the invert port checkbox and set the port to the required state using the InvertPort method
            CheckBox cb = (CheckBox)sender;
            if (expi.IsConnected)
            {
                if (cb.IsChecked == true)
                {
                    expi.IOInvertPort(0, 0xFF);
                    expi.IOInvertPort(1, 0xFF);

                    WriteMessage("IO Inverted");
                }
                else
                {
                    expi.IOInvertPort(0, 0x00);
                    expi.IOInvertPort(1, 0x00);

                    WriteMessage("IO not inverted");
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("expander pi not connected");
            }
        }

        private void IO_EnablePort0(object sender, RoutedEventArgs e)
        {
            // get the value from the enable port checkbox and set the port values to the required state using the WritePort method
            CheckBox cb = (CheckBox)sender;
            if (expi.IsConnected == true)
            {
                if (IO_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        expi.IOWritePort(0, 0xFF);
                    }
                    else
                    {
                        expi.IOWritePort(0, 0x00);
                    }
                }
                else
                {
                    cb.IsChecked = false;
                    WriteMessage("You can not set a port state in read mode.");
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("expander pi not connected.");
            }
        }

        private void IO_EnablePort1(object sender, RoutedEventArgs e)
        {
            // get the value from the enable port checkbox and set the port values to the required state using the WritePort method
            CheckBox cb = (CheckBox)sender;
            if (expi.IsConnected == true)
            {
                if (IO_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        expi.IOWritePort(1, 0xFF);
                    }
                    else
                    {
                        expi.IOWritePort(1, 0x00);
                    }
                }
                else
                {
                    cb.IsChecked = false;
                    WriteMessage("You can not set a port state in read mode.");
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("expander pi not connected.");
            }
        }

        private void IO_PinEnable(object sender, RoutedEventArgs e)
        {
            // check which pin checkbox was clicked and update the value of that pin to the required state
            CheckBox cb = (CheckBox)sender;
            if (expi.IsConnected == true)
            {
                if (IO_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        expi.IOWritePin(Convert.ToByte(cb.Content.ToString()), true);
                    }
                    else
                    {
                        expi.IOWritePin(Convert.ToByte(cb.Content.ToString()), false);
                    }
                }
                else
                {
                    cb.IsChecked = false;
                    WriteMessage("You can not set a pin state in read mode.");
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("expander pi not connected.");
            }
        }


        

        

        private void updateDAC(byte channel)
        {
            if (expi.IsConnected)
            {
                double newvoltage = 0;

                if (channel == 1)
                {
                    newvoltage = slider_DACChannel1.Value;
                }
                else
                {
                    newvoltage = slider_DACChannel2.Value;
                }                

                // check to see if the gain is set to 1
                if (radio_DACGain1.IsChecked == true)
                {
                    // write the value to the dac channel 1
                    expi.DACSetVoltage(channel, newvoltage, 1);
                }
                else // gain is set to 2
                {
                    // double the value in newvoltage before writing it to the DAC
                    newvoltage = newvoltage * 2;
                    expi.DACSetVoltage(channel, newvoltage, 2);
                }
            }
            else
            {
                WriteMessage("expander pi not connected.");
            }
        }

        private void DACChannel1_Changed(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            updateDAC(1);
        }

        private void DACChannel2_Changed(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            updateDAC(2);
        }

        private void radio_DACGain1_Checked(object sender, RoutedEventArgs e)
        {
            updateDAC(1);
            updateDAC(2);
        }

        private void radio_DACGain2_Checked(object sender, RoutedEventArgs e)
        {
            updateDAC(1);
            updateDAC(2);
        }


        private void bt_SetDate_Click(object sender, RoutedEventArgs e)
        {
            if (expi.IsConnected)
            {
                // create a new DateTime object using the values from the Date and Time pickers
                DateTime newdate = new DateTime(picker_NewDate.Date.Year, picker_NewDate.Date.Month, picker_NewDate.Date.Day, picker_NewTime.Time.Hours, picker_NewTime.Time.Minutes, picker_NewTime.Time.Seconds);

                // update the RTC Pi with the new DateTime object
                expi.RTCSetDate(newdate);
            }
        }

        private void cb_sqw_clicked(object sender, RoutedEventArgs e)
        {
            // check the value for the SQW checkbox and enable or disable the square wave output pin
            if (expi.IsConnected)
            {
                if (cb_sqw.IsChecked == true)
                {
                    radio_frequency_clicked(null, null);
                    expi.RTCEnableOutput();
                }
                else
                {
                    expi.RTCDisableOutput();
                }

            }
        }

        private void radio_frequency_clicked(object sender, RoutedEventArgs e)
        {
            // check which frequency radio button has been clicked and update the frequency for the square wave output
            if (expi.IsConnected)
            {
                if (radio_frequency1.IsChecked == true)
                {
                    expi.RTCSetFrequency(1);
                }
                if (radio_frequency2.IsChecked == true)
                {
                    expi.RTCSetFrequency(2);
                }
                if (radio_frequency3.IsChecked == true)
                {
                    expi.RTCSetFrequency(3);
                }
                if (radio_frequency4.IsChecked == true)
                {
                    expi.RTCSetFrequency(4);
                }

            }
        }
    }
}
