Imports System.Threading.Tasks
Imports System.Threading


Public NotInheritable Class IOPi
    Inherits Page

    ' The IO Pi contains two MCP23017 chips so we need to create a separate instance of the IOPi class for each chip and call the bus1 and bus2
    Public bus1 As New ABElectronics_Win10IOT_Libraries.IOPi(&H20)
    Public bus2 As New ABElectronics_Win10IOT_Libraries.IOPi(&H21)

    ' create two timers for reading from each IO Pi bus
    Private TIME_INTERVAL_IN_MILLISECONDS As Integer = 200
    Private _timer1 As Timer
    Private _timer2 As Timer

    ' used to set the bus direction.  true = read, false = write.
    Private Bus1_Direction As Boolean = True
    Private Bus2_Direction As Boolean = True

    Public Sub New()
        Me.InitializeComponent()
        ' initialise the timers with the preset period
        _timer1 = New Timer(AddressOf Timer1_Tick, Nothing, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)

        _timer2 = New Timer(AddressOf Timer2_Tick, Nothing, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub

    Private Async Sub bt_Bus1_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' check to see if there is an address in the textbox for bus 1 and if so connect to the IO Pi
        If txt_Bus1_Address.Text.Length > 0 Then
            Try
                ' get the i2c address from the textbox for bus 1
                bus1.Address = Convert.ToByte(txt_Bus1_Address.Text.Replace("0x", ""), 16)

                ' create an event handler for the Connected event and connect to bus 1
                AddHandler bus1.Connected, AddressOf Bus1_Connected
                Await bus1.Connect()
            Catch ex As Exception
                Throw ex
            End Try
        End If
    End Sub


    Private Sub Bus1_Connected(sender As Object, e As EventArgs)
        ' bus 1 is connected so update the message box, read from the bus and start the timer
        WriteMessage("Bus 1 Connected")
        ReadBus1()
        _timer1.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub

    Private Async Sub bt_Bus2_Connect_Click(sender As Object, e As RoutedEventArgs)
        ' check to see if there is an address in the textbox for bus 2 and if so connect to the IO Pi
        If txt_Bus2_Address.Text.Length > 0 Then
            Try
                ' get the i2c address from the textbox for bus 2
                bus2.Address = Convert.ToByte(txt_Bus2_Address.Text.Replace("0x", ""), 16)

                ' create an event handler for the Connected event and connect to bus 2
                AddHandler bus2.Connected, AddressOf Bus2_Connected
                Await bus2.Connect()
            Catch ex As Exception
                Throw ex
            End Try
        End If
    End Sub

    Private Sub Bus2_Connected(sender As Object, e As EventArgs)
        ' bus 2 is connected so update the message box, read from the bus and start the timer
        WriteMessage("Bus 2 Connected")
        ReadBus2()
        _timer2.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
    End Sub

    Private Sub Timer1_Tick(state As [Object])
        ' on timer tick check if the bus is connected and read from the bus before resetting the timer
        If bus1.IsConnected Then
            ReadBus1()
            _timer1.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        End If
    End Sub

    Private Sub Timer2_Tick(state As [Object])
        ' on timer 2 tick check if the bus is connected and read from the bus before resetting the timer
        If bus2.IsConnected Then
            ReadBus2()
            _timer2.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite)
        End If
    End Sub

    Private Async Sub ReadBus1()
        ' check that the bus is connected
        If bus1.IsConnected Then
            Try
                ' invoke the dispatcher to update the checkboxes for bus 1 with the values read from each pin
                Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                               Bus1_Channel_01_checkBox.IsChecked = bus1.ReadPin(1)
                                                                                                                                                               Bus1_Channel_02_checkBox.IsChecked = bus1.ReadPin(2)
                                                                                                                                                               Bus1_Channel_03_checkBox.IsChecked = bus1.ReadPin(3)
                                                                                                                                                               Bus1_Channel_04_checkBox.IsChecked = bus1.ReadPin(4)
                                                                                                                                                               Bus1_Channel_05_checkBox.IsChecked = bus1.ReadPin(5)
                                                                                                                                                               Bus1_Channel_06_checkBox.IsChecked = bus1.ReadPin(6)
                                                                                                                                                               Bus1_Channel_07_checkBox.IsChecked = bus1.ReadPin(7)
                                                                                                                                                               Bus1_Channel_08_checkBox.IsChecked = bus1.ReadPin(8)
                                                                                                                                                               Bus1_Channel_09_checkBox.IsChecked = bus1.ReadPin(9)
                                                                                                                                                               Bus1_Channel_10_checkBox.IsChecked = bus1.ReadPin(10)
                                                                                                                                                               Bus1_Channel_11_checkBox.IsChecked = bus1.ReadPin(11)
                                                                                                                                                               Bus1_Channel_12_checkBox.IsChecked = bus1.ReadPin(12)
                                                                                                                                                               Bus1_Channel_13_checkBox.IsChecked = bus1.ReadPin(13)
                                                                                                                                                               Bus1_Channel_14_checkBox.IsChecked = bus1.ReadPin(14)
                                                                                                                                                               Bus1_Channel_15_checkBox.IsChecked = bus1.ReadPin(15)
                                                                                                                                                               Bus1_Channel_16_checkBox.IsChecked = bus1.ReadPin(16)
                                                                                                                                                               Return 0

                                                                                                                                                           End Function)
            Catch e As Exception
                Throw e
            End Try
        End If
    End Sub

    Private Async Sub ReadBus2()
        ' check that the bus is connected
        If bus2.IsConnected Then
            Try
                ' invoke the dispatcher to update the checkboxes for bus 1 with the values read from each pin
                Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                               Bus2_Channel_01_checkBox.IsChecked = bus2.ReadPin(1)
                                                                                                                                                               Bus2_Channel_02_checkBox.IsChecked = bus2.ReadPin(2)
                                                                                                                                                               Bus2_Channel_03_checkBox.IsChecked = bus2.ReadPin(3)
                                                                                                                                                               Bus2_Channel_04_checkBox.IsChecked = bus2.ReadPin(4)
                                                                                                                                                               Bus2_Channel_05_checkBox.IsChecked = bus2.ReadPin(5)
                                                                                                                                                               Bus2_Channel_06_checkBox.IsChecked = bus2.ReadPin(6)
                                                                                                                                                               Bus2_Channel_07_checkBox.IsChecked = bus2.ReadPin(7)
                                                                                                                                                               Bus2_Channel_08_checkBox.IsChecked = bus2.ReadPin(8)
                                                                                                                                                               Bus2_Channel_09_checkBox.IsChecked = bus2.ReadPin(9)
                                                                                                                                                               Bus2_Channel_10_checkBox.IsChecked = bus2.ReadPin(10)
                                                                                                                                                               Bus2_Channel_11_checkBox.IsChecked = bus2.ReadPin(11)
                                                                                                                                                               Bus2_Channel_12_checkBox.IsChecked = bus2.ReadPin(12)
                                                                                                                                                               Bus2_Channel_13_checkBox.IsChecked = bus2.ReadPin(13)
                                                                                                                                                               Bus2_Channel_14_checkBox.IsChecked = bus2.ReadPin(14)
                                                                                                                                                               Bus2_Channel_15_checkBox.IsChecked = bus2.ReadPin(15)
                                                                                                                                                               Bus2_Channel_16_checkBox.IsChecked = bus2.ReadPin(16)
                                                                                                                                                               Return 0

                                                                                                                                                           End Function)
            Catch e As Exception
                Throw e
            End Try
        End If
    End Sub


    Private Sub Bus1_SetDirection(sender As Object, e As RoutedEventArgs)
        ' read the values from the direction radio buttons and update the bus 1 ports using the SetPortDirection method
        If radio_Bus1_Read.IsChecked = True Then
            If bus1.IsConnected Then
                bus1.SetPortDirection(0, &HFF)
                bus1.SetPortDirection(1, &HFF)
                Bus1_Direction = True

                WriteMessage("Bus 1 Reading")
            Else
                radio_Bus1_Read.IsChecked = False
                WriteMessage("Bus 1 not connected")
            End If
        End If
        If radio_Bus1_Write.IsChecked = True Then
            If bus1.IsConnected Then
                bus1.SetPortDirection(0, &H0)
                bus1.SetPortDirection(1, &H0)
                Bus1_Direction = False

                WriteMessage("Bus 1 Writing")
            Else
                radio_Bus1_Write.IsChecked = False
                WriteMessage("Bus 1 not connected")
            End If
        End If

    End Sub

    Private Sub Bus2_SetDirection(sender As Object, e As RoutedEventArgs)
        ' read the values from the direction radio buttons and update the bus 2 ports using the SetPortDirection method
        If radio_Bus2_Read.IsChecked = True Then
            If bus2.IsConnected Then
                bus2.SetPortDirection(0, &HFF)
                bus2.SetPortDirection(1, &HFF)
                Bus2_Direction = True

                WriteMessage("Bus 2 Reading")
            Else
                radio_Bus2_Read.IsChecked = False
                WriteMessage("Bus 2 not connected")
            End If
        End If
        If radio_Bus2_Write.IsChecked = True Then
            If bus2.IsConnected Then
                bus2.SetPortDirection(0, &H0)
                bus2.SetPortDirection(1, &H0)
                Bus2_Direction = False

                WriteMessage("Bus 2 Writing")
            Else
                radio_Bus2_Write.IsChecked = False
                WriteMessage("Bus 2 not connected")
            End If
        End If
    End Sub


    Private Sub Bus1_EnablePullups(sender As Object, e As RoutedEventArgs)
        ' get the value from the pull-ups checkbox and set the port pull-ups to the required state using the SetPortPullups method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus1.IsConnected Then
            If cb.IsChecked = True Then
                bus1.SetPortPullups(0, &HFF)
                bus1.SetPortPullups(1, &HFF)
            Else
                bus1.SetPortPullups(0, &H0)
                bus1.SetPortPullups(1, &H0)
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 1 not connected")
        End If
    End Sub

    Private Sub Bus2_EnablePullups(sender As Object, e As RoutedEventArgs)
        ' get the value from the pull-ups checkbox and set the port pull-ups to the required state using the SetPortPullups method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus2.IsConnected Then
            If cb.IsChecked = True Then
                bus2.SetPortPullups(0, &HFF)
                bus2.SetPortPullups(1, &HFF)
            Else
                bus2.SetPortPullups(0, &H0)
                bus2.SetPortPullups(1, &H0)
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 2 not connected")
        End If
    End Sub



    Private Sub Bus1_InvertPort(sender As Object, e As RoutedEventArgs)
        ' get the value from the invert port checkbox and set the port to the required state using the InvertPort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus1.IsConnected Then
            If cb.IsChecked = True Then
                bus1.InvertPort(0, &HFF)
                bus1.InvertPort(1, &HFF)

                WriteMessage("Bus 2 Inverted")
            Else
                bus1.InvertPort(0, &H0)
                bus1.InvertPort(1, &H0)

                WriteMessage("Bus 1 not inverted")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 1 not connected")
        End If
    End Sub

    Private Sub Bus2_InvertPort(sender As Object, e As RoutedEventArgs)
        ' get the value from the invert port checkbox and set the port to the required state using the InvertPort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus2.IsConnected Then
            If cb.IsChecked = True Then
                bus2.InvertPort(0, &HFF)
                bus2.InvertPort(1, &HFF)

                WriteMessage("Bus 2 Inverted")
            Else
                bus2.InvertPort(0, &H0)
                bus2.InvertPort(1, &H0)

                WriteMessage("Bus 2 not inverted")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 2 not connected")
        End If
    End Sub
    Private Sub Bus1_EnablePort0(sender As Object, e As RoutedEventArgs)
        ' get the value from the enable port checkbox and set the port values to the required state using the WritePort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus1.IsConnected = True Then
            If Bus1_Direction = False Then
                If cb.IsChecked = True Then
                    bus1.WritePort(0, &HFF)
                Else
                    bus1.WritePort(0, &H0)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a port state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 1 not connected.")
        End If
    End Sub

    Private Sub Bus1_EnablePort1(sender As Object, e As RoutedEventArgs)
        ' get the value from the enable port checkbox and set the port values to the required state using the WritePort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus1.IsConnected = True Then
            If Bus1_Direction = False Then
                If cb.IsChecked = True Then
                    bus1.WritePort(1, &HFF)
                Else
                    bus1.WritePort(1, &H0)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a port state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 1 not connected.")
        End If
    End Sub

    Private Sub Bus2_EnablePort0(sender As Object, e As RoutedEventArgs)
        ' get the value from the enable port checkbox and set the port values to the required state using the WritePort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus2.IsConnected = True Then
            If Bus2_Direction = False Then
                If cb.IsChecked = True Then
                    bus2.WritePort(0, &HFF)
                Else
                    bus2.WritePort(0, &H0)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a port state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 2 not connected.")
        End If
    End Sub

    Private Sub Bus2_EnablePort1(sender As Object, e As RoutedEventArgs)
        ' get the value from the enable port checkbox and set the port values to the required state using the WritePort method
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus2.IsConnected = True Then
            If Bus2_Direction = False Then
                If cb.IsChecked = True Then
                    bus2.WritePort(1, &HFF)
                Else
                    bus2.WritePort(1, &H0)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a port state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 2 not connected.")
        End If
    End Sub

    Private Sub Bus1_PinEnable(sender As Object, e As RoutedEventArgs)
        ' check which pin checkbox was clicked and update the value of that pin to the required state
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus1.IsConnected = True Then
            If Bus1_Direction = False Then
                If cb.IsChecked = True Then
                    bus1.WritePin(Convert.ToByte(cb.Content.ToString()), True)
                Else
                    bus1.WritePin(Convert.ToByte(cb.Content.ToString()), False)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a pin state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 2 not connected.")
        End If
    End Sub

    Private Sub Bus2_PinEnable(sender As Object, e As RoutedEventArgs)
        ' check which pin checkbox was clicked and update the value of that pin to the required state
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        If bus2.IsConnected = True Then
            If Bus2_Direction = False Then
                If cb.IsChecked = True Then
                    bus2.WritePin(Convert.ToByte(cb.Content.ToString()), True)
                Else
                    bus2.WritePin(Convert.ToByte(cb.Content.ToString()), False)
                End If
            Else
                cb.IsChecked = False
                WriteMessage("You can not set a pin state in read mode.")
            End If
        Else
            cb.IsChecked = False
            WriteMessage("Bus 2 not connected.")
        End If
    End Sub


    Private Async Sub WriteMessage(message As String)
        ' this method updates the Message textbox on the page
        Await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, Function()
                                                                                                                                                       txt_Message.Text = message
                                                                                                                                                       Return 0
                                                                                                                                                   End Function)
    End Sub


    Private Sub bt_Back_Clicked(sender As Object, e As RoutedEventArgs)
        ' go back to the main page
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)
        rootFrame.Navigate(GetType(MainPage))
    End Sub

End Class
