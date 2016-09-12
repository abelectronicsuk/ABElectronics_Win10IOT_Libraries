
Public NotInheritable Class MainPage
    Inherits Page

    Private rootFrame As Frame = TryCast(Window.Current.Content, Frame)

    Private Sub iopi_Click(sender As Object, e As RoutedEventArgs)
        rootFrame.Navigate(GetType(IOPi))
    End Sub

    Private Sub adc_Click(sender As Object, e As RoutedEventArgs)
        rootFrame.Navigate(GetType(ADCPi))
    End Sub

    Private Sub adcdifferentialpi_Click(sender As Object, e As RoutedEventArgs)
        rootFrame.Navigate(GetType(ADCDifferentialPi))
    End Sub

    Private Sub rtc_Click(sender As Object, e As RoutedEventArgs)
        rootFrame.Navigate(GetType(RTCPi))
    End Sub

    Private Sub adcdac_Click(sender As Object, e As RoutedEventArgs)
        rootFrame.Navigate(GetType(ADCDACPi))
    End Sub

    Private Sub servo_Click(sender As Object, e As RoutedEventArgs)
        rootFrame.Navigate(GetType(ServoPi))
    End Sub


End Class
