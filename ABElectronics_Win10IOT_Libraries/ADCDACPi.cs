using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Foundation.Metadata;

namespace ABElectronics_Win10IOT_Libraries
{
    /// <summary>
    ///     Class for accessing the ADCDAC Pi from AB Electronics UK.
    /// </summary>
    public class ADCDACPi : IDisposable
    {
        private const string SPI_CONTROLLER_NAME = "SPI0";
        private const Int32 ADC_CHIP_SELECT_LINE = 0; // ADC on SPI channel select CE0
        private const Int32 DAC_CHIP_SELECT_LINE = 1; // ADC on SPI channel select CE1

        private SpiDevice adc;
        private double ADCReferenceVoltage = 3.3;
        private SpiDevice dac;

        /// <summary>
        ///     Event triggers when a connection is established.
        /// </summary>
        public bool IsConnected { get; private set; }

        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        System.Runtime.InteropServices.SafeHandle handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        ///     Open a connection to the ADCDAC Pi.
        /// </summary>
        public async void Connect()
        {
            if (IsConnected)
            {
                return; // Already connected
            }

            if(!ApiInformation.IsTypePresent("Windows.Devices.Spi.SpiDevice"))
            {
                return; // This system does not support this feature: can't connect
            }

            try
            {
                // Create SPI initialization settings for the ADC
                var adcsettings =
                    new SpiConnectionSettings(ADC_CHIP_SELECT_LINE)
                    {
                        ClockFrequency = 10000000, // SPI clock frequency of 10MHz
                        Mode = SpiMode.Mode0
                    };


                var spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME); // Find the selector string for the SPI bus controller
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs); // Find the SPI bus controller device with our selector string

                if (devicesInfo.Count == 0)
                {
                    return; // Controller not found
                }

                adc = await SpiDevice.FromIdAsync(devicesInfo[0].Id, adcsettings); // Create an ADC connection with our bus controller and SPI settings

                // Create SPI initialization settings for the DAC
                var dacSettings =
                    new SpiConnectionSettings(DAC_CHIP_SELECT_LINE)
                    {
                        ClockFrequency = 2000000,  // SPI clock frequency of 20MHz
                        Mode = SpiMode.Mode0
                    };

                dac = await SpiDevice.FromIdAsync(devicesInfo[0].Id, dacSettings); // Create a DAC connection with our bus controller and SPI settings

                IsConnected = true; // connection established, set IsConnected to true.

                // Fire the Connected event handler
                Connected?.Invoke(this, EventArgs.Empty);
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                IsConnected = false;
                throw new Exception("SPI Initialization Failed", ex);
            }
        }

        /// <summary>
        ///     Event occurs when connection is made.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        ///     Read the voltage from the selected <paramref name="channel" /> on the ADC.
        /// </summary>
        /// <param name="channel">1 or 2</param>
        /// <returns>voltage</returns>
        public double ReadADCVoltage(byte channel)
        {
            if (channel < 1 || channel > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            var raw = ReadADCRaw(channel);
            var voltage = ADCReferenceVoltage / 4096 * raw; // convert the raw value into a voltage based on the reference voltage.
            return voltage;
        }


        /// <summary>
        ///     Read the raw value from the selected <paramref name="channel" /> on the ADC.
        /// </summary>
        /// <param name="channel">1 or 2</param>
        /// <returns>Integer</returns>
        public int ReadADCRaw(byte channel)
        {
            if (channel < 1 || channel > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            CheckConnected();

            var writeArray = new byte[] { 0x01, (byte) ((1 + channel) << 6), 0x00}; // create the write bytes based on the input channel

            var readBuffer = new byte[3]; // this holds the output data

            adc.TransferFullDuplex(writeArray, readBuffer); // transfer the adc data

            var ret = (short) (((readBuffer[1] & 0x0F) << 8) + readBuffer[2]); // combine the two bytes into a single 16bit integer

            return ret;
        }

        /// <summary>
        ///     Set the reference <paramref name="voltage" /> for the analogue to digital converter.
        ///     The ADC uses the raspberry pi 3.3V power as a <paramref name="voltage" /> reference
        ///     so using this method to set the reference to match the exact output 
        ///     <paramref name="voltage" /> from the 3.3V regulator will increase the accuracy of
        ///     the ADC readings.
        /// </summary>
        /// <param name="voltage">double</param>
        public void SetADCrefVoltage(double voltage)
        {
            CheckConnected();

            if (voltage < 0.0 || voltage > 7.0)
            {
                throw new ArgumentOutOfRangeException(nameof(voltage), "Reference voltage must be between 0.0V and 7.0V.");
            }

            ADCReferenceVoltage = voltage;
        }

        /// <summary>
        ///     Set the <paramref name="voltage" /> for the selected channel on the DAC.
        /// </summary>
        /// <param name="channel">1 or 2</param>
        /// <param name="voltage">Voltage can be between 0 and 2.047 volts</param>
        public void SetDACVoltage(byte channel, double voltage)
        {
            // Check for valid channel and voltage variables
            if (channel < 1 || channel > 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (voltage >= 0.0 && voltage < 2.048)
            {
                var rawval = Convert.ToInt16(voltage / 2.048 * 4096); // convert the voltage into a raw value
                SetDACRaw(channel, rawval);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Set the raw <paramref name="value" /> from the selected <paramref name="channel" /> on the DAC.
        /// </summary>
        /// <param name="channel">1 or 2</param>
        /// <param name="value">Value between 0 and 4095</param>
        public void SetDACRaw(byte channel, short value)
        {
            CheckConnected();

            if (channel < 1 || channel > 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            // split the raw value into two bytes and send it to the DAC.
            var lowByte = (byte) (value & 0xff);
            var highByte = (byte) (((value >> 8) & 0xff) | ((channel - 1) << 7) | (0x1 << 5) | (1 << 4));
            var writeBuffer = new [] { highByte, lowByte};
            dac.Write(writeBuffer);
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
                adc?.Dispose();
                adc = null;

                dac?.Dispose();
                dac = null;

                IsConnected = false;
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }
}