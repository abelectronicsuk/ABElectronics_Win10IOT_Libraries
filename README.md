# AB Electronics UK Windows 10 IOT Libraries and Demos #

This is the Windows 10 IOT Library to work with Raspberry Pi expansion boards from <https://www.abelectronics.co.uk>.

The AB Electronics Windows 10 IOT Library in the ABElectronics_Win10IOT_Libraries directory contains classes for the following Raspberry Pi expansion boards:

**This library is not compatible with the direct memory mapped driver (DMAP).**

## Installing The Library ##

To install ABElectronics-Windows10-IOT, run the following command in the Package Manager Console 
``` powershell
Install-Package ABElectronics_Win10IOT_Libraries 
```
The library is available from [NuGet](https://www.nuget.org/packages/ABElectronics_Win10IOT_Libraries/).

## Boards ##
### ADC-DAC Pi ###
<https://www.abelectronics.co.uk/products/3/Raspberry-Pi/39/ADC-DAC-Pi-Raspberry-Pi-ADC-and-DAC-expansion-board>

### ADC Pi, ADC Pi Zero and ADC Pi Plus ###
- RP Pi Zero: <https://www.abelectronics.co.uk/p/69/ADC-Pi-Zero-Raspberry-Pi-Analogue-to-Digital-converter>*
- RP A+, B+, 2 & 3: <https://www.abelectronics.co.uk/p/56/ADC-Pi-Plus-Raspberry-Pi-Analogue-to-Digital-converter>

### Delta Sigma Pi and ADC Differential Pi ###
- RP A+, B+, 2 & 3: <https://www.abelectronics.co.uk/p/65/ADC-Differential-Pi-Raspberry-Pi-Analogue-to-Digital-converter>

### Expander Pi ###
- RP Zero, A+, B+, 2 & 3: <https://www.abelectronics.co.uk/p/50/Expander-Pi>

### IO Pi, IO Pi Zero and IO Pi Plus ###
- RP Pi Zero: <https://www.abelectronics.co.uk/p/71/IO-Pi-Zero>*
- RP A+, B+, 2 & 3: <https://www.abelectronics.co.uk/p/54/IO-Pi-Plus>

### RTC Pi, RTC Pi Zero and RTC Pi Plus ###
- RP Pi Zero: <https://www.abelectronics.co.uk/p/70/RTC-Pi-Zero>*
- RP A+, B+, 2 & 3: <https://www.abelectronics.co.uk/p/52/RTC-Pi-Plus>

### Servo Pi Zero and Servo Pi ###
- RP Pi Zero: <https://www.abelectronics.co.uk/p/72/Servo-PWM-Pi-Zero>*
- RP A+, B+, 2 & 3: <https://www.abelectronics.co.uk/p/44/Servo-PWM-Pi>

> \* Note: Windows 10 IoT won't work on Pi Zero because it's ARMv6. ARMv7+ is required for Windows.

## Demo Applications ##

- The "**DemoApplication**" directory contains a sample GUI application to connect to each of the supported boards and get and set data.
-- The project/solution is written for Visual Studio 2015 running under Windows 10.

If you are not using the Nuget version, you'll need to include the following dll file as a reference in your project:
`ABElectronics_Win10IOT_Libraries\bin\ARM\Release\ABElectronics_Win10IOT_Libraries.dll`.
