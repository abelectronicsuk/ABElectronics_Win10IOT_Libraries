Imports System.Threading.Tasks
Imports System.Threading

Public NotInheritable Class ExpanderPi
    Inherits Page


    Public expi As New ABElectronics_Win10IOT_Libraries.ExpanderPi()

    ' create two timers.  _timer 1 reads from the IO and ADC.  _timer2 updates the date from the RTC at 1 second intervals
    Private TIMER1_INTERVAL_IN_MILLISECONDS As Integer = 200
    Private TIMER2_INTERVAL_IN_MILLISECONDS As Integer = 1000
    Private _timer1 As Timer
    Private _timer2 As Timer

    ' used to set the expander pi direction.  true = read, false = write.
    Private IO_Direction As Boolean = True

    Public Sub New()
        Me.InitializeComponent()

        ' initialise the timers with the preset period
        _timer1 = New Timer(AddressOf Timer1_Tick, Nothing, TIMER1_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        _timer2 = New Timer(AddressOf Timer2_Tick, Nothing, TIMER2_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub

    Private Async Sub bt_expi_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' when the connect button is clicked check that the RTC Pi is not already connected before creating a Connected event handler and connecting to the RTC Pi
        If Not expi.IsConnected Then
            AddHandler expi.Connected, AddressOf expi_Connected
            Await expi.Connect()
        End If
    End Sub

    Private Sub bt_Back_Clicked(sender As Object, e As RoutedEventArgs)
        ' dispose of the expander pi and go back to the main page
        expi.Dispose()
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame.Navigate(GetType(MainPage))
    End Sub


    Private Sub expi_Connected(sender As Object, e As EventArgs)
        ' expander pi 1 is connected so update the message box, read from the expander pi and start the timer
        WriteMessage("expander pi Connected")

        ' set the ADC reference voltage to 4.096V
        expi.ADCSetRefVoltage(4.096)

        radio_IO_Read.IsChecked = True
        radio_DACGain1.IsChecked = True

        RefeshDisplay()
        _timer1.Change(TIMER1_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        _timer2.Change(TIMER2_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub

    Private Sub Timer1_Tick(state As [Object])
        ' on timer tick check if the expander pi is connected and read from the expander pi before resetting the timer
        If expi.IsConnected Then
            RefeshDisplay()
            _timer1.Change(TIMER1_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        End If
    End Sub

    Private Async Sub Timer2_Tick(state As [Object])
        ' on timer 2 tick check if the expander pi is connected and read from the expander pi before resetting the timer
        If expi.IsConnected Then
            Try
                ' read the current date and time from the RTC Pi into a DateTime object
                Dim [date] As DateTime = expi.RTCReadDate()

                ' invoke a dispatcher to update the date textbox on the page
                Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                               txt_ClockOut.Text = [date].ToString("d MMMM yyyy hh:mm:ss tt")

                                                                                                                                                           End Function)
            Catch
                Return
            End Try
            _timer2.Change(TIMER2_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        End If
    End Sub

    Private Function GetBit(b As Byte, bitNumber As Integer) As Boolean
        Return (b And (1 << bitNumber)) <> 0
    End Function

    Private Async Sub WriteMessage(message As String)
        ' this method updates the Message textbox on the page
        Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                       txt_Message.Text = message

                                                                                                                                                   End Function)
    End Sub

    Private Async Sub RefeshDisplay()
        ' check that the expander pi is connected
        If expi.IsConnected Then
            Try
                ' invoke the dispatcher to update the checkboxes for expander pi IO ports with the values read from each pin
                Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                               ' check if the IO Pi is in read mode

                                                                                                                                                               If radio_IO_Read.IsChecked = True Then

                                                                                                                                                                   ' read the values from ports 0 and 1 into two byte variables and check 
                                                                                                                                                                   ' the status of each variable to find the pin status
                                                                                                                                                                   ' this is faster than reading each pin individually
                                                                                                                                                                   Dim port0 As Byte = expi.IOReadPort(0)
                                                                                                                                                                   Dim port1 As Byte = expi.IOReadPort(1)

                                                                                                                                                                   chk_IO_Channel_01.IsChecked = GetBit(port0, 0)
                                                                                                                                                                   chk_IO_Channel_02.IsChecked = GetBit(port0, 1)
                                                                                                                                                                   chk_IO_Channel_03.IsChecked = GetBit(port0, 2)
                                                                                                                                                                   chk_IO_Channel_04.IsChecked = GetBit(port0, 3)
                                                                                                                                                                   chk_IO_Channel_05.IsChecked = GetBit(port0, 4)
                                                                                                                                                                   chk_IO_Channel_06.IsChecked = GetBit(port0, 5)
                                                                                                                                                                   chk_IO_Channel_07.IsChecked = GetBit(port0, 6)
                                                                                                                                                                   chk_IO_Channel_08.IsChecked = GetBit(port0, 7)
                                                                                                                                                                   chk_IO_Channel_09.IsChecked = GetBit(port1, 0)
                                                                                                                                                                   chk_IO_Channel_10.IsChecked = GetBit(port1, 1)
                                                                                                                                                                   chk_IO_Channel_11.IsChecked = GetBit(port1, 2)
                                                                                                                                                                   chk_IO_Channel_12.IsChecked = GetBit(port1, 3)
                                                                                                                                                                   chk_IO_Channel_13.IsChecked = GetBit(port1, 4)
                                                                                                                                                                   chk_IO_Channel_14.IsChecked = GetBit(port1, 5)
                                                                                                                                                                   chk_IO_Channel_15.IsChecked = GetBit(port1, 6)

                                                                                                                                                                   chk_IO_Channel_16.IsChecked = GetBit(port1, 7)
                                                                                                                                                               End If

                                                                                                                                                               ' read the adc values and update the textblocks
                                                                                                                                                               txt_ADC1.Text = expi.ADCReadVoltage(1, 0).ToString("#.###")
                                                                                                                                                               txt_ADC2.Text = expi.ADCReadVoltage(2, 0).ToString("#.###")
                                                                                                                                                               txt_ADC3.Text = expi.ADCReadVoltage(3, 0).ToString("#.###")
                                                                                                                                                               txt_ADC4.Text = expi.ADCReadVoltage(4, 0).ToString("#.###")
                                                                                                                                                               txt_ADC5.Text = expi.ADCReadVoltage(5, 0).ToString("#.###")
                                                                                                                                                               txt_ADC6.Text = expi.ADCReadVoltage(6, 0).ToString("#.###")
                                                                                                                                                               txt_ADC7.Text = expi.ADCReadVoltage(7, 0).ToString("#.###")


                                                                                                                                                               txt_ADC8.Text = expi.ADCReadVoltage(8, 0).ToString("#.###")


                                                                                                                                                           End Function)
            Catch e As Exception
                Throw e
            End Try
        End If
    End Sub

    Private Async Sub UpdateClock()
        ' Updates the clock label from the RTC clock value

        ' check that the expander pi is connected
        If expi.IsConnected Then
            Try
                ' invoke the dispatcher to update the checkboxes for expander pi IO ports with the values read from each pin

                Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()


                                                                                                                                                           End Function)
            Catch e As Exception
                Throw e
            End Try
        End If
    End Sub


    Private Sub IO_SetDirection(sender As Object, e As RoutedEventArgs)
        ' read the values from the direction radio buttons and update the expander pi 2 ports using the SetPortDirection method
        If radio_IO_Read.IsChecked = True Then
            If expi.IsConnected Then
                expi.IOSetPortDirection(0, &HFF)
                expi.IOSetPortDirection(1, &HFF)
                IO_Direction = True

                WriteMessage("IO Reading")
            Else
                radio_IO_Read.IsChecked = False
                WriteMessage("expander pi not connected")
            End If
        End If
        If radio_IO_Write.IsChecked = True Then
            If expi.IsConnected Then
                expi.IOSetPortDirection(0, &H0)
                expi.IOSetPortDirection(1, &H0)
                IO_Direction = False

                WriteMessage("IO Writing")
            Else
                radio_IO_Write.IsChecked = False
                WriteMessage("expander pi not connected")
            End If
        End If
    End Sub


    Private Sub IO_EnablePullups(sender As Object, e As RoutedEventArgs)
        ' get the value from the pull-ups checkbox and set the port pull-ups to the required state using the SetPortPullups method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If expi.IsConnected Then
            If cb.IsChecked = True Then
                expi.IOSetPortPullups(0, &HFF)
                expi.IOSetPortPullups(1, &HFF)
            Else
                expi.IOSetPortPullups(0, &H0)
                expi.IOSetPortPullups(1, &H0)
            End If
        Else
            cb.IsChecked = False
            WriteMessage("expander pi not connected")
        End If
    End Sub

    Private Sub IO_InvertPort(sender As Object, e As RoutedEventArgs)
        ' get the value from the invert port checkbox and set the port to the required state using the InvertPort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If expi.IsConnected Then
            If cb.IsChecked = True Then
                expi.IOInvertPort(0, &HFF)
                expi.IOInvertPort(1, &HFF)

                WriteMessage("IO Inverted")
            Else
                expi.IOInvertPort(0, &H0)
                expi.IOInvertPort(1, &H0)

                WriteMessage("IO not inverted")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("expander pi not connected")
        End If
    End Sub

    Private Sub IO_EnablePort0(sender As Object, e As RoutedEventArgs)
        ' get the value from the enable port checkbox and set the port values to the required state using the WritePort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If expi.IsConnected = True Then
            If IO_Direction = False Then
                If cb.IsChecked = True Then
                    expi.IOWritePort(0, &HFF)
                Else
                    expi.IOWritePort(0, &H0)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a port state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("expander pi not connected.")
        End If
    End Sub

    Private Sub IO_EnablePort1(sender As Object, e As RoutedEventArgs)
        ' get the value from the enable port checkbox and set the port values to the required state using the WritePort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If expi.IsConnected = True Then
            If IO_Direction = False Then
                If cb.IsChecked = True Then
                    expi.IOWritePort(1, &HFF)
                Else
                    expi.IOWritePort(1, &H0)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a port state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("expander pi not connected.")
        End If
    End Sub

    Private Sub IO_PinEnable(sender As Object, e As RoutedEventArgs)
        ' check which pin checkbox was clicked and update the value of that pin to the required state
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If expi.IsConnected = True Then
            If IO_Direction = False Then
                If cb.IsChecked = True Then
                    expi.IOWritePin(Convert.ToByte(cb.Content.ToString()), True)
                Else
                    expi.IOWritePin(Convert.ToByte(cb.Content.ToString()), False)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a pin state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("expander pi not connected.")
        End If
    End Sub






    Private Sub updateDAC(channel As Byte)
        If expi.IsConnected Then
            Dim newvoltage As Double = 0

            If channel = 1 Then
                newvoltage = slider_DACChannel1.Value
            Else
                newvoltage = slider_DACChannel2.Value
            End If

            ' check to see if the gain is set to 1
            If radio_DACGain1.IsChecked = True Then
                ' write the value to the dac channel 1
                expi.DACSetVoltage(channel, newvoltage, 1)
            Else
                ' gain is set to 2
                ' double the value in newvoltage before writing it to the DAC
                newvoltage = newvoltage * 2
                expi.DACSetVoltage(channel, newvoltage, 2)
            End If
        Else
            WriteMessage("expander pi not connected.")
        End If
    End Sub

    Private Sub DACChannel1_Changed(sender As Object, e As Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs)
        updateDAC(1)
    End Sub

    Private Sub DACChannel2_Changed(sender As Object, e As Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs)
        updateDAC(2)
    End Sub

    Private Sub radio_DACGain1_Checked(sender As Object, e As RoutedEventArgs)
        updateDAC(1)
        updateDAC(2)
    End Sub

    Private Sub radio_DACGain2_Checked(sender As Object, e As RoutedEventArgs)
        updateDAC(1)
        updateDAC(2)
    End Sub


    Private Sub bt_SetDate_Click(sender As Object, e As RoutedEventArgs)
        If expi.IsConnected Then
            ' create a new DateTime object using the values from the Date and Time pickers
            Dim newdate As New DateTime(picker_NewDate.[Date].Year, picker_NewDate.[Date].Month, picker_NewDate.[Date].Day, picker_NewTime.Time.Hours, picker_NewTime.Time.Minutes, picker_NewTime.Time.Seconds)

            ' update the RTC Pi with the new DateTime object
            expi.RTCSetDate(newdate)
        End If
    End Sub

    Private Sub cb_sqw_clicked(sender As Object, e As RoutedEventArgs)
        ' check the value for the SQW checkbox and enable or disable the square wave output pin
        If expi.IsConnected Then
            If cb_sqw.IsChecked = True Then
                radio_frequency_clicked(Nothing, Nothing)
                expi.RTCEnableOutput()
            Else
                expi.RTCDisableOutput()

            End If
        End If
    End Sub

    Private Sub radio_frequency_clicked(sender As Object, e As RoutedEventArgs)
        ' check which frequency radio button has been clicked and update the frequency for the square wave output
        If expi.IsConnected Then
            If radio_frequency1.IsChecked = True Then
                expi.RTCSetFrequency(1)
            End If
            If radio_frequency2.IsChecked = True Then
                expi.RTCSetFrequency(2)
            End If
            If radio_frequency3.IsChecked = True Then
                expi.RTCSetFrequency(3)
            End If
            If radio_frequency4.IsChecked = True Then
                expi.RTCSetFrequency(4)

            End If
        End If
    End Sub


End Class
