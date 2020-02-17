/**************************************************************************************
** Monash University - Faculty of Information Technology - Immersive Analytics Lab   **
** Version 3.0 - January 2020                                                        **
***************************************************************************************

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 * TO DO (SIDDHANT)/CHANGE LOG:
 * - 
 * 
 * 
 * 
 -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 Code Structure:
 For each new axis that is switched on, a new COM port appears. Each new COM port subsequently is added into an array in this script.
 Each COM port instance is tied to an instance of the Axis class
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

#if true // Directive to allow file to stop compiling in Unity- for debugging purposes
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using ArduinoSlidesAndRotary; // Contains asar class
using System;
using Leap; // Library for interfacing with LEAP motion controller
using System.Diagnostics; // Contains Stopwatch class
using System.Threading;
using System.Reflection;
using System.IO.Ports;

public class Wireless_axes : MonoBehaviour
{
    // Add controller object to the program
    Controller controller;
    [SerializeField] VirtualFaderAxis _virtualFaderAxisTwo;

    public bool toggleHaptics = false, discretizeAxes = false, hapticTracking = true, drawCube = false;

    public int itr = 0, xSteps = 4, ySteps = 4, zSteps = 4;
    public int baudRate = 2000000;
    // Elapsed Time
    public float timeElapsed;
    public int index = 0, i = 0;


    public int time, handInFrameT = 0;
    public float[] sent_data = new float[3];
    // Create new stopwatch object
    public Stopwatch stopwatch = new Stopwatch();

    public int[] yAxis = { 0, 1 }, xAxis = { 3, 2 }, zAxis = { 5, 4 };

    public int[] sentPositions;
    public int[] sentRotaryVals;
    public int[] sentRotaryValsHist;
    public int[] sentButtonVals;
    public int setter = 1;

    // Array for current COM port addresses that are open
    string[] comPortAdd;
    // Old COM port adress (from last frame)
    string[] comPortAddOld;


    public int x0 = 0;
    WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;

    /* Function to compute the velocity of a point ALONG AN AXIS given its present position, past position, and time between measurements
    * Inputs:
    * - current_pos - Current position in desired axis
    * - old_pos - Position of point along an axis in the last frame
    * - timeSpan - Time taken for the program to compute each frame
    * 
    * Outputs:
    * - velocity- Velocity along the desired axis
    * 
    * Function:
    * Computes velocity of a point/body along an axis defined in the Leap Motion Controller
    */

    float velocity(float current_pos, float old_pos, float timeSpan)
    {
        float velocity = 0;
        if (timeSpan == 0)
        {
            velocity = 0;
        }
        else
        {
            velocity = (current_pos - old_pos) / timeSpan;
            //print(fingerTipVelocity);
        }
        return velocity;
    }

    public void serial_Read()
    {
        string str = asar.ReadSerial();
        string[] numbers = str.Split(',');
        int string_len = numbers.Length;
        int cntr = 0;
        if (string_len > 2)
        {
            int nbSliders = string_len / 2;
            print("-----------------------");
            print(string_len);
            int ind = nbSliders;// * 3;

            if (setter == 1)
            {
                sentPositions = new int[ind];
                sentRotaryVals = new int[ind / 2];
                sentRotaryValsHist = new int[ind / 2];
                for (int b = 0; b < ind / 2; b++)
                {
                    sentRotaryVals[b] = 0;
                    sentRotaryValsHist[b] = 0;
                }
                sentButtonVals = new int[ind / 2];

                setter = 0;
            }

            //int cntr = 0;

            for (int u = 0; u < (string_len / 2); u++)
            {
                sentPositions[u] = Int32.Parse(numbers[u]);
                //print("POS VAL");
                print(sentPositions[u]);
                cntr++;
            }

            cntr = 0;
            for (int m = string_len / 2; m < (string_len); m = m + 2)
            {
                if (Int32.Parse(numbers[m]) != 255)
                {
                    if ((sentRotaryValsHist[cntr] - Int32.Parse(numbers[m]) > 5) || (Int32.Parse(numbers[m]) - sentRotaryValsHist[cntr] > 5))
                    {

                        if (sentRotaryValsHist[cntr] > Int32.Parse(numbers[m]))
                        {
                            sentRotaryVals[cntr] += Int32.Parse(numbers[m]);
                            sentRotaryVals[cntr] += 24 - sentRotaryValsHist[cntr];
                        }
                        else
                        {
                            sentRotaryVals[cntr] -= 24 - Int32.Parse(numbers[m]);
                            sentRotaryVals[cntr] -= sentRotaryValsHist[cntr];
                        }
                    }
                    else
                    {
                        if (sentRotaryValsHist[cntr] > Int32.Parse(numbers[m]))
                        {
                            sentRotaryVals[cntr] += sentRotaryValsHist[cntr] - Int32.Parse(numbers[m]);
                        }
                        else
                        {
                            sentRotaryVals[cntr] -= Int32.Parse(numbers[m]) - sentRotaryValsHist[cntr];
                        }
                    }
                    sentRotaryValsHist[cntr] = Int32.Parse(numbers[m]);
                }
                else
                {
                    sentRotaryVals[cntr] = 66666; // If not reading proper rotary values set error code 5000 (to be discarded in future computations)
                }
                //print("ROTARY VAL");
                print(sentRotaryVals[cntr]);
                cntr++;
            }
            cntr = 0;
            for (int n = string_len / 2 + 1; n <= (string_len); n = n + 2)
            {
                sentButtonVals[cntr] = Int32.Parse(numbers[n]);
                if (sentButtonVals[cntr] == 255)
                {
                    sentButtonVals[cntr] = 5555; // If not connected to a button, set the button value as 5555 (to be discarded in future computations)
                }
                //print("BUTTON VAL");
                print(sentButtonVals[cntr]);
                ///print("++++++++++++++++++++");
                cntr++;
            }
            //print(str);
            // asar.DoPortRead();
        }

        ///// Rotating sample cube
        //float rot_val = (360/24)/2 * sentRotaryVals[1];
        string[] slider_ids = new string[6] { "Z_AXIS_MIN", "Z_AXIS_MAX", "X_AXIS_MIN", "X_AXIS_MAX", "Y_AXIS_MIN", "Y_AXIS_MAX" };
        /*
        for (int m = 0; m < sentPositions.Length; m++)
        {
            if (x0 != sentPositions[m])
            {
                x0 = sentPositions[m];
                float v = (float)(4.027559 * x0) / 1023.0f;

                // TAKE VALUES WITH REFERENCE TO MIDDLE OF THE SENT POSITIONS ARRAY (sentPositions.Length/2)
                if (m == 0)
                {
                    _virtualFaderAxisTwo.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Z_AXIS_MIN, v);
                }
                else if (m == sentPositions.Length - 2)
                {
                    _virtualFaderAxisTwo.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Z_AXIS_MAX, v);
                }
                else if (m == sentPositions.Length - 4)
                {
                    _virtualFaderAxisTwo.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.X_AXIS_MIN, v);
                }
                else if (m == sentPositions.Length - 5)
                {
                    _virtualFaderAxisTwo.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.X_AXIS_MAX, v);
                }
                else if (m == sentPositions.Length-3)
                {
                    _virtualFaderAxisTwo.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Y_AXIS_MIN, v);
                }
                else if (m == sentPositions.Length- 1)
                {
                    _virtualFaderAxisTwo.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Y_AXIS_MAX, v);
                }
                
            }
        }
        */

        //ObjectRotation(cube1, rot_val, 0, 0);
        //return sentPositions;
        Thread.Sleep(0);
    }

    public void initialzeDevices(int baudRate)
    {
        SerialPort port;

        for (int h = 0; h < comPortAdd.Length; h++)
        {
            port = new SerialPort(comPortAdd[h], baudRate);
            try
            {
                port.Open();
                string message = h;
                port.WriteLine(message);
                port.Close();
            }
            catch (Exception e)
            {
                //UnityEngine.Debug.Log("Port Open Failed (Function)" + y.ToString());
            }
        }
    }

    public class Axis
    {
        public string Name {get; set;}
        //left slider (WITH ENCODER AWAY FROM BODY)
        public int pos1 {get; set;}
        //right slider
        public int pos2 {get; set;}
        // Button Press Value
        public int buttonPress  {get; set;}
        // read val left slider
        public int readPos1 {get; set;}
        //read val right slider
        public int readPos2 {get; set;}
        //encoder
        public int encoder {get; set;}
        // property storing the data set mapped to the particular axis
        public int dataID {get; set;}

        private int sentRotaryValsHist;

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
            WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            Wireless_axes wa = new Wireless_axes();
            string[] portNo = name.Split('M');
            int intPortNo = Int32.Parse(portNo[1]);
            int baudRate = 2000000;
            asar.SendMessage(name, pos1, pos2, baudRate, 0, wa.xSteps, wa.ySteps, wa.zSteps);
        }

        public void toggleHapticsDataset(string name)
        {
            WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            Wireless_axes wa = new Wireless_axes();
            asar.haptic(name, wa.baudRate);
        }

        public void discreteAxes(string name)
        {
            WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            Wireless_axes wa = new Wireless_axes();
            asar.regularStep(name, wa.baudRate, wa.xSteps, wa.ySteps, wa.zSteps);
        }
        
        // 
        public void readAxis()
        {
            WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics asar;
            asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
            string str = asar.ReadSerial();
            string[] numbers = str.Split(',');
            int string_len = numbers.Length;
            Wireless_axes wa = new Wireless_axes();
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

    public Axis[] axesObjArray = new Axis[100];

    public void createAxesObj(int o)
    {
       axesObjArray[o] = new Axis(comPortAdd[o],0,0,0, 1);
    }

    void Start()
    {
        // Define new serial port, and open it for communication
        asar = new WirelessArduinoSlidesAndRotary.ArduinoReaderHaptics();
        baudRate = 2000000;
        comPortAdd = asar.findCOMPorts(baudRate);
        for (int o = 0; o < comPortAdd.Length; o++)
        {
            createAxesObj(o);
        }
        initialzeDevices(baudRate);
    }

    // Update is called once per frame
    void Update()
    {
        if (itr % 50 == 0)
        {
            comPortAddOld = comPortAdd;
            comPortAdd = asar.findCOMPorts(baudRate);
            for (int k = 0; k < comPortAdd.Length; k++)
            {
                if (comPortAdd[k] != comPortAddOld[k])
                {
                    if (comPortAdd[k] == "NULL")
                    {
                        // If the 
                        comPortAdd[k] = comPortAddOld[k];
                    }
                    else createAxesObj(k);
                }
            }
        }
        if (!toggleHaptics)
        {
            for (int u = 0; u < comPortAdd.Length; u++)
            {
                int leftPos = 500; // Add process that will define what leftPos signifies
                axesObjArray[u].pos1 = leftPos;
                int rightPos = 500; // Add process that will define what rightPos signifies
                axesObjArray[u].pos2 = rightPos;
                axesObjArray[u].setAxis(comPortAdd[u]);//, leftPos, rightPos);
            }
            discretizeAxes = false;
        }
        else
        {
            if (!discretizeAxes)
            {
                for (int l = 0; l < comPortAdd.Length; l++)
                {
                    axesObjArray[l].toggleHapticsDataset(comPortAdd[l]);
                }
            }
            else if (discretizeAxes)
            {
                for (int n = 0; n < comPortAdd.Length; n++)
                {
                    axesObjArray[n].discreteAxes(comPortAdd[n]);
                }
            }
        }
        
        
      
        

        //// Create a seprate thread to run Serial reader on (prevents lag)
        //Thread serial_reader_t = new Thread(new ThreadStart(serial_Read));
        //serial_reader_t.Start();
        ////serial_reader_t.Join();

        itr++;
    }   
}
#endif