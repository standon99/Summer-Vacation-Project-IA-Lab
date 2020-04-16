#if true
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO.Ports;
using System;

public class wirelessFaderAxisControlScript : MonoBehaviour
// TO DO: Create event handler as in https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.datareceived?view=netframework-4.8
// for serial read - Arduino needs to send something special
// Then only needs to read when there is stuff on serial port.
// write only every certain number of frames
// read and write on same serial port.
// arduino to read string properly.
// NOTE: Changing the names of public variabels in the Inspector pane of Unity, whicha are not passed as arguments to functions in the
// the axis class will not mean the changed variables are sent. The axis class creates an instance of this file within itself, and as such
// any changes to variables must be passed as function inputs, or manipulated within the axis class itself
{
    TextMeshPro btText;
    //public string spNumber = "COM5";
    //public string toWrite;
    public bool turnOffAllHaptics = false, discretizeAxis = false, followMode = false, runHapticsDemoMode = false, liveHapticsMode = true;
    public string[] COMAddressesArray = { "COM5" }; //{ "COM7", "COM11" , "COM13"};
    public int[] hapticDataSets = { 1, 2, 3 };
    public Axis axisClass;
    int numAdd;

    public int xSteps = 3, ySteps = 3, zSteps = 3;
    public SerialPort[] sp = new SerialPort[10]; // Can change array length- assuming max 20 devices will be connected 


    public int[] rightPositionsReceived;
    public int[] leftPositionsReceived;
    public int[] encodeValsReceived; // This array has the raw encoder values saved inside it
    public int firstReceiveFlag = 0;
    public int[] buttonpressStats;
    public int[] encodeValsSaved; // This array is where we store the last array value after it is added to previous values
    public int[] topSwitchStatSaved;
    private bool messg_rcv_stat = false;

    [Range(0, 9)]
    public int hapticsVal1 = 9;
    [Range(0, 9)]
    public int hapticsVal2 = 1;
    [Range(0, 4000)]
    public int followDist = 250;

    public GameObject sampleObj;
    public float x = 0, y = 0, z = 0;

    int count;
    public int itr = 0, leftPos = 62;//, rightPos = 3222;
    private string readSerial;

    [Range(0, 4024)]
    public int rightPos;
    //int noAddress = COMAddressesArray.Length;

    //SerialPort sp;
    public WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics wasr = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();

    public class Axis
    {
        public string Name { get; set; }
        //left slider (WITH ENCODER AWAY FROM BODY)
        public int pos1 { get; set; }
        //right slider
        public int pos2 { get; set; }
        // Button Press Value
        public int buttonPress { get; set; }
        // read val left slider
        public int readPos1 { get; set; }
        //read val right slider
        public int readPos2 { get; set; }
        //encoder
        public int encoder { get; set; }
        // property storing the data set mapped to the particular axis
        public int dataID { get; set; }

        private int[] sentRotaryValsHist;
        public int[] rightPositionsReceived;
        public int[] leftPositionsReceived;
        public int[] encodeValsReceived; // This array has the raw encoder values saved inside it
        public int firstReceiveFlag = 0;
        public int[] buttonpressStats;
        public int[] encodeValsSaved; // This array is where we store the last array value after it is added to previous values
        public int[] topSwitchSaved; // Variable to store top switch press statuses coming in from the ESP
        private bool messg_rcv_stat = false;

        Wireless_axes wa = new Wireless_axes();

        public WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
        //public WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics bF = new wirelessFaderAxisControlScript.;
        //wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
        // Method to create a new axis instance
        public Axis()//(string name, int Pos1, int Pos2, int Encoder, int dataSetId)
        {
            /*
            Name = name; // COM port name
            pos1 = Pos1;
            pos2 = Pos2;
            encoder = Encoder;
            dataID = dataSetId;
            */
        }

        public SerialPort[] initiateCOMPorts(SerialPort[] spArray)
        {
            wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
            int noAddress = ba.COMAddressesArray.Length;
            print("Length1 " + noAddress);
            for (int i = 0; i < noAddress; i++)
            {
                spArray[i] = new SerialPort(ba.COMAddressesArray[i], 115200);
                try
                {
                    spArray[i].Open();
                    spArray[i].ReadTimeout = 20;
                    spArray[i].WriteTimeout = 20;
                    print("Port " + ba.COMAddressesArray[i] + " is open");
                }
                catch (Exception)
                {
                    print("A PORT COULDNT OPEN");
                    print(ba.COMAddressesArray[i]);
                }
            }
            return spArray;
        }

        // Method to send data to set the required positions on the axes. 
        // Inputs: name - the COM address of the bluethooth port data should be sent on
        // Outputs: None, the function sends a message on the indicated COM port
        public void setAxis(SerialPort serialPort, int value1, int value2, int hapticsVal1, int hapticsVal2) //, int Pos1, int Pos2)
        {
            wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            // asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //Wireless_axes wa = new Wireless_axes();
            // string[] portNo = name.Split('M');
            // int intPortNo = Int32.Parse(portNo[1]);

            //asar.SendMessage(name, pos1, pos2, baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);
            //asar.MoveSlider(name, pos1, pos2, wa.baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);

            string sentMsg = ba.messageCreator(value1, value2, 0, 4, 4, 4, hapticsVal1, hapticsVal2);
            //ba.sp[num].WriteLine(sentMsg);
            try
            {
                serialPort.WriteLine(sentMsg);
            }
            catch (Exception) { }
        }

        // Thsi method is used to toggle teh haptics mapping mode- at this stage haptic data sets are saved on the ESP- but I'm working on allowing Unity to elicit haptic feedback live
        public void toggleHapticsDataset(SerialPort serialPort, int dataID, int hapticsVal1, int hapticsVal2)// have to incorporate dataID
        {
            // WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            //asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //Wireless_axes wa = new Wireless_axes();
            //asar.haptic(name, wa.baudRate);
            wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
            string sentMsg = ba.messageCreator(0, 0, 1, ba.xSteps, ba.ySteps, ba.zSteps, hapticsVal1, hapticsVal2);
            try
            {
                serialPort.WriteLine(sentMsg);
            }
            catch (Exception) { }
        }

        // This method splits the axis up into discrete points (can feel these as sort of bumps created using haptic feedback)
        public void discreteAxes(SerialPort serialPort, int hapticsVal1, int hapticsVal2)
        {
            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            // asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //  Wireless_axes wa = new Wireless_axes();
            //asar.regularStep(name, wa.baudRate, wa.xSteps, wa.ySteps, wa.zSteps);
            wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
            string sentMsg = ba.messageCreator(0, 0, 2, ba.xSteps, ba.ySteps, ba.zSteps, hapticsVal1, hapticsVal2);
            try
            {
                serialPort.WriteLine(sentMsg);
            }
            catch (Exception) { }
        }

        // This method toggles the following slider mode- the following distance is controlled by the follow dist variable
        public void toggleFollowMode(SerialPort serialPort, int dist, int hapticsVal1, int hapticsVal2)
        {
            int hapticsStat = 5;
            if (dist != 0)
            {
                if (dist > 4100)
                {
                    dist = 4100;
                }
                wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
                string sentMsg = ba.messageCreator(dist, 0, hapticsStat, 0, 0, 0, hapticsVal1, hapticsVal2);
                try
                {
                    serialPort.WriteLine(sentMsg);
                }
                catch (Exception) { }
            }
        }


        // Method reads teh ESP's messages detailing button press values and slider positions
        public void readAxis(int axis_num, string serialReadLine, int no_axes)
        {
            wirelessFaderAxisControlScript ba = new wirelessFaderAxisControlScript();
            //string str = asar.ReadSerial();
            string[] numbers = serialReadLine.Split(',');
            int string_len = numbers.Length;
            //print("String length is   " + string_len);
            if (firstReceiveFlag == 0)
            {
                //int no_axes = string_len / 4;

                rightPositionsReceived = new int[no_axes];
                leftPositionsReceived = new int[no_axes];
                encodeValsReceived = new int[no_axes];
                buttonpressStats = new int[no_axes];
                sentRotaryValsHist = new int[no_axes];
                encodeValsSaved = new int[no_axes];
                topSwitchSaved = new int[no_axes];
                firstReceiveFlag = 1;
            }
            print(numbers[0] + ", " + numbers[1] + ", " + numbers[2] + ", " + numbers[3] + ", " + numbers[4]);
            rightPositionsReceived[axis_num] = Int32.Parse(numbers[0]);
            leftPositionsReceived[axis_num] = Int32.Parse(numbers[1]);
            buttonpressStats[axis_num] = Int32.Parse(numbers[2]);
            encodeValsReceived[axis_num] = Int32.Parse(numbers[3]);
            topSwitchSaved[axis_num] = Int32.Parse(numbers[4]);

            // The control loops below allow the encoder values to stack from +infinity to - infinity (esp just gives values -24 to 24)
            if (((sentRotaryValsHist[axis_num] - encodeValsReceived[axis_num]) > 5) || (encodeValsReceived[axis_num] - sentRotaryValsHist[axis_num] > 5))
            {
                if (sentRotaryValsHist[axis_num] > encodeValsReceived[axis_num])
                {
                    encodeValsSaved[axis_num] += encodeValsReceived[axis_num];
                    encodeValsSaved[axis_num] += 24 - sentRotaryValsHist[axis_num];
                }
                else
                {
                    encodeValsSaved[axis_num] -= 24 - encodeValsReceived[axis_num];
                    encodeValsSaved[axis_num] -= sentRotaryValsHist[axis_num];
                }
            }
            else
            {
                if (sentRotaryValsHist[axis_num] > encodeValsReceived[axis_num])
                {
                    encodeValsSaved[axis_num] += sentRotaryValsHist[axis_num] - encodeValsReceived[axis_num];
                }
                else
                {
                    encodeValsSaved[axis_num] -= encodeValsReceived[axis_num] - sentRotaryValsHist[axis_num];
                }
            }
            sentRotaryValsHist[axis_num] = encodeValsReceived[axis_num];
            //print(encodeValsSaved[axis_num]);

            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            //asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();

            //Wireless_axes wa = new Wireless_axes();

            /*
            wa.axesObjArray[Int32.Parse(numbers[0])].readPos1 = Int32.Parse(numbers[1]);
            wa.axesObjArray[Int32.Parse(numbers[0])].readPos2 = Int32.Parse(numbers[2]);
            int encoderInitialVal = Int32.Parse(numbers[3]);
            int buttonPressStat = Int32.Parse(numbers[4]);
            if (((sentRotaryValsHist - encoderInitialVal) > 5) || (encoderInitialVal - sentRotaryValsHist > 5))
            {
                if (sentRotaryValsHist > encoderInitialVal)
                {
                    wa.axesObjArray[Int32.Parse(numbers[0])].encoder += encoderInitialVal;
                    wa.axesObjArray[Int32.Parse(numbers[0])].encoder += 24 - sentRotaryValsHist;
                }
                else
                {
                    wa.axesObjArray[Int32.Parse(numbers[0])].encoder -= 24 - encoderInitialVal;
                    wa.axesObjArray[Int32.Parse(numbers[0])].encoder -= sentRotaryValsHist;
                }
            }
            else
            {
                if (sentRotaryValsHist > encoderInitialVal)
                {
                    wa.axesObjArray[Int32.Parse(numbers[0])].encoder += sentRotaryValsHist - encoderInitialVal;
                }
                else
                {
                    wa.axesObjArray[Int32.Parse(numbers[0])].encoder -= encoderInitialVal - sentRotaryValsHist;
                }
            }
            sentRotaryValsHist = encoderInitialVal;
            */
        }

    }


    // Function to create and format messages for you- the output string can be directly sent through the COM port
    public string messageCreator(int value1, int value2, int hapticsStat, int xStep, int yStep, int zStep, int hapticsVal1, int hapticsVal2)//int hapticsDataSetID, int hapticsDataSetIDTwo)
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
            else if (value1 >= 1000 && value1 <= 4025)
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
            else if (value2 >= 1000 && value2 <= 4025)
            {
                value2String = "" + value2String;
            }
            // Sent message gives first pos value, second pos value and whether haptics need to be engaged
            string message = value1String + value2String + hapticsStat.ToString() + xStep.ToString() + yStep.ToString() + zStep.ToString() + hapticsVal1.ToString() + hapticsVal2.ToString();//hapticsDataSetID.ToString() + hapticsDataSetIDTwo.ToString();
            // Get a list of serial port names.
            return message;
        }
    }

    // Function that finds all availible COM ports
    string[] COMPortArray()
    {
        string[] ports = SerialPort.GetPortNames();

        Console.WriteLine("The following serial ports were found:");

        // Display each port name to the console.
        foreach (string port in ports)
        {
            print(port);
        }
        return ports;
    }

    void Start()
    {
        /*
        sp = new SerialPort(spNumber, 115200);
        sp.Open();
        sp.ReadTimeout = 10;
        sp.WriteTimeout = 10;
        */

        sampleObj = GameObject.Find("Cube");

        axisClass = new Axis();
        sp = axisClass.initiateCOMPorts(sp);

        numAdd = COMAddressesArray.Length;
        print("Length2 " + numAdd);
        for (int i = 0; i < numAdd; i++)
        {
            print(sp[i]);
            /*
            sp[i] = new SerialPort(addressesArray[i], 115200);
            sp[i].Open();
            sp[i].ReadTimeout = 10;
            sp[i].WriteTimeout = 10;
            */
        }
        /*
        try
        {
           //sp.Open();
            print("The port was opened!");
        }

        catch (Exception e)
        {
            Debug.Log("Couldn't open serial port: " + e.Message);
        }
        */
        COMPortArray();
    }



    void Update()
    {
        if (itr == 0)
        {
            int noAddress = COMAddressesArray.Length;
            encodeValsReceived = new int[noAddress];
            rightPositionsReceived = new int[noAddress]; 
            leftPositionsReceived = new int[noAddress]; 
            encodeValsReceived = new int[noAddress];  // This array has the raw encoder values saved inside it
            buttonpressStats = new int[noAddress]; 
            encodeValsSaved = new int[noAddress];  // This array is where we store the last array value after it is added to previous values
            topSwitchStatSaved = new int[noAddress];
}
        //print("NUMADD = " + numAdd);

        for (int j = 0; j < numAdd; j++)
        {
            try
            {
                readSerial = sp[j].ReadLine();
               //print(readSerial);
                //print("////////");
                messg_rcv_stat = true;
                // btText.text = readSerial;
            }
            catch (Exception f)
            {
                messg_rcv_stat = false;
                //Debug.Log("read failed " + f.Message);
            }

            // Save the values from the serial read in the axis class as a part of the file, for easier access
            if (messg_rcv_stat == true)
            {
                axisClass.readAxis(j, readSerial, numAdd);
                rightPositionsReceived[j] = axisClass.rightPositionsReceived[j];
                leftPositionsReceived[j] = axisClass.leftPositionsReceived[j];
                encodeValsReceived[j] = axisClass.encodeValsReceived[j]; // This array has the raw encoder values saved inside it
                firstReceiveFlag = axisClass.firstReceiveFlag;
                buttonpressStats[j] = axisClass.buttonpressStats[j];
                encodeValsSaved[j] = axisClass.encodeValsSaved[j]; // This array is where we store the last array value after it is added to previous values
                topSwitchStatSaved[j] = axisClass.topSwitchSaved[j];
            }

            // This checks to see if you wish to have haptics enabled at all
            if (turnOffAllHaptics)
            {
                hapticsVal1 = 0;
                hapticsVal2 = 0;
                liveHapticsMode = false;
                runHapticsDemoMode = false;
            }
            else if (liveHapticsMode)
            {
                // This is some sample code (gives feedback when we are two different psoitions for each slider), the haptics values can be modifed as needed based on the what valeus you need and what conditions are needed to satisfy them 
                // NOTE: The left positions are correlated with sldier 2- so hapticsVal2 must be modified to create feedback on that slider, and right positions correlate with  slider 1, so to control haptic feedback on that slider change the  variable hapticVal1
                if (Math.Abs(leftPositionsReceived[j] - 500) <= 80)
                {
                    hapticsVal2 = 9;
                }
                else
                {
                    hapticsVal2 = 0;
                }

                if (Math.Abs(rightPositionsReceived[j] - 900) <= 80)
                {
                    hapticsVal1 = 2;
                }
                else
                {
                    hapticsVal1 = 0;
                }
                runHapticsDemoMode = false;
            }
            else runHapticsDemoMode = true;

            count++; // Iterate the frame count variable, to determine when we want to wipe the stack

            if (count > 10000)
            {
                count = 0;
            }
            // if (Input.GetKeyDown(KeyCode.Space))
            if (!followMode && !discretizeAxis)
            {
                //axesObjArray[u].setAxis(comPortAdd[u]);//, leftPos, rightPos);
                // axesObjArray[u].setAxis(comPortAdd[u]);
                try
                {
                    if (leftPos < 4000)
                    {
                        leftPos = leftPos + 20;
                    }
                    else
                    {
                        leftPos = 0;
                    }// Add process that will define what leftPos signifies
                     // Add process that will define what rightPos signifies
                     // sp.WriteLine(""+ rightPos.ToString() + "" + leftPos.ToString() + "0000");
                     //wasr.SendMessage("COM6", 2000, 399, 115200, 0, 3, 3, 3);               // print("5" + rightPos.ToString() + "0" + leftPos.ToString() + "0000");
                     //sp.WriteLine("9");
                     //sp.WriteLine(rightPos.ToString());
                     //sp.Flu;
                     //print("Count is " + count.ToString());

                    //string message = messageCreator(leftPos, rightPos, 0, 3, 3, 3);
                    //sp[j].WriteLine(message)
                    axisClass.setAxis(sp[j], leftPos, rightPos, hapticsVal1, hapticsVal2);
                }
                catch (Exception e)
                {
                    Debug.Log("send failed " + e.Message);
                }
                discretizeAxis = false;
            }
            //else if (activateHapticDSMapping) axisClass.toggleHapticsDataset(sp[j], 1, hapticsVal1, hapticsVal2);
            else if (discretizeAxis) axisClass.discreteAxes(sp[j], hapticsVal1, hapticsVal2);
            else if (followMode) axisClass.toggleFollowMode(sp[j], followDist, hapticsVal1, hapticsVal2);
           

            if (count % 6 == 0)
            {
                try
                {
                    sp[j].DiscardInBuffer(); // If we dont clear buffer frequently, it get loaded up with old commands from the ESP
                    //print("Count is " + count.ToString());
                }
                catch (Exception e)
                {
                    //Debug.Log("send failed " + e.Message);
                }
            }
        }
        itr++;
        if (Input.GetKeyDown("space"))
        {
            for (int m = 0; m < numAdd; m++)
            {
                sp[m].Close();
            }
        }

        /*
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.H))
        {
            sampleObj.transform.position = sampleObj.transform.position + new Vector3(0,0.02f,0);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.J))
        {
            sampleObj.transform.position = sampleObj.transform.position + new Vector3(0, -0.02f,0);
        }
        */
    }

    //string readSerial = sp.ReadLine();


}
#endif