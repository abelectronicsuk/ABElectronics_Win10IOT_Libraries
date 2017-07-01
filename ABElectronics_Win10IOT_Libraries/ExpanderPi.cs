using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Devices.I2c;
using Windows.Foundation.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ABElectronics_Win10IOT_Libraries
{
    /// <summary>
    /// Class Library for use with the Expander Pi
    /// </summary>
    public class ExpanderPi : IDisposable
    {
        // SPI Variables
        private const string SPI_CONTROLLER_NAME = "SPI0";
        private const Int32 ADC_CHIP_SELECT_LINE = 0; // ADC on SPI channel select CE0
        private const Int32 DAC_CHIP_SELECT_LINE = 1; // ADC on SPI channel select CE1

        // ADC Variables
        private SpiDevice adc;
        private double ADCReferenceVoltage = 4.096;

        // DAC Variables
        private SpiDevice dac;

        // IO Variables

        private readonly ABE_Helpers helper = new ABE_Helpers();

        // Define IO registers values from datasheet

        private const byte IODIRA = 0x00; // IO direction A
        private const byte IODIRB = 0x01; // IO direction B
        private const byte IPOLA = 0x02; // Input polarity A
        private const byte IPOLB = 0x03; // Input polarity B
        private const byte GPINTENA = 0x04; // Interrupt-on-change A
        private const byte GPINTENB = 0x05; // Interrupt-on-change B
        private const byte DEFVALA = 0x06; // Default value for port A
        private const byte DEFVALB = 0x07; // Default value for port B
        private const byte INTCONA = 0x08; // Interrupt control register for port A
        private const byte INTCONB = 0x09; // Interrupt control register for port B
        private const byte IOCON = 0x0A; // configuration register
        private const byte GPPUA = 0x0C; // pull-up resistors for port A
        private const byte GPPUB = 0x0D; // pull-up resistors for port B
        private const byte INTFA = 0x0E; // interrupt condition on port A
        private const byte INTFB = 0x0F; // interrupt condition on port B
        private const byte INTCAPA = 0x10; // captures the GPIO port A value at the time the interrupt occurred
        private const byte INTCAPB = 0x11; // captures the GPIO port B value at the time the interrupt occurred
        private const byte GPIOA = 0x12; // Data port A
        private const byte GPIOB = 0x13; // Data port B
        private const byte OLATA = 0x14; // Output latches A
        private const byte OLATB = 0x15; // Output latches B
        private const byte IOADDRESS = 0x20; // I2C Address for the MCP23017 IO chip

        private byte config = 0x22; // initial configuration - see IOCON page in the MCP23017 datasheet for more information.

        private I2cDevice IOi2cbus; // create an instance of the i2c bus
        private byte intA; // interrupt control for port a
        private byte intB; // interrupt control for port a

        // variables
        private byte port_a_dir; // port a direction
        private byte port_b_dir; // port b direction
        private byte porta_polarity; // input polarity for port a
        private byte porta_pullup; // port a pull-up resistors
        private byte portaval; // port a value
        private byte portb_polarity; // input polarity for port b
        private byte portb_pullup; // port a pull-up resistors
        private byte portbval; // port b value

        // RTC Variables

        private const byte RTCADDRESS = 0x68;
        private I2cDevice RTCi2cbus; // create an instance of the i2c bus

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
        private byte rtcconfig = 0x03;

        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);


        #region General Methods

        /// <summary>
        ///     Shows if there is a connection with the Expander Pi
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     Create an instance of the Expander Pi.
        /// </summary>
        public ExpanderPi(){
            IsConnected = false;
        }


        /// <summary>
        ///     Open a connection to the Expander Pi.
        /// </summary>
        public async Task Connect()
        {
            if (IsConnected)
            {
                return; // Already connected
            }

            if (!ApiInformation.IsTypePresent("Windows.Devices.Spi.SpiDevice"))
            {
                return; // This system does not support this feature: can't connect
            }

            if (!ApiInformation.IsTypePresent("Windows.Devices.I2c.I2cDevice"))
            {
                return; // This system does not support this feature: can't connect
            }

            try
            {
                
                // Create SPI initialization settings for the ADC
                var adcsettings =
                    new SpiConnectionSettings(ADC_CHIP_SELECT_LINE)
                    {
                        ClockFrequency = 1900000, // SPI clock frequency of 1.9MHz
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

                
                
                // Initialize the I2C bus 

                var aqs = I2cDevice.GetDeviceSelector(ABE_Helpers.I2C_CONTROLLER_NAME); // Find the selector string for the I2C bus controller
                var dis = await DeviceInformation.FindAllAsync(aqs); // Find the I2C bus controller device with our selector string

                if (dis.Count == 0)
                {
                    return; // Controller not found
                }

                var IOsettings = new I2cConnectionSettings(IOADDRESS) { BusSpeed = I2cBusSpeed.FastMode };

                var RTCsettings = new I2cConnectionSettings(RTCADDRESS) { BusSpeed = I2cBusSpeed.FastMode };

                IOi2cbus = await I2cDevice.FromIdAsync(dis[0].Id, IOsettings); // Create an IOI2cDevice with our selected bus controller and I2C settings
                RTCi2cbus = await I2cDevice.FromIdAsync(dis[0].Id, RTCsettings); // Create an RTCI2cDevice with our selected bus controller and I2C settings

                
                if (IOi2cbus != null && RTCi2cbus != null)
                {
                    // Set IsConnected to true
                    IsConnected = true;

                    // i2c bus is connected so set up the initial configuration for the IO Pi
                    helper.WriteI2CByte(IOi2cbus, IOCON, config);
                    
                    portaval = helper.ReadI2CByte(IOi2cbus, GPIOA);
                    portbval = helper.ReadI2CByte(IOi2cbus, GPIOB);
                    helper.WriteI2CByte(IOi2cbus, IODIRA, 0xFF);
                    helper.WriteI2CByte(IOi2cbus, IODIRB, 0xFF);
                    IOSetPortPullups(0, 0x00);
                    IOSetPortPullups(1, 0x00);
                    IOInvertPort(0, 0x00);
                    IOInvertPort(1, 0x00);

                    // Fire the Connected event handler
                    Connected?.Invoke(this, EventArgs.Empty);
                    

                }

                
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                IsConnected = false;
                throw new Exception("SPI and I2C Initialization Failed", ex);
            }
        }

        /// <summary>
        ///     Event occurs when connection is made.
        /// </summary>
        public event EventHandler Connected;

        private void CheckConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected. You must call .Connect() first.");
            }
        }
#endregion

        #region ADC Methods


        /// <summary>
        ///     Read the voltage from the selected <paramref name="channel" /> on the ADC.
        /// </summary>
        /// <param name="channel">1 to 8</param>
        /// <param name="mode">1 = Single Ended Input, 2 = Differential Input</param>
        /// When in differential mode setting channel to 1 will make IN1 = IN+ and IN2 = IN-
        /// When in differential mode setting channel to 2 will make IN1 = IN- and IN2 = IN+
        /// When in differential mode setting channel to 3 will make IN3 = IN+ and IN4 = IN-
        /// When in differential mode setting channel to 4 will make IN3 = IN- and IN4 = IN+
        /// When in differential mode setting channel to 5 will make IN5 = IN+ and IN6 = IN-
        /// When in differential mode setting channel to 6 will make IN5 = IN- and IN6 = IN+
        /// When in differential mode setting channel to 7 will make IN7 = IN+ and IN8 = IN-
        /// When in differential mode setting channel to 8 will make IN7 = IN- and IN8 = IN+
        /// <returns>voltage</returns>
        public double ADCReadVoltage(byte channel, byte mode)
        {
            if (channel < 1 || channel > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            var raw = ADCReadRaw(channel, mode);
            var voltage = ADCReferenceVoltage / 4096 * raw; // convert the raw value into a voltage based on the reference voltage.
            return voltage;
        }


        /// <summary>
        ///     Read the raw value from the selected <paramref name="channel" /> on the ADC.
        /// </summary>
        /// <param name="channel">1 to 8</param>
        /// <param name="mode">1 = Single Ended Input, 2 = Differential Input</param>
        /// When in differential mode setting channel to 1 will make IN1 = IN+ and IN2 = IN-
        /// When in differential mode setting channel to 2 will make IN1 = IN- and IN2 = IN+
        /// When in differential mode setting channel to 3 will make IN3 = IN+ and IN4 = IN-
        /// When in differential mode setting channel to 4 will make IN3 = IN- and IN4 = IN+
        /// When in differential mode setting channel to 5 will make IN5 = IN+ and IN6 = IN-
        /// When in differential mode setting channel to 6 will make IN5 = IN- and IN6 = IN+
        /// When in differential mode setting channel to 7 will make IN7 = IN+ and IN8 = IN-
        /// When in differential mode setting channel to 8 will make IN7 = IN- and IN8 = IN+
        /// <returns>Integer</returns>
        public int ADCReadRaw(byte channel, byte mode)
        {
            if (channel < 1 || channel > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            CheckConnected();

            var writeArray = new byte[] { 0x00, 0x00, 0x00 };

            channel = (byte)(channel - 1);

            if (mode == 0)
            {
                writeArray[0] = (byte)(6 + (channel >> 2));
                writeArray[1] = (byte)((channel & 3) << 6);
            }
            else if (mode == 1)
            {
                writeArray[0] = (byte)(4 + (channel >> 2));
                writeArray[1] = (byte)((channel & 3) << 6);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            var readBuffer = new byte[3]; // this holds the output data

            adc.TransferFullDuplex(writeArray, readBuffer); // transfer the adc data

            var ret = (short)(((readBuffer[1] & 0x0F) << 8) + readBuffer[2]); // combine the two bytes into a single 16bit integer

            return ret;
        }

        /// <summary>
        ///     Set the reference <paramref name="voltage" /> for the analogue to digital converter.
        ///     The Expander Pi contains an onboard 4.096V voltage reference.  If you want to use an external
        ///     reference between 0V and 5V, disconnect the jumper J1 and connect your reference voltage to the Vref pin.
        /// </summary>
        /// <param name="voltage">double</param>
        public void ADCSetRefVoltage(double voltage)
        {
            CheckConnected();

            if (voltage < 0.0 || voltage > 5.0)
            {
                throw new ArgumentOutOfRangeException(nameof(voltage), "Reference voltage must be between 0.0V and 5.0V.");
            }

            ADCReferenceVoltage = voltage;
        }

        #endregion

        #region DAC Methods


        /// <summary>
        ///     Set the <paramref name="voltage" /> for the selected channel on the DAC.
        /// </summary>
        /// <param name="channel">1 or 2</param>
        /// <param name="voltage">Voltage will be between 0 and 2.047V when gain is 1, 0 and 4.096V when gain is 2</param>
        /// <param name="gain">Gain can be 1 or 2</param>
        public void DACSetVoltage(byte channel, double voltage, byte gain)
        {
            // Check for valid channel and voltage variables
            if (channel < 1 || channel > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            if (gain < 1 || gain > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(gain));
            }

            if ((gain == 1 && (voltage >= 0.0 && voltage <= 2.048)) || (gain == 2 && (voltage >= 0.0 && voltage <= 4.096)))
            {
                var rawval = Convert.ToInt16(((voltage / 2.048) * 4096) / gain); // convert the voltage into a raw value
                DACSetRaw(channel, rawval, gain);
            }

            else
            {
                throw new ArgumentOutOfRangeException(nameof(voltage));
            }
        }

        /// <summary>
        ///     Set the raw <paramref name="value" /> from the selected <paramref name="channel" /> on the DAC.
        /// </summary>
        /// <param name="channel">1 or 2</param>
        /// <param name="value">Value between 0 and 4095</param>
        /// <param name="gain">Gain can be 1 or 2</param>
        /// Voltage will be between 0 and 2.047V when gain is 1, 0 and 4.096V when gain is 2
        public void DACSetRaw(byte channel, short value, byte gain)
        {
            CheckConnected();

            if (channel < 1 || channel > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(channel));
            }

            if (value < 0 || value > 4095)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (gain < 1 || gain > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(gain));
            }

            // split the raw value into two bytes and send it to the DAC.
            var lowByte = (byte)(value & 0xff);
            var highByte = (byte)0;

            if (gain == 1)
            {
                highByte = (byte)(((value >> 8) & 0xff) | ((channel - 1) << 7) | (1 << 5) | (1 << 4));
            }
            else
            {
                highByte = (byte)(((value >> 8) & 0xff) | ((channel - 1) << 7) | (1 << 4));
            }

            var writeBuffer = new[] { highByte, lowByte };
            dac.Write(writeBuffer);
        }

        #endregion

        #region IO Methods

        /// <summary>
        ///     Set IO <paramref name="direction" /> for an individual pin.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="direction">true = input, false = output</param>
        public void IOSetPinDirection(byte pin, bool direction)
        {
            CheckConnected();

            pin = (byte)(pin - 1);

            if (pin < 8)
            {
                port_a_dir = helper.UpdateByte(port_a_dir, pin, direction);
                helper.WriteI2CByte(IOi2cbus, IODIRA, port_a_dir);
            }
            else if (pin >= 8 && pin < 16)
            {
                port_b_dir = helper.UpdateByte(port_b_dir, (byte)(pin - 8), direction);
                helper.WriteI2CByte(IOi2cbus, IODIRB, port_b_dir);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pin));
            }
        }

        /// <summary>
        ///     Set the <paramref name="direction"/> for an IO <paramref name="port"/>.
        ///     You can control the direction of all 8 pins on a port by sending a single byte value.
        ///     Each bit in the byte represents one pin so for example 0x0A would set pins 2 and 4 to
        ///     inputs and all other pins to outputs.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="direction">Direction for all pins on the port.  1 = input, 0 = output</param>
        public void IOSetPortDirection(byte port, byte direction)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(IOi2cbus, IODIRA, direction);
                    port_a_dir = direction;
                    break;
                case 1:
                    helper.WriteI2CByte(IOi2cbus, IODIRB, direction);
                    port_b_dir = direction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Set the internal 100K pull-up resistors for an individual pin.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="value">true = enabled, false = disabled</param>
        public void IOSetPinPullup(byte pin, bool value)
        {
            CheckConnected();

            pin = (byte)(pin - 1);

            if (pin < 8)
            {
                porta_pullup = helper.UpdateByte(porta_pullup, pin, value);
                helper.WriteI2CByte(IOi2cbus, GPPUA, porta_pullup);
            }
            else if (pin >= 8 && pin < 16)
            {
                portb_pullup = helper.UpdateByte(portb_pullup, (byte)(pin - 8), value);
                helper.WriteI2CByte(IOi2cbus, GPPUB, portb_pullup);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pin));
            }
        }

        /// <summary>
        ///     set the internal 100K pull-up resistors for the selected IO port.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void IOSetPortPullups(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    porta_pullup = value;
                    helper.WriteI2CByte(IOi2cbus, GPPUA, value);
                    break;
                case 1:
                    portb_pullup = value;
                    helper.WriteI2CByte(IOi2cbus, GPPUB, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Write to an individual <paramref name="pin"/>.
        /// </summary>
        /// <param name="pin">1 - 16</param>
        /// <param name="value">0 = logic low, 1 = logic high</param>
        public void IOWritePin(byte pin, bool value)
        {
            CheckConnected();

            pin = (byte)(pin - 1);
            if (pin < 8)
            {
                portaval = helper.UpdateByte(portaval, pin, value);
                helper.WriteI2CByte(IOi2cbus, GPIOA, portaval);
            }
            else if (pin >= 8 && pin < 16)
            {
                portbval = helper.UpdateByte(portbval, (byte)(pin - 8), value);
                helper.WriteI2CByte(IOi2cbus, GPIOB, portbval);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pin));
            }
        }

        /// <summary>
        ///     Write to all pins on the selected <paramref name="port"/>.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void IOWritePort(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(IOi2cbus, GPIOA, value);
                    portaval = value;
                    break;
                case 1:
                    helper.WriteI2CByte(IOi2cbus, GPIOB, value);
                    portbval = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     read the value of an individual <paramref name="pin"/>.
        /// </summary>
        /// <param name="pin">1 - 16</param>
        /// <returns>0 = logic level low, 1 = logic level high</returns>
        public bool IOReadPin(byte pin)
        {
            CheckConnected();

            pin = (byte)(pin - 1);
            if (pin < 8)
            {
                portaval = helper.ReadI2CByte(IOi2cbus, GPIOA);
                return helper.CheckBit(portaval, pin);
            }
            if (pin >= 8 && pin < 16)
            {
                portbval = helper.ReadI2CByte(IOi2cbus, GPIOB);
                return helper.CheckBit(portbval, (byte)(pin - 8));
            }
            throw new ArgumentOutOfRangeException(nameof(pin));
        }

        /// <summary>
        ///     Read all pins on the selected <paramref name="port"/>.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <returns>returns number between 0 and 255 or 0x00 and 0xFF</returns>
        public byte IOReadPort(byte port)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    portaval = helper.ReadI2CByte(IOi2cbus, GPIOA);
                    return portaval;
                case 1:
                    portbval = helper.ReadI2CByte(IOi2cbus, GPIOB);
                    return portbval;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Invert the polarity of the pins on a selected <paramref name="port"/>.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="polarity">0x00 - 0xFF (0 = same logic state of the input pin, 1 = inverted logic state of the input pin)</param>
        public void IOInvertPort(byte port, byte polarity)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(IOi2cbus, IPOLA, polarity);
                    porta_polarity = polarity;
                    break;
                case 1:
                    helper.WriteI2CByte(IOi2cbus, IPOLB, polarity);
                    portb_polarity = polarity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Invert the <paramref name="polarity" /> of the selected <paramref name="pin" />.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="polarity">False = same logic state of the input pin, True = inverted logic state of the input pin</param>
        public void IOInvertPin(byte pin, bool polarity)
        {
            CheckConnected();

            pin = (byte)(pin - 1);
            if (pin < 8)
            {
                porta_polarity = helper.UpdateByte(portaval, pin, polarity);
                helper.WriteI2CByte(IOi2cbus, IPOLA, porta_polarity);
            }
            else if (pin >= 8 && pin < 16)
            {
                portb_polarity = helper.UpdateByte(portbval, (byte)(pin - 8), polarity);
                helper.WriteI2CByte(IOi2cbus, IPOLB, portb_polarity);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pin));
            }
        }

        /// <summary>
        ///     Sets the mirror status of the interrupt pins.
        /// </summary>
        /// <param name="value">
        ///     0 = The INT pins are not mirrored. INTA is associated with PortA and INTB is associated with PortB.
        ///     1 = The INT pins are internally connected
        /// </param>
        public void IOMirrorInterrupts(byte value)
        {
            CheckConnected();

            switch (value)
            {
                case 0:
                    config = helper.UpdateByte(config, 6, false);
                    helper.WriteI2CByte(IOi2cbus, IOCON, config);
                    break;
                case 1:
                    config = helper.UpdateByte(config, 6, true);
                    helper.WriteI2CByte(IOi2cbus, IOCON, config);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        ///     This sets the polarity of the INT output pins.
        /// </summary>
        /// <param name="value">1 = Active - high. 0 = Active - low.</param>
        public void IOSetInterruptPolarity(byte value)
        {
            CheckConnected();

            switch (value)
            {
                case 0:
                    config = helper.UpdateByte(config, 1, false);
                    helper.WriteI2CByte(IOi2cbus, IOCON, config);
                    break;
                case 1:
                    config = helper.UpdateByte(config, 1, true);
                    helper.WriteI2CByte(IOi2cbus, IOCON, config);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        ///     Sets the type of interrupt for each pin on the selected <paramref name="port"/>.
        ///     1 = interrupt is fired when the pin matches the default value.
        ///     0 = the interrupt is fired on state change.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void IOSetInterruptType(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(IOi2cbus, INTCONA, value);
                    break;
                case 1:
                    helper.WriteI2CByte(IOi2cbus, INTCONB, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     These bits set the compare value for pins configured for interrupt-on-change
        ///     on the selected <paramref name="port"/>. If the associated pin level is the
        ///     opposite from the register bit, an interrupt occurs.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void IOSetInterruptDefaults(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(IOi2cbus, DEFVALA, value);
                    break;
                case 1:
                    helper.WriteI2CByte(IOi2cbus, DEFVALB, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Enable interrupts for the pins on the selected <paramref name="port"/>.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void IOSetInterruptOnPort(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(IOi2cbus, GPINTENA, value);
                    intA = value;
                    break;
                case 1:
                    helper.WriteI2CByte(IOi2cbus, GPINTENB, value);
                    intB = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Enable interrupts for the selected <paramref name="pin"/>.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="value">0 = interrupt disabled, 1 = interrupt enabled</param>
        public void IOSetInterruptOnPin(byte pin, bool value)
        {
            CheckConnected();

            pin = (byte)(pin - 1);
            if (pin < 8)
            {
                intA = helper.UpdateByte(intA, pin, value);
                helper.WriteI2CByte(IOi2cbus, GPINTENA, intA);
            }
            else if (pin >= 8 && pin < 16)
            {
                intB = helper.UpdateByte(intB, (byte)(pin - 8), value);
                helper.WriteI2CByte(IOi2cbus, GPINTENB, intB);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pin));
            }
        }

        /// <summary>
        ///     Read the interrupt status for the pins on the selected <paramref name="port"/>.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        public byte IOReadInterruptStatus(byte port)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    return helper.ReadI2CByte(IOi2cbus, INTFA);
                case 1:
                    return helper.ReadI2CByte(IOi2cbus, INTFB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Read the value from the selected <paramref name="port"/> at the time
        ///     of the last interrupt trigger.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        public byte IOReadInterruptCapture(byte port)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    return helper.ReadI2CByte(IOi2cbus, INTCAPA);
                case 1:
                    return helper.ReadI2CByte(IOi2cbus, INTCAPB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Reset the interrupts A and B to 0.
        /// </summary>
        public void IOResetInterrupts()
        {
            CheckConnected();

            IOReadInterruptCapture(0);
            IOReadInterruptCapture(1);
        }
#endregion

        #region RTC Methods        

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
            return (byte)(val / 10 * 16 + val % 10);
        }


        /// <summary>
        ///     Set the date and time on the RTC.
        /// </summary>
        /// <param name="date">DateTime</param>
        public void RTCSetDate(DateTime date)
        {
            CheckConnected();

            helper.WriteI2CByte(RTCi2cbus, SECONDS, BytetoBCD(date.Second));
            helper.WriteI2CByte(RTCi2cbus, MINUTES, BytetoBCD(date.Minute));
            helper.WriteI2CByte(RTCi2cbus, HOURS, BytetoBCD(date.Hour));
            helper.WriteI2CByte(RTCi2cbus, DAYOFWEEK, BytetoBCD((int)date.DayOfWeek));
            helper.WriteI2CByte(RTCi2cbus, DAY, BytetoBCD(date.Day));
            helper.WriteI2CByte(RTCi2cbus, MONTH, BytetoBCD(date.Month));
            helper.WriteI2CByte(RTCi2cbus, YEAR, BytetoBCD(date.Year - century));
        }

        /// <summary>
        ///     Read the date and time from the RTC.
        /// </summary>
        /// <returns>DateTime</returns>
        public DateTime RTCReadDate()
        {
            CheckConnected();

            var DateArray = helper.ReadI2CBlockData(RTCi2cbus, 0, 7);
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
        public void RTCEnableOutput()
        {
            CheckConnected();

            rtcconfig = helper.UpdateByte(rtcconfig, 7, true);
            rtcconfig = helper.UpdateByte(rtcconfig, 4, true);
            helper.WriteI2CByte(RTCi2cbus, CONTROL, rtcconfig);
        }

        /// <summary>
        ///     Disable the clock output pin.
        /// </summary>
        public void RTCDisableOutput()
        {
            CheckConnected();

            rtcconfig = helper.UpdateByte(rtcconfig, 7, false);
            rtcconfig = helper.UpdateByte(rtcconfig, 4, false);
            helper.WriteI2CByte(RTCi2cbus, CONTROL, rtcconfig);
        }

        /// <summary>
        ///     Set the frequency of the output pin square-wave.
        /// </summary>
        /// <param name="frequency">options are: 1 = 1Hz, 2 = 4.096KHz, 3 = 8.192KHz, 4 = 32.768KHz</param>
        public void RTCSetFrequency(byte frequency)
        {
            CheckConnected();

            switch (frequency)
            {
                case 1:
                    rtcconfig = helper.UpdateByte(rtcconfig, 0, false);
                    rtcconfig = helper.UpdateByte(rtcconfig, 1, false);
                    break;
                case 2:
                    rtcconfig = helper.UpdateByte(rtcconfig, 0, true);
                    rtcconfig = helper.UpdateByte(rtcconfig, 1, false);
                    break;
                case 3:
                    rtcconfig = helper.UpdateByte(rtcconfig, 0, false);
                    rtcconfig = helper.UpdateByte(rtcconfig, 1, true);
                    break;
                case 4:
                    rtcconfig = helper.UpdateByte(rtcconfig, 0, true);
                    rtcconfig = helper.UpdateByte(rtcconfig, 1, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(frequency));
            }
            helper.WriteI2CByte(RTCi2cbus, CONTROL, rtcconfig);
        }

        #endregion

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

                IOi2cbus?.Dispose();
                IOi2cbus = null;

                RTCi2cbus?.Dispose();
                RTCi2cbus = null;

                IsConnected = false;
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

    }
}
