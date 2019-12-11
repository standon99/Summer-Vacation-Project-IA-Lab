using System;
using System.IO.Ports;
using System.Security.Permissions;
using System.Threading;

namespace ArduinoSlidesAndRotary
{
    public class ArduinoReaderHaptics
    {
        private SerialPort port;

        Thread readThread;

        int slider1;

        public int Slider1
        {
            get
            {
                return slider1;
            }
            set
            {
                slider1 = value;
            }
        }

        int slider2;

        public int Slider2
        {
            get
            {
                return slider2;
            }
            set
            {
                slider2 = value;
            }
        }

        int slider3;

        public int Slider3
        {
            get
            {
                return slider3;
            }
            set
            {
                slider3 = value;
            }
        }

        int slider4;

        public int Slider4
        {
            get
            {
                return slider4;
            }
            set
            {
                slider4 = value;
            }
        }

        int slider5;

        public int Slider5
        {
            get
            {
                return slider5;
            }
            set
            {
                slider5 = value;
            }
        }

        int slider6;

        public int Slider6
        {
            get
            {
                return slider6;
            }
            set
            {
                slider6 = value;
            }
        }

        int rotary1;

        public int Rotary1
        {
            get
            {
                return rotary1;
            }
            set
            {
                rotary1 = value;
            }
        }

        int press1;

        public int Press1
        {
            get
            {
                return press1;
            }
            set
            {
                press1 = value;
            }
        }

        int rotary2;

        public int Rotary2
        {
            get
            {
                return rotary2;
            }
            set
            {
                rotary2 = value;
            }
        }

        int press2;

        public int Press2
        {
            get
            {
                return press2;
            }
            set
            {
                press2 = value;
            }
        }

        int rotary3;

        public int Rotary3
        {
            get
            {
                return rotary3;
            }
            set
            {
                rotary3 = value;
            }
        }

        int press3;

        public int Press3
        {
            get
            {
                return press3;
            }
            set
            {
                press3 = value;
            }
        }

        int isMoving1;

        public int IsMoving1
        {
            get
            {
                return isMoving1;
            }
            set
            {
                isMoving1 = value;
            }
        }

        int isMoving2;

        public int IsMoving2
        {
            get
            {
                return isMoving2;
            }
            set
            {
                isMoving2 = value;
            }
        }

        int isMoving3;

        public int IsMoving3
        {
            get
            {
                return isMoving3;
            }
            set
            {
                isMoving3 = value;
            }
        }

        int isMoving4;

        public int IsMoving4
        {
            get
            {
                return isMoving4;
            }
            set
            {
                isMoving4 = value;
            }
        }

        int isMoving5;

        public int IsMoving5
        {
            get
            {
                return isMoving5;
            }
            set
            {
                isMoving5 = value;
            }
        }

        int isMoving6;

        public int IsMoving6
        {
            get
            {
                return isMoving6;
            }
            set
            {
                isMoving6 = value;
            }
        }

        private bool shouldRead;

        public void ArduinoReader(String portName, int baudRate)
        {
            port = new SerialPort(portName, baudRate);
            port.ReadTimeout = 10;
            port.WriteTimeout = 10;
            port.NewLine = "\n";
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void Terminate()
        {

            shouldRead = false;
            readThread.Interrupt();
            readThread.Abort();

            port.Close();

            Console.WriteLine("CODEMAX: Thread is alive" + readThread.IsAlive);


        }

        public bool BeginRead()
        {
            try
            {
                port.Open();
            }
            catch (TimeoutException)
            {
                return false;
            }
            shouldRead = true;
            readThread = new Thread(DoPortRead);
            readThread.Start();
            return true;
        }


        //read the values from the Arduino
        private void DoPortRead()
        {
            while (shouldRead)
            {
                try
                {
                    string line = port.ReadLine();
                    string[] values = line.Split(new char[] { ' ' });
                    Slider1 = int.Parse(values[0]);
                    Slider2 = int.Parse(values[1]);
                    Slider3 = int.Parse(values[2]);
                    Slider4 = int.Parse(values[3]);
                    Slider5 = int.Parse(values[4]);
                    Slider6 = int.Parse(values[5]);
                    Rotary1 = int.Parse(values[6]);
                    Press1 = int.Parse(values[7]);
                    Rotary2 = int.Parse(values[8]);
                    Press2 = int.Parse(values[9]);
                    Rotary3 = int.Parse(values[10]);
                    Press3 = int.Parse(values[11]);
                    IsMoving1 = int.Parse(values[12]);
                    IsMoving2 = int.Parse(values[13]);
                    IsMoving3 = int.Parse(values[14]);
                    IsMoving4 = int.Parse(values[15]);
                    IsMoving5 = int.Parse(values[16]);
                    IsMoving6 = int.Parse(values[17]);
                }
                catch (Exception)
                {
                }

            }
        }

        //sends value to the Arduino

        public void SendMessage(int id, int value)
        {

            try
            {
                if (value >= 0 && value <= 9)
                {
                    string message = id.ToString() + ",000" + value.ToString();
                    port.WriteLine(message);
                }
                if (value >= 10 && value <= 99)
                {
                    string message = id.ToString() + ",00" + value.ToString();
                    port.WriteLine(message);
                }
                if (value >= 100 && value <= 999)
                {
                    string message = id.ToString() + ",0" + value.ToString();
                    port.WriteLine(message);
                }
                if (value >= 1000 && value <= 1023)
                {
                    string message = id.ToString() + "," + value.ToString();
                    port.WriteLine(message);
                }
                
            }
            catch (Exception)
            {
            }

        }
//////////////////////////////////////////////////////////////////////
  //move slider id to dest : id slider (0 to 5) dest (0 to 1023)
        public void moveSlider(int id, int dest)
        {
            if (id >= 0 && id < 6)
            {
                SendMessageHaptics(id, dest, 0, 0);
            }
        }

        //cancel the current move
        public void stopMoving()
        {
            SendMessageHaptics(6, 0, 0, 0);
        }

        //reset to initial position
        public void reset()
        {
            SendMessageHaptics(0, 0, 0, 0);
            SendMessageHaptics(1, 1023, 0, 0);
            SendMessageHaptics(2, 0, 0, 0);
            SendMessageHaptics(3, 1023, 0, 0);
            SendMessageHaptics(4, 0, 0, 0);
            SendMessageHaptics(5, 1023, 0, 0);
        }

        //turn into haptic mode
        public void haptic()
        {
            SendMessageHaptics(7, 0, 0, 0);
            //SendMessage(7, 0);
            //UnityEngine.Debug.Log("Message (haptic function) Sent");
        }

        //discretise the 3 axes in a chosen number of steps
        public void regularStep(int nbStepX, int nbStepY, int nbStepZ)
        {
            SendMessageHaptics(8, nbStepX, nbStepY, nbStepZ);
            UnityEngine.Debug.Log("Message (regular step function) Sent");
        }

        //followingSlider follow followedSlider by a distance of dist (followingSlider and followedSlider are id between 0 and 5)
        public void follow(int followedSlider, int followingSlider, int dist)
        {
            dist = dist % 1024;
            int val; 
            if (dist < 0)
            {
                val = followedSlider * 100 + followingSlider * 10 + 0;
            }
            else
            {
                val = followedSlider * 100 + followingSlider * 10 + 1;
            }
            SendMessageHaptics(9, val, dist, 0);
        }
        
        public string convert(int value)
        {
            string convertValue = "";
            if (value >= 0 && value <= 9)
            {
                convertValue = "000" + value.ToString();
            }
            else if (value >= 10 && value <= 99)
            {
                convertValue = "00" + value.ToString();
            }
            else if (value >= 100 && value <= 999)
            {
                convertValue = "0" + value.ToString();
            }
            else if (value >= 1000 && value <= 1023)
            {
                convertValue = value.ToString();
            }
            else{
                convertValue = "0000";
            }
            return convertValue;
        }

         public void SendMessageHaptics(int id, int value, int value2, int value3)
        {
            UnityEngine.Debug.Log("ID-------------------------");
            UnityEngine.Debug.Log(id);
            try
            {
                if (id >= 0 && id < 8)
                {
                    string message = id.ToString() + "," + convert(value);
                    UnityEngine.Debug.Log("Message <8-------------------------");
                    UnityEngine.Debug.Log(message);
                    port.WriteLine(message);
                    //UnityEngine.Debug.Log("Message Sent");
                }
                else if (id == 8)
                {
                    string message = id.ToString() + "," + convert(value) + "," + convert(value2) + "," + convert(value3);
                    UnityEngine.Debug.Log("Message 8-------------------------");
                    UnityEngine.Debug.Log(message);
                }

            }
            catch (Exception)
            {
            }

        }

    }
    
}
