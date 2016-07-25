AB Electronics UK Windows 10 IOT Libraries and Demos
=====

Windows 10 IOT Library to work with Raspberry Pi expansion boards from https://www.abelectronics.co.uk

Installation instructions for the Windows 10 IOT release are available on https://ms-iot.github.io/content/en-US/GetStarted.htm

We have GitHub pages for this library and demos at http://abelectronicsuk.github.io/ABElectronics_Win10IOT_Libraries/

The AB Electronics Windows 10 IOT Library in the ABElectronics_Win10IOT_Libraries directory contains classes for the following Raspberry Pi expansion boards:

### ADC-DAC Pi ###
https://www.abelectronics.co.uk/products/3/Raspberry-Pi/39/ADC-DAC-Pi-Raspberry-Pi-ADC-and-DAC-expansion-board

### ADC Pi, ADC Pi Zero and ADC Pi Plus ###
https://www.abelectronics.co.uk/p/69/ADC-Pi-Zero-Raspberry-Pi-Analogue-to-Digital-converter and https://www.abelectronics.co.uk/p/56/ADC-Pi-Plus-Raspberry-Pi-Analogue-to-Digital-converter

### Delta Sigma Pi and ADC Differential Pi ###
https://www.abelectronics.co.uk/p/65/ADC-Differential-Pi-Raspberry-Pi-Analogue-to-Digital-converter

### IO Pi, IO Pi Zero and IO Pi Plus ###
https://www.abelectronics.co.uk/p/71/IO-Pi-Zero and https://www.abelectronics.co.uk/p/54/IO-Pi-Plus

### RTC Pi, RTC Pi Zero and RTC Pi Plus ###
https://www.abelectronics.co.uk/p/15/RTC-Pi 
https://www.abelectronics.co.uk/p/70/RTC-Pi-Zero
https://www.abelectronics.co.uk/p/52/RTC-Pi-Plus

### Servo Pi and Servo Pi Zero ###
https://www.abelectronics.co.uk/p/44/Servo-PWM-Pi and https://www.abelectronics.co.uk/p/72/Servo-PWM-Pi-Zero

## Demo and Test Applications ##

The "**DemoApplication**" directory contains a sample GUI application to connect to each of the supported boards and get and set data.

The "**Tests**" directory contains background task applications to use with the supported boards and the Raspberry Pi GPIO port.

The project/solution is written for Visual Studio 2015 running under Windows 10. 

The "\ABElectronics_Win10IOT_Libraries\bin\ARM\Release\ABElectronics_Win10IOT_Libraries.dll" dll will need to be built and included in your projects to use with the demo and test applications.