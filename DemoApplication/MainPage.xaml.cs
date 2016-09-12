using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DemoApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Frame rootFrame = Window.Current.Content as Frame;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void iopi_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(IOPi));
        }

        private void adc_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(ADCPi));
        }

        private void adcdifferentialpi_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(ADCDifferentialPi));
        }

        private void rtc_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(RTCPi));
        }

        private void adcdac_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(ADCDACPi));
        }

        private void servo_Click(object sender, RoutedEventArgs e)
        {
            rootFrame.Navigate(typeof(ServoPi));
        }

    }
}
