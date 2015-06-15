using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DemoApplication
{
    /// <summary>
    /// This demonstration shows how to use the PWM outputs on the Servo Pi from AB Electronics UK
    /// </summary>
    public sealed partial class ServoPi : Page
    {

        ABElectronics_Win10IOT_Libraries.ServoPi servo = new ABElectronics_Win10IOT_Libraries.ServoPi();        

        public ServoPi()
        {
            this.InitializeComponent();
        }

        private async void bt_Connect_Click(object sender, RoutedEventArgs e)
        {
            // check if the servo pi is already connected and if the i2c address textbox contains a value
            if ((!servo.IsConnected) && (txt_Address.Text.Length > 0))
            {
                    try
                    {
                        // set the i2c address from the textbox value
                        servo.Address = Convert.ToByte(txt_Address.Text.Replace("0x", ""), 16);

                        // create a Connected event listener and connect to the servo pi
                        servo.Connected += Servo_Connected; ;
                        await servo.Connect();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
             }

        }

        private void Servo_Connected(object sender, EventArgs e)
        {
            // on connection get the value from the frequency slider and set the PWM frequency on the Servo Pi
            int frequency = Convert.ToInt32(slider_Frequency.Value);
            servo.SetPWMFreqency(frequency);
            WriteMessage("Connected");
        }

        private void FrequencySliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (servo.IsConnected)
            {
                // get the value from the frequency slider and set the PWM frequency on the Servo Pi
                int frequency = Convert.ToInt32(slider_Frequency.Value);
                servo.SetPWMFreqency(frequency);
            }
        }

       

        private void ChannelSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            
            if (servo.IsConnected)
            {
                // Find out which slider was changed and use the slider value to update the PWM value on the Servo Pi

                Slider slider = (Slider)sender;
                byte channel = Convert.ToByte(slider.Name.ToString().Replace("slider_Channel", ""));
                short highvalue = 0;
                short lowvalue = Convert.ToInt16(slider.Value);
                Debug.WriteLine(highvalue.ToString() + " " + lowvalue.ToString());
                servo.SetPWM(channel, highvalue, lowvalue);
            }
        }

        private async void WriteMessage(string message)
        {
            // used to update the message textbox on the page
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txt_Message.Text = message;
            });
        }

        private void bt_Back_Clicked(object sender, RoutedEventArgs e)
        {
            // dispose of the servo pi and go back to the main page
            try
            {
                servo.Dispose();
            }
            catch { }
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }
    }
}
