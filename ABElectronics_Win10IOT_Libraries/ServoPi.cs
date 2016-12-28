using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Foundation.Metadata;

namespace ABElectronics_Win10IOT_Libraries
{
	/// <summary>
	///     Class for controlling the Servo Pi expansion board from AB Electronics UK
	///     Based on the PCA9685 PWM controller IC from NXT.
	/// </summary>
	public class ServoPi
	{
		private readonly byte ALL_LED_OFF_H = 0xFD;
		private readonly byte ALL_LED_OFF_L = 0xFC;
		private readonly byte ALL_LED_ON_H = 0xFB;
		private readonly byte ALL_LED_ON_L = 0xFA;
		private byte ALLCALLADR = 0x05;
		private GpioController gpio;
		private readonly ABE_Helpers helper = new ABE_Helpers();

		// create an instance of the i2c bus and GPIO controller
		private I2cDevice i2cbus;
		private readonly byte LED0_OFF_H = 0x09;
		private readonly byte LED0_OFF_L = 0x08;
		private readonly byte LED0_ON_H = 0x07;
		private readonly byte LED0_ON_L = 0x06;

		// Define registers values from the datasheet
		private readonly byte MODE1 = 0x00;
		private byte MODE2 = 0x01;
		private GpioPin pin;
		private readonly byte PRE_SCALE = 0xFE;
		private byte SUBADR1 = 0x02;
		private byte SUBADR2 = 0x03;
		private byte SUBADR3 = 0x04;

		/// <summary>
		///     Create an instance of a Servo Pi bus.
		/// </summary>
		/// <param name="address">I2C address of Servo Pi bus</param>
		/// <example>ABElectronics_Win10IOT_Libraries.ServoPi servo = new ABElectronics_Win10IOT_Libraries.ServoPi();</example>
		public ServoPi(byte address = 0x40)
		{
			Address = address;
			IsConnected = false;
			OutputEnablePin = 0;
		}


		/// <summary>
		///     I2C address for the Servo Pi bus.
		/// </summary>
		/// <example>servopi.Address = 0x40;</example>
		public byte Address { get; set; }

		/// <summary>
		///     Set the GPIO pin for the output enable function.
		///     The default GPIO pin 4 is not supported in Windows 10 IOT so the OE pad will need to be connected to a different
		///     GPIO pin.
		/// </summary>
		/// <example>servopi.OutputEnablePin = 17;</example>
		public byte OutputEnablePin { get; set; }

		/// <summary>
		///     Shows if there is a connection with the Servo Pi
		/// </summary>
		/// <example>if (servopi.IsConnected) { }</example>
		public bool IsConnected { get; private set; }

		/// <summary>
		///     Open a connection with the Servo Pi
		/// </summary>
		/// <returns></returns>
		/// <example>servopi.Connect();</example>
		public async Task Connect()
		{
			IsConnected = false;

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

				i2cbus = await I2cDevice.FromIdAsync(dis[0].Id, settings);
					/* Create an I2cDevice with our selected bus controller and I2C settings */
				if (i2cbus != null)
				{
					// Connection is established so set IsConnected to true

					IsConnected = true;

					helper.WriteI2CByte(i2cbus, MODE1, 0x00);

					// Check to see if the output pin has been set and if so try to connect to the GPIO pin on the Raspberry Pi
					if (OutputEnablePin != 0)
					{
						gpio = GpioController.GetDefault();

						if (gpio != null)
						{
							GpioOpenStatus status;

							gpio.TryOpenPin(OutputEnablePin, GpioSharingMode.Exclusive, out pin, out status);
							if (status == GpioOpenStatus.PinOpened)
							{
								// Latch HIGH value first. This ensures a default value when the pin is set as output
								pin.Write(GpioPinValue.High);
								// Set the IO direction as output
								pin.SetDriveMode(GpioPinDriveMode.Output);
							}
						}
					}

					// Fire the Connected event handler
					Connected?.Invoke(this, EventArgs.Empty);
				}
				else
				{
					IsConnected = false;
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
		///     Set the output frequency of all PWM channels.
		///     The output frequency is programmable from a typical 40Hz to 1000Hz.
		/// </summary>
		/// <param name="freq">Integer frequency value</param>
		/// <example>servopi.SetPWMFreqency(500);</example>
		public void SetPWMFreqency(int freq)
		{
			var scaleval = 25000000.0; // 25MHz
			scaleval /= 4096.0; // 12-bit
			scaleval /= freq;
			scaleval -= 1.0;
			var prescale = Math.Floor(scaleval + 0.5);
			var oldmode = helper.ReadI2CByte(i2cbus, MODE1);
			var newmode = (byte) ((oldmode & 0x7F) | 0x10);
			helper.WriteI2CByte(i2cbus, MODE1, newmode);
			helper.WriteI2CByte(i2cbus, PRE_SCALE, (byte) Math.Floor(prescale));
			helper.WriteI2CByte(i2cbus, MODE1, oldmode);
			helper.WriteI2CByte(i2cbus, MODE1, (byte) (oldmode | 0x80));
		}

		/// <summary>
		///     Set the PWM output on a single <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">1 to 16</param>
		/// <param name="on">Value between 0 and 4096</param>
		/// <param name="off">Value between 0 and 4096</param>
		/// <example>servopi.SetPWM(1,512,1024);</example>
		public void SetPWM(byte channel, short on, short off)
		{
			channel = (byte) (channel - 1);
			helper.WriteI2CByte(i2cbus, (byte) (LED0_ON_L + 4 * channel), (byte) (on & 0xFF));
			helper.WriteI2CByte(i2cbus, (byte) (LED0_ON_H + 4 * channel), (byte) (on >> 8));
			helper.WriteI2CByte(i2cbus, (byte) (LED0_OFF_L + 4 * channel), (byte) (off & 0xFF));
			helper.WriteI2CByte(i2cbus, (byte) (LED0_OFF_H + 4 * channel), (byte) (off >> 8));
		}

		/// <summary>
		///     Set PWM output on all channels.
		/// </summary>
		/// <param name="on">Value between 0 and 4096</param>
		/// <param name="off">Value between 0 and 4096</param>
		/// <example>servopi.SetAllPWM(512,1024);</example>
		public void SetAllPWM(short on, short off)
		{
			helper.WriteI2CByte(i2cbus, ALL_LED_ON_L, (byte) (on & 0xFF));
			helper.WriteI2CByte(i2cbus, ALL_LED_ON_H, (byte) (on >> 8));
			helper.WriteI2CByte(i2cbus, ALL_LED_OFF_L, (byte) (off & 0xFF));
			helper.WriteI2CByte(i2cbus, ALL_LED_OFF_H, (byte) (off >> 8));
		}

		/// <summary>
		///     Disable output via OE pin.  Only used when the OE jumper is joined.
		/// </summary>
		/// <example>servopi.OutputDisable();</example>
		public void OutputDisable()
		{
			if (pin != null)
			{
				pin.Write(GpioPinValue.High);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		///     Enable output via OE pin.  Only used when the OE jumper is joined.
		/// </summary>
		/// <example>servopi.OutputEnable();</example>
		public void OutputEnable()
		{
			if (pin != null)
			{
				pin.Write(GpioPinValue.Low);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		///     Dispose of the Servo Pi device.
		/// </summary>
		/// <example>servopi.Dispose();</example>
		public void Dispose()
		{
			i2cbus.Dispose();
			IsConnected = false;
		}
	}
}