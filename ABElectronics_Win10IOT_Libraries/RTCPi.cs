using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation.Metadata;

namespace ABElectronics_Win10IOT_Libraries
{
    /// <summary>
    ///     Class for controlling the RTC Pi and RTC Pi Plus expansion boards from AB Electronics UK
    ///     Based on the DS1307 real-time clock from Maxim.
    /// </summary>
    public class RTCPi : IDisposable
    {
        // Register addresses for the DS1307 IC
        private const byte SECONDS = 0x00;
        private const byte MINUTES = 0x01;
        private const byte HOURS = 0x02;
        private const byte DAYOFWEEK = 0x03;
        private const byte DAY = 0x04;
        private const byte MONTH = 0x05;
        private const byte YEAR = 0x06;
        private const byte CONTROL = 0x07;

        // the DS1307 does not store the current century so that has to be added on manually.
        private readonly int century = 2000;

        // initial configuration - square wave and output disabled, frequency set to 32.768KHz.
        private byte config = 0x03;
        private readonly ABE_Helpers helper = new ABE_Helpers();

        private I2cDevice i2cbus; // create an instance of the i2c bus.

        // variables
        private readonly byte rtcAddress = 0x68; // I2C address

        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        System.Runtime.InteropServices.SafeHandle handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        ///     Create an instance of a RTC Pi bus.
        /// </summary>
        public RTCPi()
        {
            IsConnected = false;
        }

        /// <summary>
        ///     Shows if there is a connection with the RTC Pi.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     Open a connection with the RTC Pi.
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            if (IsConnected)
            {
                return; // Already connected
            }

            if (!ApiInformation.IsTypePresent("Windows.Devices.I2c.I2cDevice"))
            {
                return; // This system does not support this feature: can't connect
            }

            /* Initialize the I2C bus */
            try
            {
                var aqs = I2cDevice.GetDeviceSelector(ABE_Helpers.I2C_CONTROLLER_NAME); // Find the selector string for the I2C bus controller
                var dis = await DeviceInformation.FindAllAsync(aqs); // Find the I2C bus controller device with our selector string

                if (dis.Count == 0)
                {
                    return; // Controller not found
                }

                var settings = new I2cConnectionSettings(rtcAddress) {BusSpeed = I2cBusSpeed.FastMode};

                i2cbus = await I2cDevice.FromIdAsync(dis[0].Id, settings); // Create an I2cDevice with our selected bus controller and I2C settings
                if (i2cbus != null)
                {
                    // i2c bus is connected so set IsConnected to true and fire the Connected event handler
                    IsConnected = true;

                    // Fire the Connected event handler
                    Connected?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw new Exception("I2C Initialization Failed", ex);
            }
        }

        /// <summary>
        ///     Event occurs when connection is made.
        /// </summary>
        public event EventHandler Connected;


        /// <summary>
        ///     Converts BCD format to integer.
        /// </summary>
        /// <param name="x">BCD formatted byte</param>
        /// <returns></returns>
        private int BCDtoInt(byte x)
        {
            return x - 6 * (x >> 4);
        }

        /// <summary>
        ///     Converts byte to BCD format.
        /// </summary>
        /// <param name="val">value to convert</param>
        /// <returns>Converted byte</returns>
        private byte BytetoBCD(int val)
        {
            return (byte) (val / 10 * 16 + val % 10);
        }


        /// <summary>
        ///     Set the date and time on the RTC.
        /// </summary>
        /// <param name="date">DateTime</param>
        public void SetDate(DateTime date)
        {
            CheckConnected();

            helper.WriteI2CByte(i2cbus, SECONDS, BytetoBCD(date.Second));
            helper.WriteI2CByte(i2cbus, MINUTES, BytetoBCD(date.Minute));
            helper.WriteI2CByte(i2cbus, HOURS, BytetoBCD(date.Hour));
            helper.WriteI2CByte(i2cbus, DAYOFWEEK, BytetoBCD((int) date.DayOfWeek));
            helper.WriteI2CByte(i2cbus, DAY, BytetoBCD(date.Day));
            helper.WriteI2CByte(i2cbus, MONTH, BytetoBCD(date.Month));
            helper.WriteI2CByte(i2cbus, YEAR, BytetoBCD(date.Year - century));
        }

        /// <summary>
        ///     Read the date and time from the RTC.
        /// </summary>
        /// <returns>DateTime</returns>
        public DateTime ReadDate()
        {
            CheckConnected();

            var DateArray = helper.ReadI2CBlockData(i2cbus, 0, 7);
            var year = BCDtoInt(DateArray[6]) + century;
            var month = BCDtoInt(DateArray[5]);
            var day = BCDtoInt(DateArray[4]);
            // var dayofweek = BCDtoInt(DateArray[3]);
            var hours = BCDtoInt(DateArray[2]);
            var minutes = BCDtoInt(DateArray[1]);
            var seconds = BCDtoInt(DateArray[0]);

            try
            {
                var date = new DateTime(year, month, day, hours, minutes, seconds);
                return date;
            }
            catch
            {
                var date = new DateTime(1990, 01, 01, 01, 01, 01);
                return date;
            }
        }

        /// <summary>
        ///     Enable the clock output pin.
        /// </summary>
        public void EnableOutput()
        {
            CheckConnected();

            config = helper.UpdateByte(config, 7, true);
            config = helper.UpdateByte(config, 4, true);
            helper.WriteI2CByte(i2cbus, CONTROL, config);
        }

        /// <summary>
        ///     Disable the clock output pin.
        /// </summary>
        public void DisableOutput()
        {
            CheckConnected();

            config = helper.UpdateByte(config, 7, false);
            config = helper.UpdateByte(config, 4, false);
            helper.WriteI2CByte(i2cbus, CONTROL, config);
        }

        /// <summary>
        ///     Set the frequency of the output pin square-wave.
        /// </summary>
        /// <param name="frequency">options are: 1 = 1Hz, 2 = 4.096KHz, 3 = 8.192KHz, 4 = 32.768KHz</param>
        public void SetFrequency(byte frequency)
        {
            CheckConnected();

            switch (frequency)
            {
                case 1:
                    config = helper.UpdateByte(config, 0, false);
                    config = helper.UpdateByte(config, 1, false);
                    break;
                case 2:
                    config = helper.UpdateByte(config, 0, true);
                    config = helper.UpdateByte(config, 1, false);
                    break;
                case 3:
                    config = helper.UpdateByte(config, 0, false);
                    config = helper.UpdateByte(config, 1, true);
                    break;
                case 4:
                    config = helper.UpdateByte(config, 0, true);
                    config = helper.UpdateByte(config, 1, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(frequency));
            }
            helper.WriteI2CByte(i2cbus, CONTROL, config);
        }

        private void CheckConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected. You must call .Connect() first.");
            }
        }


        /// <summary>
        ///     Dispose of the resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                i2cbus?.Dispose();
                i2cbus = null;

                IsConnected = false;
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }
}