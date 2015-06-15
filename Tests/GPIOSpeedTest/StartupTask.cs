using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// Speed test for writing to the GPIO pins.  Toggles the state of GPIO18 as fast as possible.

namespace GPIOSpeedTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var gpio = GpioController.GetDefault();
          
            GpioPin pin = gpio.OpenPin(18);

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin.SetDriveMode(GpioPinDriveMode.Output);

            while (true)
            {
                pin.Write(GpioPinValue.High);
                pin.Write(GpioPinValue.Low);
            }


        }
    }
}
