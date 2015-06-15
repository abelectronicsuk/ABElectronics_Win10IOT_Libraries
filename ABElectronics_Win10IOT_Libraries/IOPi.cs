using System;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using System.Threading.Tasks;

namespace ABElectronics_Win10IOT_Libraries
{
    /// <summary>
    /// Class for controlling the IO Pi and IO Pi Plus expansion boards from AB Electronics UK
    /// Based on the MCP23017 IO expander IC from Microchip
    /// </summary>
    public class IOPi
    {
        private ABE_Helpers helper = new ABE_Helpers();

        private I2cDevice i2cbus; // create an instance of the i2c bus

        /// <summary>
        /// I2C address for the IO Pi bus
        /// </summary>
        public byte Address { get; set; }

        /// <summary>
        /// Shows if there is a connection with the IO Pi
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            private set
            {
                isConnected = value;
            }
        }

        private bool isConnected;

        // Define registers values from datasheet
        private const byte IODIRA = 0x00;  // IO direction A - 1= input 0 = output
        private const byte IODIRB = 0x01;  // IO direction B - 1= input 0 = output       
        private const byte IPOLA = 0x02; // Input polarity A - If a bit is set, the corresponding GPIO register bit will reflect the inverted value on the pin.       
        private const byte IPOLB = 0x03; // Input polarity B - If a bit is set, the corresponding GPIO register bit will reflect the inverted value on the pin.       
        private const byte GPINTENA = 0x04; // The GPINTEN register controls the interrupt-on-change feature for each pin on port A.       
        private const byte GPINTENB = 0x05; // The GPINTEN register controls the interrupt-on-change feature for each pin on port B.        
        private const byte DEFVALA = 0x06; // Default value for port A - These bits set the compare value for pins configured for interrupt-on-change. If the associated pin level is the opposite from the register bit, an interrupt occurs.        
        private const byte DEFVALB = 0x07; // Default value for port B - These bits set the compare value for pins configured for interrupt-on-change. If the associated pin level is the opposite from the register bit, an interrupt occurs.        
        private const byte INTCONA = 0x08; // Interrupt control register for port A.  If 1 interrupt is fired when the pin matches the default value, if 0 the interrupt is fired on state change        
        private const byte INTCONB = 0x09; // Interrupt control register for port B.  If 1 interrupt is fired when the pin matches the default value, if 0 the interrupt is fired on state change
        private const byte IOCON = 0x0A;  // see datasheet for configuration register
        private const byte GPPUA = 0x0C;  // pull-up resistors for port A
        private const byte GPPUB = 0x0D;  // pull-up resistors for port B        
        private const byte INTFA = 0x0E; // The INTFA register reflects the interrupt condition on the port A pins of any pin that is enabled for interrupts. A set bit indicates that the associated pin caused the interrupt.        
        private const byte INTFB = 0x0F; // The INTFB register reflects the interrupt condition on the port B pins of any pin that is enabled for interrupts. A set bit indicates that the associated pin caused the interrupt.        
        private const byte INTCAPA = 0x10; // The INTCAP register captures the GPIO port A value at the time the interrupt occurred.        
        private const byte INTCAPB = 0x11; // The INTCAP register captures the GPIO port B value at the time the interrupt occurred.
        private const byte GPIOA = 0x12; // data port A
        private const byte GPIOB = 0x13;  // data port B
        private const byte OLATA = 0x14; // output latches A
        private const byte OLATB = 0x15; // output latches B

        // variables
        private byte port_a_dir = 0x00;  // port a direction
        private byte port_b_dir = 0x00;  // port b direction
        private byte portaval = 0x00;  // port a value
        private byte portbval = 0x00;  // port b value
        private byte porta_pullup = 0x00; // port a pull-up resistors
        private byte portb_pullup = 0x00; // port a pull-up resistors
        private byte porta_polarity = 0x00;  // input polarity for port a
        private byte portb_polarity = 0x00; // input polarity for port b
        private byte intA = 0x00; // interrupt control for port a
        private byte intB = 0x00; // interrupt control for port a
        
        private byte config = 0x22; // initial configuration - see IOCON page in the MCP23017 datasheet for more information.

        /// <summary>
        /// Instance of an IOPi bus
        /// </summary>
        /// <param name="i2caddress">I2C Address of IO Pi bus</param>
        public IOPi(byte i2caddress)
        {
            Address = i2caddress;
            IsConnected = false;
        }

        /// <summary>
        /// Open a connection with the IO Pi
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            /* Initialize the I2C bus */
            try
            {
                string aqs = I2cDevice.GetDeviceSelector(helper.I2C_CONTROLLER_NAME);  /* Find the selector string for the I2C bus controller                   */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
                var settings = new I2cConnectionSettings(Address);
                settings.BusSpeed = I2cBusSpeed.FastMode;

                i2cbus = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
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

                    EventHandler handler = Connected;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
                else
                {
                    IsConnected = false;
                }
                return;
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception e)
            {
                IsConnected = false;
                throw e;
            }
        }

        

        /// <summary>
        /// Event occurs when connection is made
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Set IO direction for an individual pin.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="direction">1 = input, 0 = output</param>

        public void SetPinDirection(byte pin, bool direction)
        {
            pin = (byte)(pin - 1);

            if (pin >= 0 && pin < 8)
            {
                port_a_dir = helper.UpdateByte(port_a_dir, pin, direction);
                helper.WriteI2CByte(i2cbus, IODIRA, port_a_dir);
            }
            else if (pin >= 8 && pin < 16)
            {
                port_b_dir = helper.UpdateByte(port_b_dir, (byte)(pin - 8), direction);
                helper.WriteI2CByte(i2cbus, IODIRB, port_b_dir);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Set the direction for an IO port.  You can control the direction of all 8 pins on a port by sending a single byte value.  Each bit in the byte represents one pin so for example 0x0A would set pins 2 and 4 to inputs and all other pins to outputs.
        /// </summary>
        /// <param name="direction">Direction for all pins on the port.  1 = input, 0 = output</param>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        public void SetPortDirection(byte port, byte direction)
        {
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
                    throw new NotSupportedException();
            }

        }
        /// <summary>
        /// Set the internal 100K pull-up resistors for an individual pin 
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="value">true = enabled, false = disabled</param>
        public void SetPinPullup(byte pin, bool value)
        {
            pin = (byte)(pin - 1);

            if (pin >= 0 && pin < 8)
            {
                porta_pullup = helper.UpdateByte(porta_pullup, pin, value);
                helper.WriteI2CByte(i2cbus, GPPUA, porta_pullup);
            }
            else if (pin >= 8 && pin < 16)
            {
                portb_pullup = helper.UpdateByte(portb_pullup, (byte)(pin - 8), value);
                helper.WriteI2CByte(i2cbus, GPPUB, portb_pullup);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// set the internal 100K pull-up resistors for the selected IO port
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void SetPortPullups(byte port, byte value)
        {
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
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// write to an individual pin 
        /// </summary>
        /// <param name="pin">1 - 16</param>
        /// <param name="value">0 = logic low, 1 = logic high</param>
        public void WritePin(byte pin, bool value)
        {
            pin = (byte)(pin - 1);
            if (pin >= 0 && pin < 8)
            {
                portaval = helper.UpdateByte(portaval, pin, value);
                helper.WriteI2CByte(i2cbus, GPIOA, portaval);
            }
            else if (pin >= 8 && pin < 16)
            {
                portbval = helper.UpdateByte(portbval, (byte)(pin - 8), value);
                helper.WriteI2CByte(i2cbus, GPIOB, portbval);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// write to all pins on the selected port.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void WritePort(byte port, byte value)
        {
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
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// read the value of an individual pin. 
        /// </summary>
        /// <param name="pin">1 - 16</param>
        /// <returns>0 = logic level low, 1 = logic level high</returns>
        public bool ReadPin(byte pin)
        {
            pin = (byte)(pin - 1);
            if (pin >= 0 && pin < 8)
            {
                portaval = helper.ReadI2CByte(i2cbus, GPIOA);
                return helper.CheckBit(portaval, pin);
            }
            else if (pin >= 8 && pin < 16)
            {
                portbval = helper.ReadI2CByte(i2cbus, GPIOB);
                return helper.CheckBit(portbval, (byte)(pin - 8));
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// read all pins on the selected port.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <returns>returns number between 0 and 255 or 0x00 and 0xFF</returns>
        public byte ReadPort(byte port)
        {
            switch (port)
            {
                case 0:
                    portaval = helper.ReadI2CByte(i2cbus, GPIOA);
                    return portaval;
                case 1:
                    portbval = helper.ReadI2CByte(i2cbus, GPIOB);
                    return portbval;
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// invert the polarity of the pins on a selected port.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="polarity">0x00 - 0xFF (0 = same logic state of the input pin, 1 = inverted logic state of the input pin)</param>
        public void InvertPort(byte port, byte polarity)
        {
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
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// invert the polarity of the selected pin.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="polarity">False = same logic state of the input pin, True = inverted logic state of the input pin</param>
        public void InvertPin(byte pin, bool polarity)
        {
            pin = (byte)(pin - 1);
            if (pin >= 0 && pin < 8)
            {
                porta_polarity = helper.UpdateByte(portaval, pin, polarity);
                helper.WriteI2CByte(i2cbus, IPOLA, porta_polarity);
            }
            else if (pin >= 8 && pin < 16)
            {
                portb_polarity = helper.UpdateByte(portbval, (byte)(pin - 8), polarity);
                helper.WriteI2CByte(i2cbus, IPOLB, portb_polarity);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Sets the mirror status of the interrupt pins.
        /// </summary>
        /// <param name="value">0 = The INT pins are not mirrored. INTA is associated with PortA and INTB is associated with PortB. 1 = The INT pins are internally connected</param>
        public void MirrorInterrupts(byte value)
        {
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
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// This sets the polarity of the INT output pins
        /// </summary>
        /// <param name="value">1 = Active - high. 0 = Active - low.</param>
        public void SetInterruptPolarity(byte value)
        {
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
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Sets the type of interrupt for each pin on the selected port. 1 = interrupt is fired when the pin matches the default value. 0 = the interrupt is fired on state change
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void SetInterruptType(byte port, byte value)
        {
            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, INTCONA, value);
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, INTCONB, value);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// These bits set the compare value for pins configured for interrupt-on-change on the selected port. If the associated pin level is the opposite from the register bit, an interrupt occurs.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void SetInterruptDefaults(byte port, byte value)
        {
            switch (port)
            {
                case 0:
                    helper.WriteI2CByte(i2cbus, DEFVALA, value);
                    break;
                case 1:
                    helper.WriteI2CByte(i2cbus, DEFVALB, value);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Enable interrupts for the pins on the selected port.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        /// <param name="value">number between 0 and 255 or 0x00 and 0xFF</param>
        public void SetInterruptOnPort(byte port, byte value)
        {
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
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Enable interrupts for the selected pin.
        /// </summary>
        /// <param name="pin">1 to 16</param>
        /// <param name="value">0 = interrupt disabled, 1 = interrupt enabled</param>
        public void SetInterruptOnPin(byte pin, bool value)
        {

            pin = (byte)(pin - 1);
            if (pin >= 0 && pin < 8)
            {
                intA = helper.UpdateByte(intA, pin, value);
                helper.WriteI2CByte(i2cbus, GPINTENA, intA);
            }
            else if (pin >= 8 && pin < 16)
            {
                intB = helper.UpdateByte(intB, (byte)(pin - 8), value);
                helper.WriteI2CByte(i2cbus, GPINTENB, intB);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// read the interrupt status for the pins on the selected port.
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        public byte ReadInterruptStatus(byte port)
        {
            switch (port)
            {
                case 0:
                    return helper.ReadI2CByte(i2cbus, INTFA);
                case 1:
                    return helper.ReadI2CByte(i2cbus, INTFB);
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// read the value from the selected port at the time of the last interrupt trigger. 
        /// </summary>
        /// <param name="port">0 = pins 1 to 8, 1 = pins 9 to 16</param>
        public byte ReadInterruptCapture(byte port)
        {
            switch (port)
            {
                case 0:
                    return helper.ReadI2CByte(i2cbus, INTCAPA);
                case 1:
                    return helper.ReadI2CByte(i2cbus, INTCAPB);
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Reset the interrupts A and B to 0
        /// </summary>
        public void ResetInterrupts()
        {
            ReadInterruptCapture(0);
            ReadInterruptCapture(1);
        }


        /// <summary>
        /// Dispose of the I2C device
        /// </summary>
        public void Dispose()
        {
            i2cbus.Dispose();
            IsConnected = false;
        }
    }
}
