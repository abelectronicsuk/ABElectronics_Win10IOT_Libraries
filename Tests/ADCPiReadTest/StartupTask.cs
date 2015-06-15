
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using ABElectronics_Win10IOT_Libraries;
using System.Diagnostics;
using System.Threading;
using System;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ADCPiReadTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        GpioPin pin;
        ABElectronics_Win10IOT_Libraries.ADCPi adc = new ADCPi(0x68, 0x69);
        double readvalue = 0;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // set up GPIO 18 so it can toggle after each sample read.
            var gpio = GpioController.GetDefault();

            pin = gpio.OpenPin(18);

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin.SetDriveMode(GpioPinDriveMode.Output);
          
            adc.Connect();

            while (!adc.IsConnected)
            {

            }
            Debug.WriteLine("ADC Connected");
            adc.SetBitRate(18);
            adc.SetPGA(1);
            while (true)
            {
                readvalue = adc.ReadVoltage(1);
                // toggle GPIO pin to show a value has been read
                pin.Write(GpioPinValue.High);
                pin.Write(GpioPinValue.Low);
            }
        }

    }
}
