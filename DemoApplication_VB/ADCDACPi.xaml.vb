Imports System.Threading.Tasks
Imports System.Threading

Public NotInheritable Class ADCDACPi
    Inherits Page


    ' create an instance of the ADCDAC class
    Private adcdac As New ABElectronics_Win10IOT_Libraries.ADCDACPi()

    ' variables for storing the ADC values
    Private ADC1_value As Double = 0
    Private ADC2_value As Double = 0

    ' A timer for reading from the ADC
    Private _timer As Timer

    ' set a time interval for reading from the ADC
    Private TIME_INTERVAL_IN_MILLISECONDS As Integer = 50

    Public Sub New()
        Me.InitializeComponent()
    End Sub


    Private Sub bt_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' when the connect button is clicked set the ADC reference voltage, create an event handler for the Connected event and connect to the ADCDAC Pi.
        adcdac.SetADCrefVoltage(3.3)
        AddHandler adcdac.Connected, AddressOf Adcdac_Connected
        adcdac.Connect()
    End Sub

    Private Sub Adcdac_Connected(sender As Object, e As EventArgs)
        ' The ADCDAC Pi is connected to start the timer to read from the ADC channels
        _timer = New Timer(AddressOf ReadADC, Nothing, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub

    Private Async Sub ReadADC(state As Object)
        ' Get the values from both ADC channels and store them in two variables.
        ADC1_value = adcdac.ReadADCVoltage(1)
        ADC2_value = adcdac.ReadADCVoltage(2)

        ' use a dispatcher event to update the textboxes on the page with the saved values
        Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                       txt_ADC1.Text = ADC1_value.ToString("F3")
                                                                                                                                                       txt_ADC2.Text = ADC2_value.ToString("F3")
                                                                                                                                                       Return 0
                                                                                                                                                   End Function)

        ' reset the timer so it will trigger again after the set period
        _timer = New Timer(AddressOf ReadADC, Nothing, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub



    Private Sub DAC1SliderChanged(sender As Object, e As RangeBaseValueChangedEventArgs)
        ' get the new value from slider 1 and use it to update the DAC channel 1
        Dim dac_value As Double = slider_Channel1.Value
        adcdac.SetDACVoltage(1, dac_value)
    End Sub

    Private Sub DAC2SliderChanged(sender As Object, e As RangeBaseValueChangedEventArgs)
        ' get the new value from slider 2 and use it to update the DAC channel 2
        Dim dac_value As Double = slider_Channel2.Value
        adcdac.SetDACVoltage(2, dac_value)
    End Sub

    Private Sub bt_Back_Clicked(sender As Object, e As RoutedEventArgs)
        ' go back to the main page
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame.Navigate(GetType(MainPage))
    End Sub

End Class
