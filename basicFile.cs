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
    int count;
    public int itr = 0;
    SerialPort sp;

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

    // Get a list of serial port names.
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
                // Debug.Log("read failed " + f.Message);
            }
        }
        count++;
        if (count > 10000)
        {
            count = 0;
        }
        // if (Input.GetKeyDown(KeyCode.Space))
        if (count % 10 == 0)
        {
            //axesObjArray[u].setAxis(comPortAdd[u]);//, leftPos, rightPos);
           // axesObjArray[u].setAxis(comPortAdd[u]);

            try
            {
                int leftPos = 58; // Add process that will define what leftPos signifies
                int rightPos = 100; // Add process that will define what rightPos signifies
                sp.WriteLine("0"+ rightPos.ToString() + "00" + leftPos.ToString() + "0000");
                                    // print("5" + rightPos.ToString() + "0" + leftPos.ToString() + "0000");
                //sp.WriteLine("9");
                //sp.WriteLine(rightPos.ToString());
                //sp.Flu;
                //print("Count is " + count.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("send failed " + e.Message);
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
