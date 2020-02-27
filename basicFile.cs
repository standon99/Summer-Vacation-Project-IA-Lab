using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO.Ports;
using System;

public class basicFile : MonoBehaviour
// TO DO: Create event handler as in https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.datareceived?view=netframework-4.8
// for serial read - Arduino needs to send something special
// Then only needs to read when there is stuff on serial port.
// write only every certain number of frames
// read and write on same serial port.
// arduino to read string properly.
{
    TextMeshPro btText;
    //public string spNumber = "COM5";
    //public string toWrite;
    public bool activateHpaticOptions_dataSetMappingDefault = false, discretizeAxis = false;
    public string[] COMAddressesArray = { "COM11", "COM7" };//, "COM8" };//, "COM9" };//{"COM7", "COM5"}; // Was COM6
    public int[] hapticDataSets = {1, 2, 3};
    public Axis axisClass;
    int numAdd;

    public int xSteps = 3, ySteps = 3, zSteps = 3;
    public SerialPort[] sp = new SerialPort[10]; // Can change array length- assuming max 20 devices will be connected 

    int count;
    public int itr = 0, leftPos = 62;//, rightPos = 3222;

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

        private int sentRotaryValsHist;

        Wireless_axes wa = new Wireless_axes();

        public WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
        //public WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics bF = new basicFile.;
        basicFile ba = new basicFile();
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
        public void setAxis(SerialPort serialPort, int value1, int value2) //, int Pos1, int Pos2)
        {

            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            // asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //Wireless_axes wa = new Wireless_axes();
            // string[] portNo = name.Split('M');
            // int intPortNo = Int32.Parse(portNo[1]);

            //asar.SendMessage(name, pos1, pos2, baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);
            //asar.MoveSlider(name, pos1, pos2, wa.baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);

            string sentMsg = ba.messageCreator(value1, value2, 0, 4, 4, 4);
            //ba.sp[num].WriteLine(sentMsg);
            try
            {
                serialPort.WriteLine(sentMsg);
            }
            catch (Exception) { }
        }

        public void toggleHapticsDataset(SerialPort serialPort, int dataID)// have to incorporate dataID
        {
            // WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            //asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //Wireless_axes wa = new Wireless_axes();
            //asar.haptic(name, wa.baudRate);
            string sentMsg = ba.messageCreator(0, 0, 1, ba.xSteps, ba.ySteps, ba.zSteps);
            try
            {
                serialPort.WriteLine(sentMsg);
            }
            catch (Exception) { }
        }

        public void discreteAxes(SerialPort serialPort)
        {
            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            // asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //  Wireless_axes wa = new Wireless_axes();
            //asar.regularStep(name, wa.baudRate, wa.xSteps, wa.ySteps, wa.zSteps);
            string sentMsg = ba.messageCreator(0, 0, 2, ba.xSteps, ba.ySteps, ba.zSteps);
            try
            {
                serialPort.WriteLine(sentMsg);
            }
            catch (Exception) { }
        }

        // 
        public void readAxis()
        {
            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            //asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            string str = asar.ReadSerial();
            string[] numbers = str.Split(',');
            int string_len = numbers.Length;
            //Wireless_axes wa = new Wireless_axes();
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
        }

    }

    public string messageCreator(int value1, int value2, int hapticsStat, int xStep, int yStep, int zStep)
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
            string message = value1String + value2String + hapticsStat.ToString() + xStep.ToString() + yStep.ToString() + zStep.ToString();
            // Get a list of serial port names.
            return message;
        }
    }

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
        axisClass = new Axis();
        sp = axisClass.initiateCOMPorts(sp);
        
        numAdd = COMAddressesArray.Length;
        print("Length2 "+ numAdd);
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
        //int noAddress = COMAddressesArray.Length;
        //print("NUMADD = " + numAdd);
        for (int j = 0; j < numAdd; j++)
        { 
                try
                { 
                    string readSerial = sp[j].ReadLine();

                    // btText.text = readSerial;
                    print(readSerial);
                    print("////////");

                }
                catch (Exception f)
                {
                    Debug.Log("read failed " + f.Message);
                }
            
            count++;
            if (count > 10000)
            {
                count = 0;
            }
            // if (Input.GetKeyDown(KeyCode.Space))
            if (!activateHpaticOptions_dataSetMappingDefault)
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
                    axisClass.setAxis(sp[j], leftPos, rightPos);
                }
                catch (Exception e)
                {
                    Debug.Log("send failed " + e.Message);
                }
                discretizeAxis = false;
            }
            else
            {
                if (!discretizeAxis)
                {
                    //string message = messageCreator(leftPos, rightPos, 1, 3, 3, 3);
                    //UnityEngine.Debug.Log(message);
                    //sp[j].WriteLine(message);
                    axisClass.toggleHapticsDataset(sp[j], 1);
                }
                else if (discretizeAxis)
                {
                    //string message = messageCreator(leftPos, rightPos, 2, 3, 3, 3);
                    //UnityEngine.Debug.Log(message);
                    //sp[j].WriteLine(message);
                    axisClass.discreteAxes(sp[j]);
                }
            }
            if (count % 100 == 0)
            {
                try
                {
                    sp[j].DiscardInBuffer();
                    //print("Count is " + count.ToString());
                }
                catch (Exception e)
                {
                    Debug.Log("send failed " + e.Message);
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
    }

    //string readSerial = sp.ReadLine();


}
