Imports System.Threading.Tasks
Imports System.Threading


Public NotInheritable Class ServoPi
    Inherits Page

    Private servo As New ABElectronics_Win10IOT_Libraries.ServoPi()

    Public Sub New()
        Me.InitializeComponent()
    End Sub

    Private Async Sub bt_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' check if the servo pi is already connected and if the i2c address textbox contains a value
        If (Not servo.IsConnected) AndAlso (txt_Address.Text.Length > 0) Then
            Try
                ' set the i2c address from the textbox value
                servo.Address = Convert.ToByte(txt_Address.Text.Replace("0x", ""), 16)

                ' create a Connected event listener and connect to the servo pi
                AddHandler servo.Connected, AddressOf Servo_Connected


                Await servo.Connect()
            Catch ex As Exception
                Throw ex
            End Try
        End If

    End Sub

    Private Sub Servo_Connected(sender As Object, e As EventArgs)
        ' on connection get the value from the frequency slider and set the PWM frequency on the Servo Pi
        Dim frequency As Integer = Convert.ToInt32(slider_Frequency.Value)
        servo.SetPWMFreqency(frequency)
        WriteMessage("Connected")
    End Sub

    Private Sub FrequencySliderChanged(sender As Object, e As RangeBaseValueChangedEventArgs)
        If servo.IsConnected Then
            ' get the value from the frequency slider and set the PWM frequency on the Servo Pi
            Dim frequency As Integer = Convert.ToInt32(slider_Frequency.Value)
            servo.SetPWMFreqency(frequency)
        End If
    End Sub



    Private Sub ChannelSliderChanged(sender As Object, e As RangeBaseValueChangedEventArgs)

        If servo.IsConnected Then
            ' Find out which slider was changed and use the slider value to update the PWM value on the Servo Pi

            Dim slider As Slider = DirectCast(sender, Slider)
            Dim channel As Byte = Convert.ToByte(slider.Name.ToString().Replace("slider_Channel", ""))
            Dim highvalue As Short = 0
            Dim lowvalue As Short = Convert.ToInt16(slider.Value)
            Debug.WriteLine(highvalue.ToString() + " " + lowvalue.ToString())
            servo.SetPWM(channel, highvalue, lowvalue)
        End If
    End Sub

    Private Sub cbServoControl_Click(sender As Object, e As RoutedEventArgs)
        ' create an array containing all of the channel sliders
        Dim sliders As Slider() = {slider_Channel1, slider_Channel2, slider_Channel3, slider_Channel4, slider_Channel5, slider_Channel6,
        slider_Channel7, slider_Channel8, slider_Channel9, slider_Channel10, slider_Channel11, slider_Channel12,
        slider_Channel13, slider_Channel14, slider_Channel15, slider_Channel16}

        ' check to see if the checkbox is checked
        If cbServoControl.IsChecked = True Then
            ' set the frequency to 60Hz and the slider limits to be 150 to 700
            ' these values should allow the Servo Pi to control most RC model servos.
            slider_Frequency.Value = 60
            servo.SetPWMFreqency(60)

            ' loop through all of the sliders setting their value, minimum and maximum
            For Each slider As Slider In sliders
                slider.Value = 425
                slider.Minimum = 150
                slider.Maximum = 700


            Next
        Else
            ' reset the sliders to the default limits
            For Each slider As Slider In sliders
                slider.Value = 0
                slider.Minimum = 0
                slider.Maximum = 4096
            Next
        End If

    End Sub

    Private Async Sub WriteMessage(message As String)
        ' used to update the message textbox on the page
        Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                       txt_Message.Text = message
                                                                                                                                                       Return 0
                                                                                                                                                   End Function)
    End Sub

    Private Sub bt_Back_Clicked(sender As Object, e As RoutedEventArgs)
        ' dispose of the servo pi and go back to the main page
        Try
            servo.Dispose()
        Catch
        End Try
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame.Navigate(GetType(MainPage))
    End Sub

End Class
