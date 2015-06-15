using Windows.ApplicationModel.Background;
using System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace ServoPiServoMoveTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        //Initialize the ServoPi class:

        ABElectronics_Win10IOT_Libraries.ServoPi servo = new ABElectronics_Win10IOT_Libraries.ServoPi(0x40);

        // Create a timer which we will use to set the servo movement time
        Timer timer;

        // Create an array of servo motor positions
        short[] positions = new short[] { 250, 400, 500 };

        // A counter for the array loop
        int x = 0;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // connect to the servo pi
            servo.Connect();

            // wait until a connection is established
            while (!servo.IsConnected)
            {
            }

            // set the pwm frequency to 60Hz
            servo.SetPWMFreqency(60);           

            // assign the timer to tick once a second
            timer = new Timer(Timer_Tick, null, 1000, Timeout.Infinite);


            // waste some cycles so the program doesn't exit.
            while (true)
            {
                
            }
        }

        private void Timer_Tick(object state)
        {
            // move the servo to a new position from the array
            servo.SetPWM(1, 0, positions[x]);

            // increase the array position unless it is at the end of the array in which case set it to 0
            x++;
            if (x >= positions.Length)
            {
                x = 0;
            }

            // reset the timer so it ticks again in 1 second
            timer.Change(1000, Timeout.Infinite);
        }
    }
}


