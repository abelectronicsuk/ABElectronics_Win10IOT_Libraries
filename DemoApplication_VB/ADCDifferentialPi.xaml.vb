Imports System.Threading

Public NotInheritable Class ADCDifferentialPi
    Inherits Page

    ' create an instance of the ADCDifferentialPi class called adc
    Private adc As New ABElectronics_Win10IOT_Libraries.ADCDifferentialPi()

    ' set the time interval for sampling from the ADC and create a timer
    Private TIME_INTERVAL_IN_MILLISECONDS As Integer = 10
    Private _timer As Timer

    ' this will be used to measure the sample rate from the ADC Pi
    Private startTime As DateTime

    ' used to start and stop the sampling
    Private run As Boolean = False

    ' temporary variables where the ADC values will be stored
    Private channel1_value As Double = 0
    Private channel2_value As Double = 0
    Private channel3_value As Double = 0
    Private channel4_value As Double = 0
    Private channel5_value As Double = 0
    Private channel6_value As Double = 0
    Private channel7_value As Double = 0
    Private channel8_value As Double = 0

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Private Async Sub bt_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' when the connect button is clicked update the ADC i2c addresses with the values in the textboxes on the page
        Try
            adc.Address1 = Convert.ToByte(txt_Address1.Text.Replace("0x", ""), 16)
            adc.Address2 = Convert.ToByte(txt_Address2.Text.Replace("0x", ""), 16)
        Catch ex As Exception
            Throw ex
        End Try

        ' create a Connected event handler and connect to the Delta-Sigma Pi.
        AddHandler adc.Connected, AddressOf Adc_Connected
        Await adc.Connect()
    End Sub

    Private Sub Adc_Connected(sender As Object, e As EventArgs)
        ' The Delta-Sigma Pi is connected

        ' set the initial bit rate to 16
        adc.SetBitRate(16)
        radio_bitrate12.IsChecked = False
        radio_bitrate14.IsChecked = False
        radio_bitrate16.IsChecked = True
        radio_bitrate18.IsChecked = False

        ' set the gain to 1
        adc.SetPGA(1)
        radio_Gain1.IsChecked = True
        radio_Gain2.IsChecked = False
        radio_Gain4.IsChecked = False
        radio_Gain8.IsChecked = False

        ' set the startTime to be now and start the timer
        startTime = DateTime.Now
        _timer = New Timer(AddressOf ReadADC, Nothing, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)

    End Sub

    Private Async Sub ReadADC(state As Object)
        ' check to see if the run is true
        If run Then
            ' get the voltage values from all 8 ADC channels
            channel1_value = adc.ReadVoltage(1)
            channel2_value = adc.ReadVoltage(2)
            channel3_value = adc.ReadVoltage(3)
            channel4_value = adc.ReadVoltage(4)
            channel5_value = adc.ReadVoltage(5)
            channel6_value = adc.ReadVoltage(6)
            channel7_value = adc.ReadVoltage(7)
            channel8_value = adc.ReadVoltage(8)

            ' use a dispatcher event to update the textboxes on the page
            Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                           txt_Channel1.Text = channel1_value.ToString("F4")
                                                                                                                                                           txt_Channel2.Text = channel2_value.ToString("F4")
                                                                                                                                                           txt_Channel3.Text = channel3_value.ToString("F4")
                                                                                                                                                           txt_Channel4.Text = channel4_value.ToString("F4")
                                                                                                                                                           txt_Channel5.Text = channel5_value.ToString("F4")
                                                                                                                                                           txt_Channel6.Text = channel6_value.ToString("F4")
                                                                                                                                                           txt_Channel7.Text = channel7_value.ToString("F4")
                                                                                                                                                           txt_Channel8.Text = channel8_value.ToString("F4")
                                                                                                                                                           Return 0
                                                                                                                                                       End Function)

            ' calculate how long it has been since the last reading and use that to work out the sample rate
            Dim duration As TimeSpan = DateTime.Now.Subtract(startTime)
            startTime = DateTime.Now
            WriteMessage((1000 / (duration.TotalMilliseconds / 8)).ToString("F2") + "sps")

            ' reset the timer so it will run again after the preset period
            _timer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        End If
    End Sub

    Private Sub radio_bitrate_clicked(sender As Object, e As RoutedEventArgs)
        ' find out which radio button was clicked and use that to update the bit rate for the ADC
        Dim cb As RadioButton = DirectCast(sender, RadioButton)
        If adc.IsConnected = True Then
            adc.SetBitRate(Convert.ToByte(cb.Content.ToString()))
        Else
            WriteMessage("ADC not connected.")
        End If
    End Sub

    Private Sub radio_gain_clicked(sender As Object, e As RoutedEventArgs)
        ' find out which radio button was clicked and use that to update the gain for the ADC
        Dim cb As RadioButton = DirectCast(sender, RadioButton)
        If adc.IsConnected = True Then
            adc.SetPGA(Convert.ToByte(cb.Content.ToString()))
        Else
            WriteMessage("ADC not connected.")
        End If
    End Sub

    Private Sub bt_Back_Clicked(sender As Object, e As RoutedEventArgs)
        ' go back to the main page
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame.Navigate(GetType(MainPage))
    End Sub

    Private Async Sub WriteMessage(message As String)
        ' WriteMessage is used to update the message box on the page
        Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                       txt_Message.Text = message

                                                                                                                                                   End Function)
    End Sub

    Private Sub bt_Start_Click(sender As Object, e As RoutedEventArgs)
        ' set run to be true and call ReadADC to start the ADC reading
        run = True
        ReadADC(Nothing)
    End Sub

    Private Sub bt_Stop_Click(sender As Object, e As RoutedEventArgs)
        ' set run to false to stop the ADC from reading
        run = False
    End Sub

End Class
