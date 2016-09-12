Imports System.Threading.Tasks
Imports System.Threading


Public NotInheritable Class RTCPi
    Inherits Page
    ' create an instance of the RTCPi class called rtc
    Private rtc As New ABElectronics_Win10IOT_Libraries.RTCPi()

    ' a timer will be used to read from the RTC Pi at 1 second intervals
    Private _timer As Timer

    Public Sub New()

        Me.InitializeComponent()
    End Sub

    Private Async Sub bt_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' when the connect button is clicked check that the RTC Pi is not already connected before creating a Connected event handler and connecting to the RTC Pi
        If Not rtc.IsConnected Then
            AddHandler rtc.Connected, AddressOf Rtc_Connected
            Await rtc.Connect()
        End If
    End Sub

    Private Sub Rtc_Connected(sender As Object, e As EventArgs)
        ' a connection has been established so start the timer to read the date from the RTC Pi
        _timer = New Timer(AddressOf Timer_Tick, Nothing, 1000, Timeout.Infinite)
    End Sub

    Private Async Sub Timer_Tick(state As Object)
        ' check that the RTC Pi is still connected
        If rtc.IsConnected Then
            Try
                ' read the current date and time from the RTC Pi into a DateTime object
                Dim [date] As DateTime = rtc.ReadDate()

                ' invoke a dispatcher to update the date textbox on the page
                Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                               txt_Date.Text = [date].ToString("d MMMM yyyy hh:mm:ss tt")
                                                                                                                                                               Return 0
                                                                                                                                                           End Function)
            Catch
                txt_Date.Text = "Error Reading Date"
            End Try
        End If
        _timer.Change(1000, Timeout.Infinite)
    End Sub

    Private Sub bt_SetDate_Click(sender As Object, e As RoutedEventArgs)
        If rtc.IsConnected Then
            ' create a new DateTime object using the values from the Date and Time pickers
            Dim newdate As New DateTime(picker_NewDate.[Date].Year, picker_NewDate.[Date].Month, picker_NewDate.[Date].Day, picker_NewTime.Time.Hours, picker_NewTime.Time.Minutes, picker_NewTime.Time.Seconds)

            ' update the RTC Pi with the new DateTime object
            rtc.SetDate(newdate)
        End If
    End Sub

    Private Sub cb_sqw_clicked(sender As Object, e As RoutedEventArgs)
        ' check the value for the SQW checkbox and enable or disable the square wave output pin
        If rtc.IsConnected Then
            If cb_sqw.IsChecked = True Then
                radio_frequency_clicked(Nothing, Nothing)
                rtc.EnableOutput()
            Else
                rtc.DisableOutput()

            End If
        End If
    End Sub

    Private Sub radio_frequency_clicked(sender As Object, e As RoutedEventArgs)
        ' check which frequency radio button has been clicked and update the frequency for the square wave output
        If rtc.IsConnected Then
            If radio_frequency1.IsChecked = True Then
                rtc.SetFrequency(1)
            End If
            If radio_frequency2.IsChecked = True Then
                rtc.SetFrequency(2)
            End If
            If radio_frequency3.IsChecked = True Then
                rtc.SetFrequency(3)
            End If
            If radio_frequency4.IsChecked = True Then
                rtc.SetFrequency(4)

            End If
        End If
    End Sub


    Private Sub bt_Back_Clicked(sender As Object, e As RoutedEventArgs)
        ' dispose of the rtc object and go back to the main page
        Try
            rtc.Dispose()
        Catch
        End Try
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame.Navigate(GetType(MainPage))
    End Sub
End Class
