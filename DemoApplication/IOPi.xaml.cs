using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;


namespace DemoApplication
{
    /// <summary>
    /// This demonstration shows how to use the IO Pi class to write to and read from and IO Pi or IO Pi Plus
    /// </summary>
    public sealed partial class IOPi : Page
    {
        // The IO Pi contains two MCP23017 chips so we need to create a separate instance of the IOPi class for each chip and call the bus1 and bus2
        public ABElectronics_Win10IOT_Libraries.IOPi bus1 = new ABElectronics_Win10IOT_Libraries.IOPi(0x20);
        public ABElectronics_Win10IOT_Libraries.IOPi bus2 = new ABElectronics_Win10IOT_Libraries.IOPi(0x21);

        // create two timers for reading from each IO Pi bus
        int TIME_INTERVAL_IN_MILLISECONDS = 200;
        Timer _timer1;
        Timer _timer2;

        // used to set the bus direction.  true = read, false = write.
        bool Bus1_Direction = true;
        bool Bus2_Direction = true;

        public IOPi()
        {
            this.InitializeComponent();
            // initialise the timers with the preset period
            _timer1 = new Timer(Timer1_Tick, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            _timer2 = new Timer(Timer2_Tick, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);

        }

        private async void bt_Bus1_Connect_Click(object sender, RoutedEventArgs e)
        {
            // check to see if there is an address in the textbox for bus 1 and if so connect to the IO Pi
            if (txt_Bus1_Address.Text.Length > 0)
            {
                try {
                    // get the i2c address from the textbox for bus 1
                    bus1.Address = Convert.ToByte(txt_Bus1_Address.Text.Replace("0x",""), 16);
                    
                    // create an event handler for the Connected event and connect to bus 1
                    bus1.Connected += Bus1_Connected;
                    await bus1.Connect();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        private void Bus1_Connected(object sender, EventArgs e)
        {
            // bus 1 is connected so update the message box, read from the bus and start the timer
            WriteMessage("Bus 1 Connected");
            ReadBus1();
            _timer1.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        private async void bt_Bus2_Connect_Click(object sender, RoutedEventArgs e)
        {
            // check to see if there is an address in the textbox for bus 2 and if so connect to the IO Pi
            if (txt_Bus2_Address.Text.Length > 0)
            {
                try
                {
                    // get the i2c address from the textbox for bus 2
                    bus2.Address = Convert.ToByte(txt_Bus2_Address.Text.Replace("0x", ""), 16);

                    // create an event handler for the Connected event and connect to bus 2
                    bus2.Connected += Bus2_Connected;
                    await bus2.Connect();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void Bus2_Connected(object sender, EventArgs e)
        {
            // bus 2 is connected so update the message box, read from the bus and start the timer
            WriteMessage("Bus 2 Connected");
            ReadBus2();
            _timer2.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        private void Timer1_Tick(Object state)
        {
            // on timer tick check if the bus is connected and read from the bus before resetting the timer
            if (bus1.IsConnected)
            {
                ReadBus1();
                _timer1.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            }
        }

        private void Timer2_Tick(Object state)
        {
            // on timer 2 tick check if the bus is connected and read from the bus before resetting the timer
            if (bus2.IsConnected)
            {
                ReadBus2();
                _timer2.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
            }
        }

        private async void ReadBus1()
        {
            // check that the bus is connected
            if (bus1.IsConnected)
            {
                try
                {
                    // invoke the dispatcher to update the checkboxes for bus 1 with the values read from each pin
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        Bus1_Channel_01_checkBox.IsChecked = bus1.ReadPin(1);
                        Bus1_Channel_02_checkBox.IsChecked = bus1.ReadPin(2);
                        Bus1_Channel_03_checkBox.IsChecked = bus1.ReadPin(3);
                        Bus1_Channel_04_checkBox.IsChecked = bus1.ReadPin(4);
                        Bus1_Channel_05_checkBox.IsChecked = bus1.ReadPin(5);
                        Bus1_Channel_06_checkBox.IsChecked = bus1.ReadPin(6);
                        Bus1_Channel_07_checkBox.IsChecked = bus1.ReadPin(7);
                        Bus1_Channel_08_checkBox.IsChecked = bus1.ReadPin(8);
                        Bus1_Channel_09_checkBox.IsChecked = bus1.ReadPin(9);
                        Bus1_Channel_10_checkBox.IsChecked = bus1.ReadPin(10);
                        Bus1_Channel_11_checkBox.IsChecked = bus1.ReadPin(11);
                        Bus1_Channel_12_checkBox.IsChecked = bus1.ReadPin(12);
                        Bus1_Channel_13_checkBox.IsChecked = bus1.ReadPin(13);
                        Bus1_Channel_14_checkBox.IsChecked = bus1.ReadPin(14);
                        Bus1_Channel_15_checkBox.IsChecked = bus1.ReadPin(15);
                        Bus1_Channel_16_checkBox.IsChecked = bus1.ReadPin(16);
                    }
                    );

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private async void ReadBus2()
        {
            // check that the bus is connected
            if (bus2.IsConnected)
            {
                try
                {
                    // invoke the dispatcher to update the checkboxes for bus 1 with the values read from each pin
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        Bus2_Channel_01_checkBox.IsChecked = bus2.ReadPin(1);
                        Bus2_Channel_02_checkBox.IsChecked = bus2.ReadPin(2);
                        Bus2_Channel_03_checkBox.IsChecked = bus2.ReadPin(3);
                        Bus2_Channel_04_checkBox.IsChecked = bus2.ReadPin(4);
                        Bus2_Channel_05_checkBox.IsChecked = bus2.ReadPin(5);
                        Bus2_Channel_06_checkBox.IsChecked = bus2.ReadPin(6);
                        Bus2_Channel_07_checkBox.IsChecked = bus2.ReadPin(7);
                        Bus2_Channel_08_checkBox.IsChecked = bus2.ReadPin(8);
                        Bus2_Channel_09_checkBox.IsChecked = bus2.ReadPin(9);
                        Bus2_Channel_10_checkBox.IsChecked = bus2.ReadPin(10);
                        Bus2_Channel_11_checkBox.IsChecked = bus2.ReadPin(11);
                        Bus2_Channel_12_checkBox.IsChecked = bus2.ReadPin(12);
                        Bus2_Channel_13_checkBox.IsChecked = bus2.ReadPin(13);
                        Bus2_Channel_14_checkBox.IsChecked = bus2.ReadPin(14);
                        Bus2_Channel_15_checkBox.IsChecked = bus2.ReadPin(15);
                        Bus2_Channel_16_checkBox.IsChecked = bus2.ReadPin(16);
                    }
                    );

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
               

        private void Bus1_SetDirection(object sender, RoutedEventArgs e)
        {
            // read the values from the direction radio buttons and update the bus 1 ports using the SetPortDirection method
            if (radio_Bus1_Read.IsChecked == true)
            {
                if (bus1.IsConnected)
                {
                    bus1.SetPortDirection(0, 0xFF);
                    bus1.SetPortDirection(1, 0xFF);
                    Bus1_Direction = true;
                    
                    WriteMessage("Bus 1 Reading");
                }
                else
                {
                    radio_Bus1_Read.IsChecked = false;
                    WriteMessage("Bus 1 not connected");
                }
            }
            if (radio_Bus1_Write.IsChecked == true)
            {
                if (bus1.IsConnected)
                {
                    bus1.SetPortDirection(0, 0x00);
                    bus1.SetPortDirection(1, 0x00);
                    Bus1_Direction = false;

                    WriteMessage("Bus 1 Writing");
                }
                else
                {
                    radio_Bus1_Write.IsChecked = false;
                    WriteMessage("Bus 1 not connected");
                }
            }

        }

        private void Bus2_SetDirection(object sender, RoutedEventArgs e)
        {
            // read the values from the direction radio buttons and update the bus 2 ports using the SetPortDirection method
            if (radio_Bus2_Read.IsChecked == true)
            {
                if (bus2.IsConnected)
                {
                    bus2.SetPortDirection(0, 0xFF);
                    bus2.SetPortDirection(1, 0xFF);
                    Bus2_Direction = true;                    

                    WriteMessage("Bus 2 Reading");
                }
                else
                {
                    radio_Bus2_Read.IsChecked = false;
                    WriteMessage("Bus 2 not connected");
                }
            }
            if (radio_Bus2_Write.IsChecked == true)
            {
                if (bus2.IsConnected)
                {
                    bus2.SetPortDirection(0, 0x00);
                    bus2.SetPortDirection(1, 0x00);
                    Bus2_Direction = false;

                    WriteMessage("Bus 2 Writing");
                }
                else
                {
                    radio_Bus2_Write.IsChecked = false;
                    WriteMessage("Bus 2 not connected");
                }
            }
        }


        private void Bus1_EnablePullups(object sender, RoutedEventArgs e)
        {
            // get the value from the pull-ups checkbox and set the port pull-ups to the required state using the SetPortPullups method
            CheckBox cb = (CheckBox)sender;
            if (bus1.IsConnected)
            {
                if (cb.IsChecked == true)
                {
                    bus1.SetPortPullups(0, 0xFF);
                    bus1.SetPortPullups(1, 0xFF);
                }
                else
                {
                    bus1.SetPortPullups(0, 0x00);
                    bus1.SetPortPullups(1, 0x00);
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("Bus 1 not connected");
            }
        }

        private void Bus2_EnablePullups(object sender, RoutedEventArgs e)
        {
            // get the value from the pull-ups checkbox and set the port pull-ups to the required state using the SetPortPullups method
            CheckBox cb = (CheckBox)sender;
            if (bus2.IsConnected)
            {
                if (cb.IsChecked == true)
                {
                    bus2.SetPortPullups(0, 0xFF);
                    bus2.SetPortPullups(1, 0xFF);
                }
                else
                {
                    bus2.SetPortPullups(0, 0x00);
                    bus2.SetPortPullups(1, 0x00);
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("Bus 2 not connected");
            }
        }

        

        private void Bus1_InvertPort(object sender, RoutedEventArgs e)
        {
            // get the value from the invert port checkbox and set the port to the required state using the InvertPort method
            CheckBox cb = (CheckBox)sender;
            if (bus1.IsConnected)
            {
                if (cb.IsChecked == true)
                {
                    bus1.InvertPort(0, 0xFF);
                    bus1.InvertPort(1, 0xFF);

                    WriteMessage("Bus 2 Inverted");
                }
                else
                {
                    bus1.InvertPort(0, 0x00);
                    bus1.InvertPort(1, 0x00);

                    WriteMessage("Bus 1 not inverted");
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("Bus 1 not connected");
            }
        }

        private void Bus2_InvertPort(object sender, RoutedEventArgs e)
        {
            // get the value from the invert port checkbox and set the port to the required state using the InvertPort method
            CheckBox cb = (CheckBox)sender;
            if (bus2.IsConnected)
            {
                if (cb.IsChecked == true)
                {
                    bus2.InvertPort(0, 0xFF);
                    bus2.InvertPort(1, 0xFF);

                    WriteMessage("Bus 2 Inverted");
                }
                else
                {
                    bus2.InvertPort(0, 0x00);
                    bus2.InvertPort(1, 0x00);

                    WriteMessage("Bus 2 not inverted");
                }
            }
            else
            {
                cb.IsChecked = false;
                WriteMessage("Bus 2 not connected");
            }
        }
        private void Bus1_EnablePort0(object sender, RoutedEventArgs e)
        {
            // get the value from the enable port checkbox and set the port values to the required state using the WritePort method
            CheckBox cb = (CheckBox)sender;
            if (bus1.IsConnected == true)
            {
                if (Bus1_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        bus1.WritePort(0, 0xFF);
                    }
                    else
                    {
                        bus1.WritePort(0, 0x00);
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
                WriteMessage("Bus 1 not connected.");
            }
        }

        private void Bus1_EnablePort1(object sender, RoutedEventArgs e)
        {
            // get the value from the enable port checkbox and set the port values to the required state using the WritePort method
            CheckBox cb = (CheckBox)sender;
            if (bus1.IsConnected == true)
            {
                if (Bus1_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        bus1.WritePort(1, 0xFF);
                    }
                    else
                    {
                        bus1.WritePort(1, 0x00);
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
                WriteMessage("Bus 1 not connected.");
            }
        }

        private void Bus2_EnablePort0(object sender, RoutedEventArgs e)
        {
            // get the value from the enable port checkbox and set the port values to the required state using the WritePort method
            CheckBox cb = (CheckBox)sender;
            if (bus2.IsConnected == true)
            {
                if (Bus2_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        bus2.WritePort(0, 0xFF);
                    }
                    else
                    {
                        bus2.WritePort(0, 0x00);
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
                WriteMessage("Bus 2 not connected.");
            }
        }

        private void Bus2_EnablePort1(object sender, RoutedEventArgs e)
        {
            // get the value from the enable port checkbox and set the port values to the required state using the WritePort method
            CheckBox cb = (CheckBox)sender;
            if (bus2.IsConnected == true)
            {
                if (Bus2_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        bus2.WritePort(1, 0xFF);
                    }
                    else
                    {
                        bus2.WritePort(1, 0x00);
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
                WriteMessage("Bus 2 not connected.");
            }
        }

        private void Bus1_PinEnable(object sender, RoutedEventArgs e)
        {
            // check which pin checkbox was clicked and update the value of that pin to the required state
            CheckBox cb = (CheckBox)sender;
            if (bus1.IsConnected == true)
            {
                if (Bus1_Direction == false)
                {
                    if (cb.IsChecked == true)
                    {
                        bus1.WritePin(Convert.ToByte(cb.Content.ToString()), true);
                    }
                    else
                    {
                        bus1.WritePin(Convert.ToByte(cb.Content.ToString()), false);
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
                WriteMessage("Bus 2 not connected.");
            }
        }

        private void Bus2_PinEnable(object sender, RoutedEventArgs e)
        {
            // check which pin checkbox was clicked and update the value of that pin to the required state
            CheckBox cb = (CheckBox)sender;
            if (bus2.IsConnected == true) {
                if (Bus2_Direction == false) {            
                    if (cb.IsChecked == true)
                    {
                        bus2.WritePin(Convert.ToByte(cb.Content.ToString()), true);               
                    }
                    else
                    {
                        bus2.WritePin(Convert.ToByte(cb.Content.ToString()), false);
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
                WriteMessage("Bus 2 not connected.");
            }
        }


        private async void WriteMessage(string message)
        {
            // this method updates the Message textbox on the page
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txt_Message.Text = message;
            });
        }

        
        private void bt_Back_Clicked(object sender, RoutedEventArgs e)
        {
            // go back to the main page
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

    }
}
