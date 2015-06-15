AB Electronics Windows 10 IOT Libraries
=====

Windows 10 IOT Library to use with Raspberry Pi expansion boards from http://www.abelectronics.co.uk

Installation instructions for the Windows 10 IOT release are available on https://ms-iot.github.io/content/en-US/GetStarted.htm

We have GitHub pages for this library with individual pages for each class and expansion board  and demo applications at http://abelectronicsuk.github.io/ABElectronics_Win10IOT_Libraries/

The AB Electronics Windows 10 IOT Library contains the following classes:

##ADCDACPi
This class contains methods for use with the ADC DAC Pi from  https://www.abelectronics.co.uk/products/3/Raspberry-Pi/39/ADC-DAC-Pi-Raspberry-Pi-ADC-and-DAC-expansion-board

###Methods:

```
ReadADCVoltage(byte channel)
```
Read the voltage from the selected channel on the ADC  
**Parameters:** channel - 1 or 2  
**Returns:** number as float between 0 and 2.048

```
ReadADCRaw(byte channel)
```
Read the raw value from the selected channel on the ADC  
**Parameters:** channel - 1 or 2  
**Returns:** int
```
SetADCrefVoltage(double voltage) 
```
Set the reference voltage for the analogue to digital converter.  
The ADC uses the raspberry pi 3.3V power as a voltage reference so using this method to set the reference to match the exact output voltage from the 3.3V regulator will increase the accuracy of the ADC readings.  
**Parameters:** voltage - double between 0.0 and 7.0  
**Returns:** null

```
SetDACVoltage(byte channel, double voltage)
```
Set the voltage for the selected channel on the DAC  
**Parameters:** channel - 1 or 2,  voltage can be between 0 and 2.047 volts  
**Returns:** null 

```
SetDACRaw(byte channel, int value)
```
Set the raw value from the selected channel on the DAC  
**Parameters:** channel - 1 or 2,value int between 0 and 4095  
**Returns:** null 
###Usage

To use the ADCDACPi library in your code you must first import the library dll:
```
using ABElectronics_Win10IOT_Libraries;
```
Next you must initialise the ADCDACPi class:
```
ABElectronics_Win10IOT_Libraries.ADCDACPi adcdac = new ABElectronics_Win10IOT_Libraries.ADCDACPi();
```
Next we need to connect to the device and wait for the connection
```
adcdac.Connect();

while (!adcdac.IsConnected)
{
}

```
Set the reference voltage.
```
adcdac.SetADCrefVoltage(3.3);
```
Read the voltage from channel 2
```
double value = adcdac.ReadADCVoltage(2);
```

Set the DAC voltage on channel 2 to 1.5 volts
```
adcdac.SetDACVoltage(2, 1.5);
```

##ADCPi 
This class contains methods for use with the  ADC Pi from  http://www.abelectronics.co.uk/products/3/Raspberry-Pi/17/ADC-Pi-V2---Raspberry-Pi-Analogue-to-Digital-converter

###Methods:
```
Connect() 
```
Connect to the I2C device  
**Parameters:** none
**Returns:** null

```
IsConnected() 
```
Check if the device is connected
**Parameters:** none
**Returns:** boolean
```
Dispose() 
```
Dispose of the active I2C device  
**Parameters:** none
**Returns:** null
```
ReadVoltage(byte channel)
```
Read the voltage from the selected channel  
**Parameters:** channel as int - 1 to 8 
**Returns:** number as double between 0 and 5.0

```
ReadRaw(byte channel)
```
Read the raw int value from the selected channel  
**Parameters:** channel as int - 1 to 8 
**Returns:** raw integer value from ADC buffer

```
SetPGA(byte gain)
```
Set the gain of the PDA on the chip  
**Parameters:** gain as int -  1, 2, 4, 8  
**Returns:** null

```
SetBitRate(byte rate)
```
Set the sample bit rate of the adc  
**Parameters:** rate as int -  12, 14, 16, 18  
**Returns:** null  
12 = 12 bit (240SPS max)  
14 = 14 bit (60SPS max)  
16 = 16 bit (15SPS max)  
18 = 18 bit (3.75SPS max)  

```
SetConversionMode(bool mode)
```
Set the conversion mode for the adc  
**Parameters:** mode as boolean -  false = One-shot conversion, true = Continuous conversion  
**Returns:** null

###Usage

To use the ADC Pi library in your code you must first import the library dll:
```
using ABElectronics_Win10IOT_Libraries;
```
Next you must initialise the adc class:
```
ABElectronics_Win10IOT_Libraries.ADCPi adc = new ADCPi(0x68, 0x69);
```
The arguments are the two I2C addresses of the ADC chips. The values shown are the default addresses of the ADC board.

Next we need to connect to the device and wait for the connection before setting the bit rate and gain. The sample rate can be 12, 14, 16 or 18
```
adc.Connect();

while (!adc.IsConnected)
{
}
adc.SetBitRate(18);
adc.SetPGA(1);

```

You can now read the voltage from channel 1 with:
```
double readvalue = 0;

readvalue = adc.ReadVoltage(1);
```


##DeltaSigmaPi
This class contains methods for use with the Delta Sigma Pi from http://www.abelectronics.co.uk/products/3/Raspberry-Pi/14/Delta-Sigma-Pi-18-bit-Analogue-to-Digital-converter

###Methods:
```
Connect() 
```
Connect to the I2C device  
**Parameters:** none
**Returns:** null

```
IsConnected() 
```
Check if the device is connected
**Parameters:** none
**Returns:** boolean

```
Dispose() 
```
Dispose of the active I2C device  
**Parameters:** none
**Returns:** null

```
ReadVoltage(byte channel)
```
Read the voltage from the selected channel  
**Parameters:** channel as int - 1 to 8 
**Returns:** number as double between 0 and 5.0

```
ReadRaw(byte channel)
```
Read the raw int value from the selected channel  
**Parameters:** channel as int - 1 to 8 
**Returns:** raw integer value from ADC buffer

```
SetPGA(byte gain)
```
Set the gain of the PDA on the chip  
**Parameters:** gain as int -  1, 2, 4, 8  
**Returns:** null

```
SetBitRate(byte rate)
```
Set the sample bit rate of the adc  
**Parameters:** rate as int -  12, 14, 16, 18  
**Returns:** null  
12 = 12 bit (240SPS max)  
14 = 14 bit (60SPS max)  
16 = 16 bit (15SPS max)  
18 = 18 bit (3.75SPS max)  

```
SetConversionMode(bool mode)
```
Set the conversion mode for the adc  
**Parameters:** mode as boolean -  false = One-shot conversion, true = Continuous conversion  
**Returns:** null

###Usage

To use the Delta Sigma Pi library in your code you must first import the library dll:
```
using ABElectronics_Win10IOT_Libraries;
```
Next you must initialise the adc class:
```
ABElectronics_Win10IOT_Libraries.DeltaSigmaPi adc = new DeltaSigmaPi(0x68, 0x69);
```
The arguments are the two I2C addresses of the ADC chips. The values shown are the default addresses of the Delta Sigma Pi board.

Next we need to connect to the device and wait for the connection before setting the bit rate and gain. The sample rate can be 12, 14, 16 or 18
```
adc.Connect();

while (!adc.IsConnected)
{
}
adc.SetBitRate(18);
adc.SetPGA(1);

```

You can now read the voltage from channel 1 with:
```
double readvalue = 0;

readvalue = adc.ReadVoltage(1);
```

##IOPi
This class contains methods for use with the IO Pi from http://www.abelectronics.co.uk/products/3/Raspberry-Pi/18/IO-Pi-32-Channel-Port-Expander-for-the-Raspberry-Pi

###Methods:
```
Connect() 
```
Connect to the I2C device  
**Parameters:** none
**Returns:** null

```
IsConnected() 
```
Check if the device is connected
**Parameters:** none
**Returns:** boolean

```
Dispose() 
```
Dispose of the active I2C device  
**Parameters:** none
**Returns:** null

```
SetPinDirection(byte pin, bool direction)
```
Sets the IO direction for an individual pin  
**Parameters:** pin - 1 to 16, direction - true = input, false = output  
**Returns:** null

```
SetPortDirection(byte port, byte direction)
```
Sets the IO direction for the specified IO port  
**Parameters:** port - 0 = pins 1 to 8, port 1 = pins 8 to 16, direction - true = input, false = output  
**Returns:** null

```
SetPinPullup(byte pin, bool value)
```
Set the internal 100K pull-up resistors for the selected IO pin  
**Parameters:** pin - 1 to 16, value: true = Enabled, false = Disabled  
**Returns:** null
```
SetPortPullups(byte port, byte value)
```
Set the internal 100K pull-up resistors for the selected IO port  
**Parameters:** 0 = pins 1 to 8, 1 = pins 9 to 16, value: true = Enabled, false = Disabled   
**Returns:** null

```
WritePin(byte pin, bool value)
```
Write to an individual pin 1 - 16  
**Parameters:** pin - 1 to 16, value - true = Enabled, false = Disabled
**Returns:** null
```
WritePort(byte port, byte value)
```
Write to all pins on the selected port  
**Parameters:** port - 0 = pins 1 to 8, port 1 = pins 8 to 16, value -  number between 0 and 255 or 0x00 and 0xFF  
**Returns:** null
```
ReadPin(byte pin)
```
Read the value of an individual pin 1 - 16   
**Parameters:** pin: 1 to 16  
**Returns:** false = logic level low, true = logic level high
```
ReadPort(byte port)
```
Read all pins on the selected port  
**Parameters:** port - 0 = pins 1 to 8, port 1 = pins 8 to 16  
**Returns:** number between 0 and 255 or 0x00 and 0xFF
```
InvertPort(byte port, byte polarity)
```
Invert the polarity of the pins on a selected port  
**Parameters:** port - 0 = pins 1 to 8, port 1 = pins 8 to 16, polarity - 0 = same logic state of the input pin, 1 = inverted logic state of the input pin  
**Returns:** null

```
InvertPin(byte pin, bool polarity)
```
Invert the polarity of the selected pin  
**Parameters:** pin - 1 to 16, polarity - false = same logic state of the input pin, true = inverted logic state of the input pin
**Returns:** null
```
MirrorInterrupts(byte value)
```
Mirror Interrupts  
**Parameters:** value - 1 = The INT pins are internally connected, 0 = The INT pins are not connected. INTA is associated with PortA and INTB is associated with PortB  
**Returns:** null

```
SetInterruptPolarity(byte value)
```
This sets the polarity of the INT output pins
**Parameters:** 1 = Active - high. 0 = Active - low.
**Returns:** null

```
SetInterruptType(byte port, byte value)
```
Sets the type of interrupt for each pin on the selected port  
**Parameters:** port 0 = pins 1 to 8, port 1 = pins 8 to 16, value: 1 = interrupt is fired when the pin matches the default value, 0 = the interrupt is fired on state change  
**Returns:** null
```
SetInterruptDefaults(byte port, byte value)
```
These bits set the compare value for pins configured for interrupt-on-change on the selected port.  
If the associated pin level is the opposite from the register bit, an interrupt occurs.    
**Parameters:** port 0 = pins 1 to 8, port 1 = pins 8 to 16, value: compare value  
**Returns:** null
```
SetInterruptOnPort(byte port, byte value)
```
Enable interrupts for the pins on the selected port  
**Parameters:** port 0 = pins 1 to 8, port 1 = pins 8 to 16, value: number between 0 and 255 or 0x00 and 0xFF  
**Returns:** null

```
SetInterruptOnPin(byte pin, bool value)
```
Enable interrupts for the selected pin  
**Parameters:** pin - 1 to 16, value - true = interrupt disabled, false = interrupt enabled  
**Returns:** null

```
ReadInterruptStatus(byte port)
```
Enable interrupts for the selected pin  
**Parameters:** port 0 = pins 1 to 8, port 1 = pins 8 to 16  
**Returns:** status

```
ReadInterruptCapture(byte port)
```
Read the value from the selected port at the time of the last interrupt trigger  
**Parameters:** port 0 = pins 1 to 8, port 1 = pins 8 to 16  
**Returns:** status
```
ResetInterrupts()
```
Set the interrupts A and B to 0  
**Parameters:** null  
**Returns:** null
###Usage

To use the IO Pi library in your code you must first import the library dll:
```
using ABElectronics_Win10IOT_Libraries;
```
Next you must initialise the io class:
```
ABElectronics_Win10IOT_Libraries.IOPi bus1 = new ABElectronics_Win10IOT_Libraries.IOPi(0x20);
```
The argument is the I2C addresses of the IO chip. The value shown are the default addresses of the IO board which are 0x20 and 0x21.

Next we need to connect to the device and wait for the connection before setting ports to be inputs
```
bus1.Connect();

while (!bus1.IsConnected)
{
}
bus1.SetPortDirection(0, 0xFF);
bus1.SetPortDirection(1, 0xFF);

```

You can now read the input status from channel 1 with:
```
bool value = bus1.ReadPin(1);
```

##RTCPi
This class contains methods for use with the RTC Pi from https://www.abelectronics.co.uk/products/3/Raspberry-Pi/15/RTC-Pi

###Methods:
```
Connect() 
```
Connect to the I2C device  
**Parameters:** none
**Returns:** null

```
IsConnected() 
```
Check if the device is connected
**Parameters:** none
**Returns:** boolean

```
Dispose() 
```
Dispose of the active I2C device  
**Parameters:** none
**Returns:** null

```
SetDate(DateTime date)
```
Set the date and time on the RTC   
**Parameters:** date as DateTime
**Returns:** null

```
ReadDate() 
```
Returns the date from the RTC in ISO 8601 format - YYYY-MM-DDTHH:MM:SS   
**Returns:** date as DateTime


```
EnableOutput() 
```
Enable the square-wave output on the SQW pin.  
**Returns:** null

```
DisableOutput()
```
Disable the square-wave output on the SQW pin.   
**Returns:** null

```
SetFrequency(byte frequency)
```
Set the frequency for the square-wave output on the SQW pin.   
**Parameters:** frequency - options are: 1 = 1Hz, 2 = 4.096KHz, 3 = 8.192KHz, 4 = 32.768KHz   
**Returns:** null

###Usage

To use the RTC Pi library in your code you must first import the library dll:
```
using ABElectronics_Win10IOT_Libraries;
```
Next you must initialise the rtc class:
```
ABElectronics_Win10IOT_Libraries.RTCPi rtc = new ABElectronics_Win10IOT_Libraries.RTCPi();
```
Next we need to connect to the device and wait for the connection
```
rtc.Connect();

while (!rtc.IsConnected)
{
}

```
You can set the date and time from the RTC chip to be the 25th December 2015 at 6 AM with:
```
DateTime newdate = new DateTime(2015, 12, 25, 06, 00, 00);
rtc.SetDate(newdate);
```

You can read the date and time from the RTC chip with:
```
DateTime value = rtc.ReadDate();
```


##ServoPi
This class contains methods for use with the ServoPi from http://www.abelectronics.co.uk/products/3/Raspberry-Pi/44/Servo-Pi---PWM-Controller

###Methods:
```
Connect() 
```
Connect to the I2C device  
**Parameters:** none
**Returns:** null

```
IsConnected() 
```
Check if the device is connected
**Parameters:** none
**Returns:** boolean

```
Dispose() 
```
Dispose of the active I2C device  
**Parameters:** none
**Returns:** null

```
SetPwmFreq(freq) 
```
Set the PWM frequency
**Parameters:** freq - required frequency  
**Returns:** null

```
SetPwm(channel, on, off) 
```
Set the output on single channels
**Parameters:** channel - 1 to 16, on - time period, off - time period
**Returns:** null


```
SetAllPwm( on, off) 
```
Set the output on all channels
**Parameters:** on - time period, off - time period
**Returns:** null

```
byte OutputEnablePin { get; set; }
```
**Parameters:** Set the GPIO pin for the output enable function.
**Returns:** null
**Notes:** The default GPIO pin 4 is not supported in Windows 10 IOT so the OE pad will need to be connected to a different GPIO pin.


```
OutputDisable()
```
Disable the output via OE pin
**Parameters:** null
**Returns:** null

```
OutputEnable()
```
Enable the output via OE pin
**Parameters:** null
**Returns:** null

###Usage

To use the ServoPi Pi library in your code you must first import the library dll:
```
using ABElectronics_Win10IOT_Libraries;
```
Next you must initialise the ServoPi class:
```
ABElectronics_Win10IOT_Libraries.ServoPi servo = new ABElectronics_Win10IOT_Libraries.ServoPi(0x40);
```
The argument is the I2C addresses of the Servo Pi chip.

Next we need to connect to the device and wait for the connection
```
servo.Connect();

while (!servo.IsConnected)
{
}

```
Set PWM frequency to 60 Hz and enable the output
```
servo.SetPWMFreqency(60);                       

```
**Optional**
You can set the enable pin to use the output enable functions and the enable and disable the output. 
The default GPIO pin 4 is not supported in Windows 10 IOT and so the OE pad will need to be connected to a different GPIO pin to use this functionality.

```
servo.OutputEnablePin(17); // set to GPIO pin 17
servo.OutputEnable();
```
```
Move the servo to a position and exit the application.
```
servo.SetPWM(1, 0, 300);

``` 
