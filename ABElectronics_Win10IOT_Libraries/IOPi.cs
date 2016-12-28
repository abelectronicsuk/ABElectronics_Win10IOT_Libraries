using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation.Metadata;

namespace ABElectronics_Win10IOT_Libraries
{
    /// <summary>
    ///     Class for controlling the IO Pi and IO Pi Plus expansion boards from AB Electronics UK
    ///     Based on the MCP23017 IO expander IC from Microchip.
    /// </summary>
    public class IOPi : IDisposable
    {
        // Define registers values from datasheet

        /// <summary>
        ///     IO direction A - 1= input 0 = output
        /// </summary>
        private const byte IODIRA = 0x00;

        /// <summary>
        ///     IO direction B - 1= input 0 = output
        /// </summary>
        private const byte IODIRB = 0x01;

        /// <summary>
        ///     Input polarity A - If a bit is set, the corresponding GPIO register bit will reflect the inverted value on the pin.
        /// </summary>
        private const byte IPOLA = 0x02;

        /// <summary>
        ///     Input polarity B - If a bit is set, the corresponding GPIO register bit will reflect the inverted value on the pin.
        /// </summary>
        private const byte IPOLB = 0x03;

        /// <summary>
        ///     The GPINTEN register controls the interrupt-on-change feature for each pin on port A.
        /// </summary>
        private const byte GPINTENA = 0x04;

        /// <summary>
        ///     The GPINTEN register controls the interrupt-on-change feature for each pin on port B.
        /// </summary>
        private const byte GPINTENB = 0x05;

        /// <summary>
        ///     Default value for port A - These bits set the compare value for pins configured for interrupt-on-change. If the
        ///     associated pin level is the opposite from the register bit, an interrupt occurs.
        /// </summary>
        private const byte DEFVALA = 0x06;

        /// <summary>
        ///     Default value for port B - These bits set the compare value for pins configured for interrupt-on-change. If the
        ///     associated pin level is the opposite from the register bit, an interrupt occurs.
        /// </summary>
        private const byte DEFVALB = 0x07;

        /// <summary>
        ///     Interrupt control register for port A.  If 1 interrupt is fired when the pin matches the default value, if 0 the
        ///     interrupt is fired on state change.
        /// </summary>
        private const byte INTCONA = 0x08;

        /// <summary>
        ///     Interrupt control register for port B.  If 1 interrupt is fired when the pin matches the default value, if 0 the
        ///     interrupt is fired on state change.
        /// </summary>
        private const byte INTCONB = 0x09;

        /// <summary>
        ///     See datasheet for configuration register
        /// </summary>
        private const byte IOCON = 0x0A;

        /// <summary>
        ///     pull-up resistors for port A
        /// </summary>
        private const byte GPPUA = 0x0C;

        /// <summary>
        ///     pull-up resistors for port B
        /// </summary>
        private const byte GPPUB = 0x0D;

        /// <summary>
        ///     The INTFA register reflects the interrupt condition on the port A pins of any pin that is enabled for interrupts. A
        ///     set bit indicates that the associated pin caused the interrupt.
        /// </summary>
        private const byte INTFA = 0x0E;

        /// <summary>
        ///     The INTFB register reflects the interrupt condition on the port B pins of any pin that is enabled for interrupts. A
        ///     set bit indicates that the associated pin caused the interrupt.
        /// </summary>
        private const byte INTFB = 0x0F;

        /// <summary>
        ///     The INTCAP register captures the GPIO port A value at the time the interrupt occurred.
        /// </summary>
        private const byte INTCAPA = 0x10;

        /// <summary>
        ///     The INTCAP register captures the GPIO port B value at the time the interrupt occurred.
        /// </summary>
        private const byte INTCAPB = 0x11;

        /// <summary>
        ///     Data port A.
        /// </summary>
        private const byte GPIOA = 0x12;

        /// <summary>
        ///     Data port B.
        /// </summary>
        private const byte GPIOB = 0x13;

        /// <summary>
        ///     Output latches A.
        /// </summary>
        private const byte OLATA = 0x14;

        /// <summary>
        ///     Output latches B
        /// </summary>
        private const byte OLATB = 0x15;

        private readonly ABE_Helpers helper = new ABE_Helpers();


        private byte config = 0x22; // initial configuration - see IOCON page in the MCP23017 datasheet for more information.

        private I2cDevice i2cbus; // create an instance of the i2c bus
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

        /// <summary>
        ///     Create an instance of an IOPi bus.
        /// </summary>
        /// <param name="i2caddress">I2C Address of IO Pi bus</param>
        public IOPi(byte i2caddress)
        {
            Address = i2caddress;
            IsConnected = false;
        }

        /// <summary>
        ///     I2C address for the IO Pi bus
        /// </summary>
        public byte Address { get; set; }

        /// <summary>
        ///     Shows if there is a connection with the IO Pi
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///     Open a connection with the IO Pi.
        /// </summary>
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

                var settings = new I2cConnectionSettings(Address) {BusSpeed = I2cBusSpeed.FastMode};

                i2cbus = await I2cDevice.FromIdAsync(dis[0].Id, settings); // Create an I2cDevice with our selected bus controller and I2C settings
                if (i2cbus != null)
                {
                    // i2c bus is connected so set up the initial configuration for the IO Pi
                    helper.WriteI2CByte(i2cbus, IOCON, config);
                    portaval = helper.ReadI2CByte(i2cbus, GPIOA);
                    portbval = helper.ReadI2CByte(i2cbus, GPIOB);
                    helper.WriteI2CByte(i2cbus, IODIRA, 0xFF);
                    helper.WriteI2CByte(i2cbus, IODIRB, 0xFF);
                    SetPortPullups(0, 0x00);
                    SetPortPullups(1, 0x00);
                    InvertPort(0, 0x00);
                    InvertPort(1, 0x00);

                    // Set IsConnected to true and fire the Connected event handler
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
        ///     Set IO <paramref name="direction" /> for an individual pin.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="direction">true = input, false = output</param>
        public void SetPinDirection(byte pin, bool direction)
        {
            CheckConnected();

            pin = (byte) (pin - 1);

            if (pin < 8)
            {
                port_a_dir = helper.UpdateByte(port_a_dir, pin, direction);
                helper.WriteI2CByte(i2cbus, IODIRA, port_a_dir);
            }
            else if (pin >= 8 && pin < 16)
            {
                port_b_dir = helper.UpdateByte(port_b_dir, (byte) (pin - 8), direction);
                helper.WriteI2CByte(i2cbus, IODIRB, port_b_dir);
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
        public void SetPortDirection(byte port, byte direction)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, IODIRA, direction);
                    port_a_dir = direction;
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, IODIRB, direction);
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
        public void SetPinPullup(byte pin, bool value)
        {
            CheckConnected();

            pin = (byte) (pin - 1);

            if (pin < 8)
            {
                porta_pullup = helper.UpdateByte(porta_pullup, pin, value);
                helper.WriteI2CByte(i2cbus, GPPUA, porta_pullup);
            }
            else if (pin >= 8 && pin < 16)
            {
                portb_pullup = helper.UpdateByte(portb_pullup, (byte) (pin - 8), value);
                helper.WriteI2CByte(i2cbus, GPPUB, portb_pullup);
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
        public void SetPortPullups(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    porta_pullup = value;
                    helper.WriteI2CByte(i2cbus, GPPUA, value);
                    break;
                case 1:
                    portb_pullup = value;
                    helper.WriteI2CByte(i2cbus, GPPUB, value);
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
        public void WritePin(byte pin, bool value)
        {
            CheckConnected();

            pin = (byte) (pin - 1);
            if (pin < 8)
            {
                portaval = helper.UpdateByte(portaval, pin, value);
                helper.WriteI2CByte(i2cbus, GPIOA, portaval);
            }
            else if (pin >= 8 && pin < 16)
            {
                portbval = helper.UpdateByte(portbval, (byte) (pin - 8), value);
                helper.WriteI2CByte(i2cbus, GPIOB, portbval);
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
        public void WritePort(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, GPIOA, value);
                    portaval = value;
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, GPIOB, value);
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
        public bool ReadPin(byte pin)
        {
            CheckConnected();

            pin = (byte) (pin - 1);
            if (pin < 8)
            {
                portaval = helper.ReadI2CByte(i2cbus, GPIOA);
                return helper.CheckBit(portaval, pin);
            }
            if (pin >= 8 && pin < 16)
            {
                portbval = helper.ReadI2CByte(i2cbus, GPIOB);
                return helper.CheckBit(portbval, (byte) (pin - 8));
            }
            throw new ArgumentOutOfRangeException(nameof(pin));
        }

        /// <summary>
        ///     Read all pins on the selected <paramref name="port"/>.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <returns>returns number between 0 and 255 or 0x00 and 0xFF</returns>
        public byte ReadPort(byte port)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    portaval = helper.ReadI2CByte(i2cbus, GPIOA);
                    return portaval;
                case 1:
                    portbval = helper.ReadI2CByte(i2cbus, GPIOB);
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
        public void InvertPort(byte port, byte polarity)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, IPOLA, polarity);
                    porta_polarity = polarity;
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, IPOLB, polarity);
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
        public void InvertPin(byte pin, bool polarity)
        {
            CheckConnected();

            pin = (byte) (pin - 1);
            if (pin < 8)
            {
                porta_polarity = helper.UpdateByte(portaval, pin, polarity);
                helper.WriteI2CByte(i2cbus, IPOLA, porta_polarity);
            }
            else if (pin >= 8 && pin < 16)
            {
                portb_polarity = helper.UpdateByte(portbval, (byte) (pin - 8), polarity);
                helper.WriteI2CByte(i2cbus, IPOLB, portb_polarity);
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
        public void MirrorInterrupts(byte value)
        {
            CheckConnected();

            switch (value)
            {
                case 0:
                    config = helper.UpdateByte(config, 6, false);
                    helper.WriteI2CByte(i2cbus, IOCON, config);
                    break;
                case 1:
                    config = helper.UpdateByte(config, 6, true);
                    helper.WriteI2CByte(i2cbus, IOCON, config);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        /// <summary>
        ///     This sets the polarity of the INT output pins.
        /// </summary>
        /// <param name="value">1 = Active - high. 0 = Active - low.</param>
        public void SetInterruptPolarity(byte value)
        {
            CheckConnected();

            switch (value)
            {
                case 0:
                    config = helper.UpdateByte(config, 1, false);
                    helper.WriteI2CByte(i2cbus, IOCON, config);
                    break;
                case 1:
                    config = helper.UpdateByte(config, 1, true);
                    helper.WriteI2CByte(i2cbus, IOCON, config);
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
        public void SetInterruptType(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, INTCONA, value);
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, INTCONB, value);
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
        public void SetInterruptDefaults(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, DEFVALA, value);
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, DEFVALB, value);
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
        public void SetInterruptOnPort(byte port, byte value)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, GPINTENA, value);
                    intA = value;
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, GPINTENB, value);
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
        public void SetInterruptOnPin(byte pin, bool value)
        {
            CheckConnected();

            pin = (byte) (pin - 1);
            if (pin < 8)
            {
                intA = helper.UpdateByte(intA, pin, value);
                helper.WriteI2CByte(i2cbus, GPINTENA, intA);
            }
            else if (pin >= 8 && pin < 16)
            {
                intB = helper.UpdateByte(intB, (byte) (pin - 8), value);
                helper.WriteI2CByte(i2cbus, GPINTENB, intB);
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
        public byte ReadInterruptStatus(byte port)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    return helper.ReadI2CByte(i2cbus, INTFA);
                case 1:
                    return helper.ReadI2CByte(i2cbus, INTFB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Read the value from the selected <paramref name="port"/> at the time
        ///     of the last interrupt trigger.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        public byte ReadInterruptCapture(byte port)
        {
            CheckConnected();

            switch (port)
            {
                case 0:
                    return helper.ReadI2CByte(i2cbus, INTCAPA);
                case 1:
                    return helper.ReadI2CByte(i2cbus, INTCAPB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(port));
            }
        }

        /// <summary>
        ///     Reset the interrupts A and B to 0.
        /// </summary>
        public void ResetInterrupts()
        {
            CheckConnected();

            ReadInterruptCapture(0);
            ReadInterruptCapture(1);
        }

        private void CheckConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected. You must call .Connect() first.");
            }
        }

        /// <summary>
        ///     Dispose of the I2C device.
        /// </summary>
        public void Dispose()
        {
            i2cbus?.Dispose();
            i2cbus = null;

            IsConnected = false;
        }
    }
}