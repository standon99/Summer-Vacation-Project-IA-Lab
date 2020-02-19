using System;
using System.IO.Ports;
using System.Security.Permissions;
using System.Threading;

namespace WirelessArduinoSlidesAndRotary
{
    public class ArduinoReaderHaptics
    {
        public SerialPort port;
        public SerialPort[] portsArray = new SerialPort[100];

        Thread readThread;

        int slider1;
        public SerialPort[] testArray = new SerialPort[1000];
        private int cnt = 0;
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
        bool[] success = new bool[100];
        // A new scanning function to detect all open COM ports


        public string[] findCOMPorts(int baudRate)
        {
            
            string[] comPorts = new string[5];
            
            //SerialPort [] testArray = new SerialPort[100];
            int index = 0;
            int comPortsCntr = 0;
            for (int y = 21; index < comPorts.Length; y=y+3)
            {
                string portname = ("COM" + y.ToString());
                //UnityEngine.Debug.Log(portname);
                SerialPort testPort = new SerialPort(portname, baudRate);
                {
                    try
                    {
                        testPort.Open();
                        UnityEngine.Debug.Log("Port Opened" + y.ToString());
                        comPorts[index] = portname;
                        //success[y] = true;
                        comPortsCntr++;
                        index++;
                        testPort.Close();
                       // might need to modify the + 2
                    }
                    catch (Exception e)
                    {
                        comPorts[index] = "NULL";
                        index++;
                        //UnityEngine.Debug.Log("Port Open Failed (Function)" + y.ToString());
                    }

                    int newCtr = 0;

                    for (int t = 0; t < (comPortsCntr - 1); t++)
                    {
                        string[] comPortsCopy = comPorts;
                        if (comPortsCopy[t] != "NULL")
                        {
                            comPorts[newCtr] = comPortsCopy[t];
                            newCtr++;
                        }
                    }
                }
            }
            UnityEngine.Debug.Log("Array of COM ports");
            UnityEngine.Debug.Log(comPorts);
            /////////NEED TO INCLUDE THE PORT TIMEOUTS!!!!
            //comPorts = new string[2];
            //comPorts[0] = "COM21";
           // comPorts[1] = "COM24";
            //comPorts[2] = "COM27";
            return comPorts;
        }
        public void ArduinoReader(String portName, int baudRate)
        {
            port = new SerialPort(portName, baudRate);
            port.ReadTimeout = 1;
            port.WriteTimeout = 1;
            //port.NewLine = "\n";
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void Terminate()
        {
            UnityEngine.Debug.Log("Terminate Called");

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
                UnityEngine.Debug.Log("Port Opened");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Port Open Failed");
                return false;
            }
            // shouldRead = true;
            //readThread = new Thread(DoPortRead);
            //readThread.Start();
            return true;
        }


        //read the values from the Arduino
        private void DoPortRead()
        {
            //UnityEngine.Debug.Log("CALLLELED");
            while (shouldRead)
            {
                try
                {
                    string line = port.ReadLine();
                    /*
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
                    */
                    //UnityEngine.Debug.Log(line);
                }
                catch (Exception e)
                {
                    //UnityEngine.Debug.Log(e);
                    // break;
                }

            }
        }

        //sends value to the Arduino - checked is good
        public void MoveSlider(string name, int pos1, int pos2, int baudRate, int hapticstat, int xSteps, int ySteps, int zSteps)
        {
            SendMessage(name, pos1, pos2, baudRate, hapticstat, xSteps, ySteps, zSteps);
        }

        public void SendMessage(string COMAddress, int value1, int value2, int baudRate, int hapticsStat, int xStep, int yStep, int zStep)
        {
            /*
            string[] portNo = COMAddress.Split('M');
            int intPortNo = Int32.Parse(portNo[1]);
            if (portsArray[intPortNo] == null)
            {
                portsArray[intPortNo] = new SerialPort(COMAddress, baudRate);
                portsArray[intPortNo].Open();
            }
            port = portsArray[intPortNo];
            */
            if (true)
            {
                string value1String = value1.ToString();
                string value2String = value2.ToString();
      
                // Print value 1
                if (value1 >= 0 && value1 <= 9)
                {
                    value1String = "000" + value1String;
                }
                else if (value1 >= 10 && value1 <= 99)
                {
                    value1String = "00" + value1String;
                }
                else if (value1 >= 100 && value1 <= 999)
                {
                    value1String = "0" + value1String;
                }
                else if (value1 >= 1000 && value1 <= 1023)
                {
                    value1String = "" + value1String;
                }
                // Print value 2 
                if (value2 >= 0 && value2 <= 9)
                {
                    value2String = "000" + value2String;
                }
                else if (value2 >= 10 && value2 <= 99)
                {
                    value2String = "00" + value2String;
                }
                else if (value2 >= 100 && value2 <= 999)
                {
                    value2String = "0" + value2String;
                }
                else if (value2 >= 1000 && value2 <= 1023)
                {
                    value2String = "" + value2String;
                }
                // Sent message gives first pos value, second pos value and whether haptics need to be engaged
                string message = value1String + value2String + hapticsStat.ToString() + xStep.ToString()  + yStep.ToString() + zStep.ToString();
                //UnityEngine.Debug.Log(message);
                try
                {
                    port = new SerialPort(COMAddress, baudRate);
                    //port = new SerialPort("COM21", baudRate);
                    port.ReadTimeout = 1;
                    port.WriteTimeout = 1;
                    port.Open();
                    port.WriteLine(message);
                    port.Close();
                    UnityEngine.Debug.Log("A MESSAGE WAS SENT!!!!!!!!!!");
                    //UnityEngine.Debug.Log(cnt+1);
                }
                catch (Exception)
                {
                    UnityEngine.Debug.Log(COMAddress);
                    UnityEngine.Debug.Log("A message failed to send (send message ERROR 1)");
                }
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
        /*
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
        */
        //turn into haptic mode- here a data set is mapped by the Arduinos
        public void haptic(string comAddress, int baudRate)
        {
            //SendMessageHaptics(7, 0, 0, 0);
            SendMessage(comAddress, 0, 0, baudRate, 1, 0,0,0);
            //UnityEngine.Debug.Log("Message (haptic function) Sent");
        }

        //discretise the 3 axes in a chosen number of steps
        public void regularStep(string comAddress, int baudRate, int nbStepX, int nbStepY, int nbStepZ)
        {
            //SendMessageHaptics(8, nbStepX, nbStepY, nbStepZ);
            // UnityEngine.Debug.Log("Message (regular step function) Sent");
            SendMessage(comAddress, 0, 0, baudRate, 2, nbStepX, nbStepY, nbStepZ);
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
            else
            {
                convertValue = "0000";
            }
            return convertValue;
        }

        public void SendMessageHaptics(int id, int value, int value2, int value3)
        {
            //UnityEngine.Debug.Log("ID-------------------------");
            //UnityEngine.Debug.Log(id);
            try
            {
                if (id >= 0 && id < 8)
                {
                    string message = id.ToString() + "," + convert(value);
                    //UnityEngine.Debug.Log("Message <8-------------------------");
                    //UnityEngine.Debug.Log(message);
                    port.WriteLine(message);
                    //UnityEngine.Debug.Log("Message Sent");
                }
                else if (id == 8)
                {
                    string message = id.ToString() + "," + convert(value) + "," + convert(value2) + "," + convert(value3);
                    //UnityEngine.Debug.Log("Message 8-------------------------");
                    //UnityEngine.Debug.Log(message);
                    port.WriteLine(message);
                }

            }
            catch (Exception)
            {
            }

        }

        public string ReadSerial()
        {

            string read_val = "";

            //port.ReadTimeout = 1;
            try
            {
                //UnityEngine.Debug.Log("Called");
                //UnityEngine.Debug.Log(port.ReadLine());
                read_val = port.ReadLine();
            }
            catch (Exception e)
            {
                //UnityEngine.Debug.Log("Serial Exception Caught!");
                //UnityEngine.Debug.Log(e);
            }
            return read_val;
        }

    }

}
