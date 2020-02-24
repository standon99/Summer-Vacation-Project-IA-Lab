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
    public string spNumber = "COM5";
    public string toWrite;
    public bool activateHpaticOptions_dataSetMappingDefault = false, discretizeAxis = false;

    int count;
    public int itr = 0, leftPos = 62, rightPos = 100;
    SerialPort sp;
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

        // Method to create a new axis instance
        public Axis(string name, int Pos1, int Pos2, int Encoder, int dataSetId)
        {
            Name = name; // COM port name
            pos1 = Pos1;
            pos2 = Pos2;
            encoder = Encoder;
            dataID = dataSetId;
        }

        // Method to send data to set the required positions on the axes
        public void setAxis(string name)//, int Pos1, int Pos2)
        {

            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            // asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //Wireless_axes wa = new Wireless_axes();
            // string[] portNo = name.Split('M');
            // int intPortNo = Int32.Parse(portNo[1]);

            //asar.SendMessage(name, pos1, pos2, baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);
            asar.MoveSlider(name, pos1, pos2, wa.baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);

        }

        public void toggleHapticsDataset(string name)
        {
            // WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            //asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //Wireless_axes wa = new Wireless_axes();
            asar.haptic(name, wa.baudRate);
        }

        public void discreteAxes(string name)
        {
            //WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            // asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            //  Wireless_axes wa = new Wireless_axes();
            asar.regularStep(name, wa.baudRate, wa.xSteps, wa.ySteps, wa.zSteps);
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
       btText = GetComponent<TextMeshPro>();
       sp = new SerialPort(spNumber, 115200);
       sp.ReadTimeout = 10;
       sp.WriteTimeout = 10;

        try
        {
           sp.Open();
            print("The port was opened!");
        }

        catch (Exception e)
        {
            Debug.Log("Couldn't open serial port: " + e.Message);
        }

        COMPortArray();
    }



    void Update()
    {
        if (itr == 0)
        {
            //sp.WriteLine("5");
        }
        if (count % 10 == 0)
        { 
            try
            {
                string readSerial = sp.ReadLine();

                // btText.text = readSerial;
                print(readSerial);
                print("////////");

            }
            catch (Exception f)
            {
                Debug.Log("read failed " + f.Message);
            }
        }
        count++;
        if (count > 10000)
        {
            count = 0;
        }
        // if (Input.GetKeyDown(KeyCode.Space))
        if (count % 10 == 0 && !activateHpaticOptions_dataSetMappingDefault)
        {
            //axesObjArray[u].setAxis(comPortAdd[u]);//, leftPos, rightPos);
           // axesObjArray[u].setAxis(comPortAdd[u]);

            try
            {
                if (leftPos < 4000)
                {
                    leftPos = leftPos+10;
                }
                else
                {
                    leftPos = 1000;
                }// Add process that will define what leftPos signifies
                rightPos =3222; // Add process that will define what rightPos signifies
               // sp.WriteLine(""+ rightPos.ToString() + "" + leftPos.ToString() + "0000");
                //wasr.SendMessage("COM6", 2000, 399, 115200, 0, 3, 3, 3);               // print("5" + rightPos.ToString() + "0" + leftPos.ToString() + "0000");
                //sp.WriteLine("9");
                //sp.WriteLine(rightPos.ToString());
                //sp.Flu;
                //print("Count is " + count.ToString());
                string message = messageCreator(leftPos, rightPos, 0, 3, 3, 3);
                sp.WriteLine(message);
            }
            catch (Exception e)
            {
                Debug.Log("send failed " + e.Message);
            }
            discretizeAxis = false;
        }
        else if (count % 10 == 0)
        {
            if (!discretizeAxis)
            {
                string message = messageCreator(leftPos, rightPos, 1, 3, 3, 3);
                UnityEngine.Debug.Log(message);
                sp.WriteLine(message);
            }
            else if (discretizeAxis)
            {
                string message = messageCreator(leftPos, rightPos, 2, 3, 3, 3);
                UnityEngine.Debug.Log(message);
                sp.WriteLine(message);
            }
        }
        if (count % 100 == 0)
        {
            try
            {
                sp.DiscardInBuffer();
                //print("Count is " + count.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("send failed " + e.Message);
            }
        }


    }

    //string readSerial = sp.ReadLine();


}
