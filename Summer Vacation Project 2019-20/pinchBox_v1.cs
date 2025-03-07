﻿/**************************************************************************************
** Monash University - Faculty of Information Technology - Immersive Analytics Lab   **
** Version 3.0 - January 2020                                                        **
***************************************************************************************

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 * TO DO (SIDDHANT)/CHANGE LOG:
 * - Smoothing of input (Kalman filter) --- DONE --- 2/12/19
 * - Only send data if present position is different to previous position --- DONE --- 28/11/19
 * - Fix so both hands can be used --- DONE IO EXCEPTION ERROR --- FIXED --- 29/11/19
 * - PID in Arduino --- MODIFIED VERSION INCORPORATED --- 2/12/19
 * - Restrict amount of data sent through serial port (no unnecessary data sends).
 * - Map movement distance of fingers to movement of fader in a 1:1 ratio --- DONE (normalization function altered) --- 2/12/19
 * - Gesture Recognition --- Pinch is now registered (index finger and thumb tips pushed together) --- 3/12/2019
 * - Gesture Recognition --- Can now terminate gesture tracking by closing fist --- 4/12/2019
 * - Gesture Recognition --- Tracking center of hand gesture area
 * - Gain Function --- Increase precision the higher you go (min 100 and max 300 due to accuracy issues) --- DRAFTED but not functioning optimally (have to address issues) --- 4/12/2019 
 * - Triple axis integration --- DONE --- Faders are slow/jittery- will be fixed when new ones are used -- DONE
 * _Reading data sent from Arduinos over the Serial Port --- DRAFTED (checking for bugs)
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

public class pinchBox_v1 : MonoBehaviour
{
    // Add controller object to the program
    Controller controller;
    [SerializeField] VirtualFaderAxis _virtualFaderAxisTwo;
    //private GameObject cube1;///////////// creating sample cube game object
    private GameObject[] slidersArray;

    private bool full_vol = false, hands_out_of_view = false;
    private bool interaction_box = true;
    public bool trackFinger = true, pinchBool = true, applyKalmanFiltering = true;
    public bool trackPalm = false, freeze = false, gainFunction = false, toggleHaptics = false, discretizeAxes = false, hapticTracking = true, drawCube = false;

    public int itr = 0, xSteps = 4, ySteps = 4, zSteps = 4;

    // Position vectors for trackable objects
    private Vector handCenter, fingerTip, thumbTip, middleFingerTip, thumbJoint, indexJoint;

    // Vectors describing position/velocity of the midpoint between thumb joint and index finger joint
    private Vector3 midPThumbIndex;
    private Vector3[] old_midPThumbIndex = new Vector3[2];

    // Velocity for trackable objects ALONG X AXIS ONLY
    private Vector handCenterV;
    private float[] old_pos_x_hand = new float[2];
    private float x_axis_hand;
    private float[,] fingerTipV_x = new float[2, 5], fingerTipV_z = new float[2, 5], fingerTipV_y = new float[2, 5], thumbTipV = new float[2, 5], indexThumbMidV_X = new float[2, 5], indexThumbMidV_Y = new float[2, 5], indexThumbMidV_Z = new float[2, 5];
    private float[] oldFingerTipV = new float[2];
    private float[] oldThumbTipV = new float[2];

    // Elapsed Time
    public float timeElapsed;
    public int index = 0, i = 0;

    // Along x axis of LEAP motion module
    //public int[] pos_x;
    public float x_axis_index = 0, x_axis_thumb = 0;
    public float[] old_pos_x_index = new float[2], old_pos_x_thumb = new float[2];

    // Vertical axis from LEAP motion module
    public float y_axis_index = 0;
    public float[] old_pos_y_index = new float[2];

    public float y_axis_thumb = 0;
    public float[] old_pos_y_thumb = new float[2];

    // Z axis from LEAP motion module
    public float z_axis_index = 0;
    public float[] old_pos_z_index = new float[2];

    public float z_axis_thumb = 0;
    public float[] old_pos_z_thumb = new float[2];

    // Kalman filtered data value
    public float[] filtered_x = new float[2], filtered_y = new float[2], filtered_z = new float[2];

    public float[] old_filtered_x = { 10, 10 }, old_filtered_y = { 10, 10 }, old_filtered_z = { 10, 10 };

    public float[] old_p_x_xaxis = { 10, 10 }, old_p_x_yaxis = { 10, 10 }, old_p_x_zaxis = { 10, 10 };


    // Vertical axis from LEAP motion module
    // public int pos_y;

    public int time, handInFrameT = 0;
    public float[] sent_data = new float[3];
    // Create new stopwatch object
    public Stopwatch stopwatch = new Stopwatch();

    // Arduino COM Port for Serial Communication
    public string COM = "COM16";

    // Variable to store gesture type
    public string gesture;
    public int[,] pinchHistory = new int[2, 5];
    public float pinchAvg = 0;
    public float[,] pinchDistHistory = new float[2, 5];
    public int[] yAxis = { 0, 1 }, xAxis = { 3, 2 }, zAxis = { 5, 4 };

    public int[] sentPositions;
    public int[] sentRotaryVals;
    public int[] sentRotaryValsHist;
    public int[] sentButtonVals;
    public int setter = 1;

    public int x0 = 0;
    ArduinoSlidesAndRotary.ArduinoReaderHaptics asar;

    float virtualSliderManipulate(int sentVal)
    {
        float lowestVal = 0.0044f;
        float highestVal = 0.11f;
        float reqPosition = ((highestVal- lowestVal) *sentVal/255) + lowestVal;
        return reqPosition;
    }

    GameObject[] virtualSliderInitialize()
    {
        GameObject xMin = GameObject.Find("XMinSlider");
        GameObject yMin = GameObject.Find("YMinSlider");
        GameObject zMin = GameObject.Find("ZMinSlider");
        GameObject xMax = GameObject.Find("XMaxSlider");
        GameObject yMax = GameObject.Find("YMaxSlider");
        GameObject zMax = GameObject.Find("ZMaxSlider");
        GameObject[] slidersArray = {xMin, yMin, zMin, xMax, yMax, zMax};
        return slidersArray;
    }
    /* Function to identify hand gestures
     * Inputs:
     * - thumbTip - Position of tip of thumb
     * - fingerTip - Position of index finger tip
     * - palmPos - position of center of the palm of the hand
     * - middleFingerTip- Position vector of the tip of the middle finger
     * - indexFinger - Finger object for the index finger
     * - ringFinger - Finger object for the ring finger
     * - littleFinger - Finger object for the little finger
     * Outputs:
     * - gesture- A string storing the name of the identified gesture, can be either "pinch", 
     * "fist" or "x/y/z point".
     * Function: 
     * Accpts the relevant parameters- finds absolute distance from the tip of the index finger 
     * to the tip of the thumb. Directions of other fingers are also considered. If a fist (when finegrs clenched)
     * or if  other fingers not pointing in similar directions, assumed not to be pinch.
     * If dist. between thumb tip and finger tip is smaller than 45, we are assumed to making a pinch gesture (provided
     * above criteria also met)
     */
    string gestureRecognition(Vector thumbTip, Vector fingerTip, Vector palmPos, Vector middleFingerTip, Finger indexfinger, Finger ringFinger, Finger littleFinger)
    {
        // String to store measured gesture type
        string gesture = "none";

        if (handInFrameT > 100)
        {
            // Summation of pinch values for calculating the average
            float pinchSum = 0;

            /*
             * Finds the 
            float max = (float)1.3;
            float min = (float)0.7;
            bool t_f_x = ((Math.Abs(thumbTip.x) > Math.Abs((min * fingerTip.x))) && (Math.Abs((thumbTip.x)) < (max * Math.Abs(fingerTip.x))));
            bool t_f_y = ((Math.Abs(thumbTip.y) > (min * Math.Abs(fingerTip.y))) && (Math.Abs(thumbTip.y) < (max * Math.Abs(fingerTip.y))));
            bool t_f_z = ((Math.Abs(thumbTip.z) > (0 * Math.Abs(fingerTip.z))) && (Math.Abs(thumbTip.z) < (3 * Math.Abs(fingerTip.z))));
            //print(t_f_x);
            //print(t_f_y);
            //print(t_f_z);
            */

            Vector littleFingerPos = littleFinger.TipPosition;
            Vector ringFingerPos = ringFinger.TipPosition;

            // Find absolute distance between index and middle finger tips- using Pythagoras' theorem in 3D
            float fingerDist = (float)Math.Sqrt(Math.Pow(((double)thumbTip.x - (double)fingerTip.x), 2) + Math.Pow(((double)thumbTip.y - (double)fingerTip.y), 2) + Math.Pow(((double)thumbTip.z - (double)fingerTip.z), 2));

            // Use Pythag. in 3D to find absolute distance between middle finger and thumb
            float middleFingerPalmDist = (float)Math.Sqrt(Math.Pow(((double)middleFingerTip.x - (double)palmPos.x), 2) + Math.Pow(((double)middleFingerTip.y - (double)palmPos.y), 2) + Math.Pow(((double)middleFingerTip.z - (double)palmPos.z), 2));

            // Use Pythag. in 3D to find absolute distance between index finger and palm
            float indexFingerPalmDist = (float)Math.Sqrt(Math.Pow(((double)fingerTip.x - (double)palmPos.x), 2) + Math.Pow(((double)fingerTip.y - (double)palmPos.y), 2) + Math.Pow(((double)fingerTip.z - (double)palmPos.z), 2));

            // Use Pythag. in 3D to find absolute distance between little finger and palm 
            float littleFPDist = (float)Math.Sqrt(Math.Pow(((double)littleFingerPos.x - (double)palmPos.x), 2) + Math.Pow(((double)littleFingerPos.y - (double)palmPos.y), 2) + Math.Pow(((double)littleFingerPos.z - (double)palmPos.z), 2));
            //print(littleFPDist);
            // Use Pythag. in 3D to find absolute distance between ring finger and palm 
            float ringFPDist = (float)Math.Sqrt(Math.Pow(((double)ringFingerPos.x - (double)palmPos.x), 2) + Math.Pow(((double)ringFingerPos.y - (double)palmPos.y), 2) + Math.Pow(((double)ringFingerPos.z - (double)palmPos.z), 2));
            //print(ringFPDist);

            // Define vectors holding the direction each finger points in
            Vector indexFingerDir = indexfinger.Direction;
            Vector ringFingerDir = ringFinger.Direction;
            Vector littleFingerDir = littleFinger.Direction;

            bool xPass = false, yPass = false, zPass = false;
            if ((Math.Abs(ringFingerDir.x) >= Math.Abs((0 * littleFingerDir.x))) && (Math.Abs(ringFingerDir.x) <= Math.Abs(2.2 * littleFingerDir.x))) xPass = true; //print(xPass);
            if ((Math.Abs(ringFingerDir.y) >= Math.Abs((0 * littleFingerDir.y))) && (Math.Abs(ringFingerDir.y) <= Math.Abs(2.2 * littleFingerDir.y))) yPass = true; //print(yPass);
            if ((Math.Abs(ringFingerDir.z) >= Math.Abs((0 * littleFingerDir.z))) && (Math.Abs(ringFingerDir.z) <= Math.Abs(2.2 * littleFingerDir.z))) zPass = true; //print(zPass); print("----");

            bool xPoint = false, yPoint = false, zPoint = false;
            if (Math.Abs(indexFingerDir.x) > 0.94 && Math.Abs(indexFingerDir.x) < 1.04) xPoint = true; //print(xPoint);
            if (Math.Abs(indexFingerDir.y) > 0.94 && Math.Abs(indexFingerDir.y) < 1.04) yPoint = true; //print(yPoint);
            if (Math.Abs(indexFingerDir.z) > 0.94 && Math.Abs(indexFingerDir.z) < 1.04) zPoint = true; //print(zPoint);

            // Code below finds the derivative of distance between middle finger and index finger. If this value sharply increases, the system assumes we are opening our fingers
            /*
            if (itr > 5)
                {
                float pinchDistSum = 0;
                for (int i = 0; i < 5; i++)
                {
                    pinchDistSum += pinchDistHistory[i];
                }
                float pinchDistAvg = pinchDistSum / 5;
                float deltaPinchDist = pinchDistAvg / timeElapsed;

                if ((((fingerDist - pinchDistHistory[(itr) % 5]) / timeElapsed) > (deltaPinchDist - 3)) && (((fingerDist - pinchDistHistory[(itr) % 5]) / timeElapsed) < (deltaPinchDist + 3)))
                {
                    pinchBool = true;
                    print(pinchBool);
                    print(deltaPinchDist - ((fingerDist - pinchDistHistory[(itr - 1) % 5]) / timeElapsed));
                }
                else
                {
                    pinchBool = false;
                }
            }
            */


            // Condition to check if we are using a pinch gesture. We are considered to be pinching when our fingers are close together, or by default if 4/5 of
            // the last frames had a pinch gesture in them. In order for a pinch gesture to be recognised, the hand must not be clenched into a fist. A fist is formed when
            // the index finger tip is close to the palm of the hand. 
            if ((fingerDist <= 55 || pinchAvg >= 0.8) && middleFingerPalmDist >= 55 && indexFingerPalmDist >= 55 && ((xPass && yPass) || (zPass && xPass) || (yPass && zPass)) && littleFPDist >= 50 && littleFPDist >= 50)
            {
                if (fingerDist <= 55)
                {
                    pinchHistory[i, itr % 5] = 1;
                    //pinchDistHistory[itr % 5] = fingerDist;
                }
                else
                {
                    pinchHistory[i, itr % 5] = 0;
                }
                gesture = "pinch";
            }
            else
            {
                pinchHistory[i, itr % 5] = 0;
            }

            // Compute average of pinches
            for (int c = 0; c < 5; c++)
            {
                pinchSum += pinchHistory[i, c];
            }

            pinchAvg = (float)(pinchSum / 5);

            // if middle finger is close to center of palm of hand, label gesture as fist
            if ((middleFingerPalmDist < 55 && indexFingerPalmDist < 55) || (littleFPDist < 50 && littleFPDist < 50))
            {
                gesture = "fist";
            }
            else if (middleFingerPalmDist < 55 && indexFingerPalmDist > 55 && zPoint && !(xPoint || yPoint))
            {
                gesture = "z point";
            }
            else if (middleFingerPalmDist < 55 && indexFingerPalmDist > 55 && yPoint && !(xPoint || zPoint))
            {
                gesture = "y point";
            }
            else if (middleFingerPalmDist < 55 && indexFingerPalmDist > 55 && xPoint && !(yPoint || zPoint))
            {
                gesture = "x point";
            }
        }
        return gesture;
    }


    // Data normalisation (to slider values) function
    // Inputs: An array of 3 values, should be in form {x, y, z}
    // Outputs: Normalised Data value (either with a gain function applied or not applied).
    // Function: Take input x, y, z values, and normalise them to a range between 0 and 1023- corresponding to the programmable positions along the 
    // fader sliders.

    float[] normalizedData(float[] vals)
    {
        float[] normalized_val = new float[3];
        float x_val, y_val, z_val;
        x_val = vals[0];
        y_val = vals[1];
        z_val = vals[2];
        // Set minimum value in interaction box of the motion controller
        if (gainFunction == true)
        {

            // Code for gain function
            /*
            float gain;

            if (y_val > 300)
            {
                y_val = 300;
            }

            // Max value in interaction box
            if (y_val < 100)
            {
                y_val = 100;
            }

            gain = (float)0.45 * (y_val - 100);

            if (x_val > (60 + ((float)0.45 * (y_val - 100))))
            {
                x_val = (60 + ((float)0.45 * (y_val - 100)));
            }

            // Max value in interaction box
            if (x_val < -(60 + ((float)0.45 * (y_val - 100))))
            {
                x_val = -(60 + ((float)0.45 * (y_val - 100)));
            }

            // Set minimum value in interaction box of the motion controller

            float present_max_x_val = (60 + ((float)0.45 * (y_val - 100)));
            if (x_val < 0)
            {
                //x_val = x_val / gain;
                normalized_val = ((float)(1023 / (2 * (60 + ((float)0.45 * (y_val - 100))))) * ((60 + ((float)0.45 * (y_val - 100))) + x_val));
                print(normalized_val);
            }

            if (x_val >= 0)
            {
                //x_val = x_val / gain;
                normalized_val = ((float)(1023 / (2 * (60 + ((float)0.45 * (y_val - 100))))) * x_val + (float)511.5);
                print(normalized_val);
            }
        */

            // Triple axis version of gain function has not been incorporated yet
            print("Gain Function does not work in this version yet!");
        }


        else
        {
            // Set minimum value in interaction box of the motion controller along x, y, and z in format {x,y,z}
            int[] xyz_max = { 150, 300, 120 };
            // Set maximum value in interaction box of the motion controller along x, y, and z in format {x,y,z}
            int[] xyz_min = { -150, 100, -120 };

            for (int i = 0; i < 3; i++)
            {
                // Min value in interaction box
                if (vals[i] > xyz_max[i])
                {
                    vals[i] = xyz_max[i];
                }

                // Min value in interaction box
                if (vals[i] < xyz_min[i])
                {
                    vals[i] = xyz_min[i];
                }

                // Check if interaction zone is symmetrical about the origin (position 0)
                if ((xyz_max[i] + xyz_min[i]) == 0)
                {
                    // if below origin (-)
                    if (vals[i] < 0)
                    {
                        normalized_val[i] = (1024 / ((xyz_max[i]) * 2)) * (xyz_max[i] + vals[i] + 1);
                        print(normalized_val);
                    }

                    // if above origin (+)
                    if (vals[i] >= 0)
                    {
                        normalized_val[i] = (1024 / ((xyz_max[i]) * 2)) * vals[i] + (float)(1023 / 2);
                        print(normalized_val);
                    }

                    /*
                    if (vals[i] < 0)
                    {
                        normalized_val[i] = (1023 / ((-xyz_min[i]) * 2)) * (xyz_max[i] + vals[i]);
                        print(normalized_val);
                    }

                    if (vals[i] >= 0)
                    {
                        normalized_val[i] = (1023 / ((-xyz_min[i]) * 2)) * vals[i] + (float)511.5;
                        print(normalized_val);
                    } 
                    */
                }

                else
                {
                    float midPoint = xyz_min[i] + (xyz_max[i] - xyz_min[i]) / 2;
                    float gradient = 1023 / ((midPoint - xyz_min[i]) * 2);

                    if (vals[i] >= midPoint)
                    {
                        normalized_val[i] = gradient * (vals[i] - xyz_min[i]);// + (float)511.5;
                    }
                    else
                    {
                        normalized_val[i] = gradient * (vals[i] - xyz_min[i]);
                    }
                }
            }
        }
        return normalized_val;
    }

    /* Function to filter the data output
     * Inputs: 
     *  - x_pos: The measured varaiable to which filteration is being applied
     *  - itr: Iteration variable counting the number of frames unity has run
     *  - velocity: Calculated instantaneous velocity of the measured value (assuming point is moving in space)
     *  - x_prior: Last filtered value
     *  - p_prior: Last covariance variable found.
     *  - deltaT: The time taken for the last frame to run
     *  
     * Outputs:
     *   - results: An array containing the filtered value at the 0 index, and the covariance value at index 1.
     *   
     * Function:
     *  Apply Kalman filteration technique to data values.
     *  
     * Key Assumptions:
     * For the predictive step- the Kalman Filter takes the past 11 velocity values, and averages them.
     * The average velocity value is multiplied by teh time taken for the last iteration of code, and, using the kinematic equation
     * s = s0 + vt is used to compute the predicted position of the slider (using the last predicted value as s0). 
     * 
     * NOTE that as the velocity and position values are taken from the same sensor, the velcoity is prone to noise just
     * as the position values are. It is for this reason that smoothing is employed on the velocity values.
     * 
     * NOTE ALSO in this function xt, x_predict and x_prior are general variables (not necesarily correlated only with the x axis).
     */

    float[] kalmanFilter(float x_pos, int itr, float[,] velocity, float x_prior, float p_prior, float deltaT, int j)
    {
        float xt = 0; // Filtered value
        float gain;
        float noiseEst = (float)1;
        float[] results = new float[3];
        float x_predict;

        float q = (float)0.35; // Reflects confidence in the model used (lower means more accurate) //0.35
        float p_predict;
        float pt;
        float velocityAvg = 0;
        float velocitySum = 0;
        int arrayMax = 5;

        // ------------------------------------------------------------------------------Predict Stage-------------------------------------------------------------------------------------
        // Find predicted value of covariance
        p_predict = p_prior + q;

        // Compute moving mean of velocity values
        for (int i = 0; i < arrayMax; i++)
        {
            velocitySum = velocity[j, i];
        }
        velocityAvg = velocitySum / arrayMax;

        // Predicted smoothed value- using kinematic equation x = x0 + vt
        x_predict = x_prior + (velocityAvg * deltaT);

        // Statements to execute on application initial start up and everytime hands reappear in frame
        if (itr == 0 || hands_out_of_view == true)
        {
            // Guess Initial values for filter initialisation
            x_predict = x_pos;
            p_predict = 500 + q;
            x_prior = x_pos;
            deltaT = (float)0.001;
            gain = (float)0.3;
            velocityAvg = (float)0.01;
        }

        // If time taken between frames with hands in them exceeds 20ms, set value to 20ms (as hands likely were removed from frame)
        if (deltaT > 40)
        {
            deltaT = 40;
        }

        // ----------------------------------------------------------------------------Update Stage----------------------------------------------------------------------------------------
        gain = p_predict / (p_predict + noiseEst);
        xt = x_predict + gain * (x_pos - x_predict);
        pt = (1 - gain) * p_prior;

        // Results array with filtered value at first index and covariant matrix value (pt) in second position
        results[0] = xt;
        results[1] = pt;
        return results;
    }

    public void snapTo(Vector thumbPos, Vector indexPos)
    {
        Vector3 thumbPos2;
        thumbPos2.x = thumbPos.x;
        thumbPos2.y = thumbPos.y;
        thumbPos2.z = thumbPos.z;

        Vector3 indexPos2;
        indexPos2.x = indexPos.x;
        indexPos2.y = indexPos.y;
        indexPos2.z = indexPos.z;

        Vector3 minVector = Vector3.Min(thumbPos2, indexPos2);
        Vector3 maxVector = Vector3.Max(thumbPos2, indexPos2);

        // minVector = minVector + Vector3.one / 2f;
        // maxVector = maxVector + Vector3.one / 2f;


        int xmax = (int)(1023f * (1f - minVector.x));
        int ymin = (int)(1023f * minVector.y);
        int zmin = (int)(1023f * minVector.z);
        int xmin = (int)(1023f * (1f - maxVector.x));
        int ymax = (int)(1023f * maxVector.y);
        int zmax = (int)(1023f * maxVector.z);

        /*
        float xmin = minVector.x;
        float ymin = minVector.y;
        float zmin = minVector.z;

        float xmax = maxVector.x;
        float ymax = maxVector.y;
        float zmax = maxVector.z;
        */

        float[] minVals = { xmin, ymin, zmin };
        float[] maxVals = { xmax, ymax, zmax };
        // float[] minData = normalizedData(minVals);
        // float[] maxData = normalizedData(maxVals);

        /*
        asar.SendMessage(0, (int)minData[0]);
        asar.SendMessage(2, (int)minData[1]);
        asar.SendMessage(4, (int)minData[2]);
        asar.SendMessage(1, (int)maxData[0]);
        asar.SendMessage(3, (int)maxData[1]);
        asar.SendMessage(5, (int)maxData[2]);
        */

        asar.SendMessage(0, xmin);
        asar.SendMessage(2, ymin);
        asar.SendMessage(4, zmin);
        asar.SendMessage(1, xmax);
        asar.SendMessage(3, ymax);
        asar.SendMessage(5, zmax);
        //print("min: " + minVector.ToString("G6") + "max " + maxVector.ToString("G6"));
        //print(xmin.ToString() + ymin.ToString() + zmin.ToString());
    }


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

    public void ObjectRotation(GameObject obj, float x, float y, float z)
    {
        obj.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
    }

    public static object GetPropValue(object src, string propName)
    {
        return src.GetType().GetProperty(propName).GetValue(src, null);
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

    void Start()
    {
        // Define new serial port, and open it for communication
        asar = new ArduinoSlidesAndRotary.ArduinoReaderHaptics();
        asar.ArduinoReader(COM, 2000000);
        asar.BeginRead();
        //haptics = new ArduinoSlidesAndRotary.HapticsClass.haptic();
        //haptics = new ArduinoSlidesAndRotary.ArduinoReader(COM, 2000000);

        /* CUBE FOR ENCODER DEMO
        // Initialize Cube
        cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube1.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        cube1.transform.Rotate(0.0f, 0.0f, 0.0f, Space.Self);
        //cube1.transform.(3, 3, 3);
        cube1.GetComponent<Renderer>().material.color = Color.blue;
        cube1.name = "obj";
        */

        slidersArray = virtualSliderInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        // Create a seprate thread to run Serial reader on (prevents lag)
        //Thread serial_reader_t = new Thread(new ThreadStart(serial_Read));
        //serial_reader_t.Start();
        ////serial_reader_t.Join();

        if (freeze == false)
        {
            if (toggleHaptics)
            {
                if (!discretizeAxes)
                {
                    //asar.regularStep(0, 0, 0);
                    asar.haptic();
                }
                else if (discretizeAxes)
                {
                    asar.regularStep(xSteps, ySteps, zSteps);
                }
            }
            else
            {
                discretizeAxes = false;
            }

            if (trackFinger)
            {
                // If tracking finger (default) disable palm tracking
                trackPalm = false;
            }

            // Create a new controller object - provided by LEAP Motion
            controller = new Controller();
            // gesture = controller.enableGesture();

            // 
            Frame frame = controller.Frame();
            //interactionBox = frame.InteractionBox;
            stopwatch.Stop();
            timeElapsed = (int)stopwatch.ElapsedMilliseconds; // Record time between frames
            stopwatch.Reset(); // Reset stopwatch function

            // Start timer to find runtime of each frame
            stopwatch.Start();

            int numberHands = frame.Hands.Count;
            // if statement to check if there are any hands in the frame
            if (numberHands > 0)
            {
                for (i = 0; i < numberHands; i++)
                {

                    handInFrameT += (int)timeElapsed;
                    hands_out_of_view = false;
                    List<Hand> hands = frame.Hands;

                    if (hands[0].IsRight && numberHands > 1)
                    {
                        Hand Handholder = hands[0];
                        hands[0] = hands[1];
                        hands[1] = Handholder;
                    }

                    Hand Hand = hands[i];
                    if (trackFinger)
                    {
                        List<Finger> fingers = Hand.Fingers;

                        // Create an object of type finger for the thumb, and index, middle, ring and little fingers
                        Finger thumb = fingers[0];
                        Finger indexFinger = fingers[1];
                        Finger middleFinger = fingers[2];
                        Finger ringFinger = fingers[3];
                        Finger littleFinger = fingers[4];

                        // Assign positions vectors for the tips of index fingers and thumbs
                        fingerTip = indexFinger.TipPosition;
                        thumbTip = thumb.TipPosition;
                        middleFingerTip = middleFinger.TipPosition;

                        //Enum.GetValues(typeof(Bone.BoneType));

                        Bone thumbBone = thumb.Bone(Bone.BoneType.TYPE_PROXIMAL); //Next Joint
                        Bone indexBone = indexFinger.Bone(Bone.BoneType.TYPE_PROXIMAL);
                        indexJoint = indexBone.NextJoint;
                        thumbJoint = thumbBone.NextJoint;

                        old_midPThumbIndex[i] = midPThumbIndex;

                        // Find modpoint between ___ Joint of thumb and ___ joint of index finger

                        midPThumbIndex.x = ((thumbJoint.x + indexJoint.x) / 2);
                        //print(midPThumbIndex.x);
                        midPThumbIndex.y = ((thumbJoint.y + indexJoint.y) / 2);
                        //print(midPThumbIndex.y);
                        midPThumbIndex.z = ((thumbJoint.z + indexJoint.z) / 2);
                        //print(midPThumbIndex.z);

                        // Call function to compute the velocity of finger tips
                        fingerTipV_x[i, index] = velocity(x_axis_index, old_pos_x_index[i], timeElapsed);
                        fingerTipV_y[i, index] = velocity(y_axis_index, old_pos_y_index[i], timeElapsed);
                        fingerTipV_z[i, index] = velocity(z_axis_index, old_pos_z_index[i], timeElapsed);

                        index = itr % 5;

                        //thumbTipV[index] = velocity(x_axis_thumb, old_pos_x_thumb[i], timeElapsed);

                        // Velocity of index finger and thumb midpoint
                        indexThumbMidV_X[i, index] = velocity(midPThumbIndex.x, old_midPThumbIndex[i].x, timeElapsed);
                        indexThumbMidV_Y[i, index] = velocity(midPThumbIndex.y, old_midPThumbIndex[i].y, timeElapsed);
                        indexThumbMidV_Z[i, index] = velocity(midPThumbIndex.z, old_midPThumbIndex[i].z, timeElapsed);

                        // Tracking the position of the tip of the index finger in all three dimensions
                        old_pos_x_index[i] = x_axis_index;
                        x_axis_index = fingerTip.x;

                        old_pos_y_index[i] = y_axis_index;
                        y_axis_index = fingerTip.y;

                        old_pos_z_index[i] = z_axis_index;
                        z_axis_index = fingerTip.z;

                        // Tracking the positions of the tip of the thumb in all three dimensions
                        old_pos_x_thumb[i] = x_axis_thumb;
                        x_axis_thumb = thumbTip.x;

                        old_pos_y_thumb[i] = y_axis_thumb;
                        y_axis_thumb = thumbTip.y;

                        old_pos_z_thumb[i] = z_axis_thumb;
                        z_axis_thumb = thumbTip.z;

                        // Find position of the palm of the hand 
                        handCenter = Hand.PalmPosition;

                        // Run gesture recognition fucntion to identify gesture in the frame
                        gesture = gestureRecognition(thumbTip, fingerTip, handCenter, middleFingerTip, indexFinger, ringFinger, littleFinger);
                        if (gesture == "pinch")
                        {
                            //UnityEngine.Debug.Log("Pinch Activated");
                        }

                        // If finger tracking is active, palm tracking should be inactivated
                        trackPalm = false;
                    }

                    if (trackPalm)
                    {
                        // Tracking palm of hand
                        handCenterV = Hand.PalmVelocity;
                        old_pos_x_hand[i] = x_axis_index;
                        x_axis_hand = handCenter.x;
                    }

                    if (drawCube && numberHands > 1)
                    {
                        //snapTo(thumbTip, fingerTip);
                    }
                    else if (true)//else if (!drawCube) /// Removed draw cube option here
                    {
                        if (gesture == "pinch") // x_axis_index > (old_pos_x_index[i] + 10) || x_axis_index < (old_pos_x_index[i] - 10)
                        {
                            if (applyKalmanFiltering)
                            {
                                filtered_x = kalmanFilter(midPThumbIndex.x, itr, indexThumbMidV_X, old_filtered_x[i], old_p_x_xaxis[i], (float)timeElapsed, i);
                                old_filtered_x[i] = filtered_x[0];
                                old_p_x_xaxis[i] = filtered_x[1];
                                //print(filtered_x[1]);

                                filtered_y = kalmanFilter(midPThumbIndex.y, itr, indexThumbMidV_Y, old_filtered_y[i], old_p_x_yaxis[i], (float)timeElapsed, i);
                                old_filtered_y[i] = filtered_y[0];
                                old_p_x_yaxis[i] = filtered_y[1];
                                // print(filtered_y[1]);

                                filtered_z = kalmanFilter(midPThumbIndex.z, itr, indexThumbMidV_Z, old_filtered_z[i], old_p_x_zaxis[i], (float)timeElapsed, i);
                                old_filtered_z[i] = filtered_z[0];
                                old_p_x_zaxis[i] = filtered_z[1];
                                // print(filtered_z[1]);

                                float[] sent_data_array = { filtered_x[0], filtered_y[0], filtered_z[0] };
                                sent_data = normalizedData(sent_data_array);
                                if (!toggleHaptics)
                                {
                                    if (midPThumbIndex.x != old_midPThumbIndex[i].x)
                                    {
                                        try
                                        {
                                            asar.SendMessage(xAxis[i], (int)sent_data[0]);
                                            GameObject obj = slidersArray[0];
                                            float reqVal = virtualSliderManipulate((int)sent_data[0]);
                                            Vector3 posVir = obj.transform.position;
                                            posVir.x = -reqVal;
                                            obj.transform.position = posVir;
                                            if (numberHands == 1)
                                            {
                                                asar.SendMessage(xAxis[1], (int)sent_data[0]);
                                                obj = slidersArray[1];
                                                
                                            }
                                            //print("SENT X");
                                        }
                                        catch (Exception e)
                                        {
                                            print("EXCEPTION!");
                                            print(e);
                                        }
                                    }

                                    if (midPThumbIndex.y != old_midPThumbIndex[i].y)
                                    {
                                        try
                                        {
                                            asar.SendMessage(yAxis[i], (int)sent_data[1]);
                                            if (numberHands == 1)
                                            {
                                                asar.SendMessage(yAxis[1], (int)sent_data[1]);
                                            }
                                            //print("SENT Y");
                                        }
                                        catch (Exception e)
                                        {
                                            print("EXCEPTION!");
                                            print(e);
                                        }
                                    }

                                    if (midPThumbIndex.z != old_midPThumbIndex[i].z)
                                    {
                                        try
                                        {
                                            asar.SendMessage(zAxis[i], (int)sent_data[2]);
                                            if (numberHands == 1)
                                            {
                                                asar.SendMessage(zAxis[1], (int)sent_data[2]);
                                            }
                                            //print("SENT Z");
                                        }
                                        catch (Exception e)
                                        {
                                            print("EXCEPTION!");
                                            print(e);
                                        }
                                    }

                                    if (toggleHaptics)
                                    {
                                        //print("Haptics Enabled");
                                        if ((discretizeAxes == false) && (hapticTracking == true))
                                        {
                                            asar.SendMessage(xAxis[i], (int)sent_data[0]);
                                            asar.SendMessage(yAxis[i], (int)sent_data[1]);
                                            asar.SendMessage(zAxis[i], (int)sent_data[2]);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Normalize data for length of fader axis before sending
                                float[] sent_data_array = { midPThumbIndex.x, midPThumbIndex.y, midPThumbIndex.z };
                                sent_data = normalizedData(sent_data_array);
                                //UnityEngine.Debug.Log("In loop");
                                //print(sent_data);

                                if (midPThumbIndex.x != old_midPThumbIndex[i].x)
                                {
                                    try
                                    {
                                        asar.SendMessage(xAxis[i], (int)sent_data[0]);
                                    }
                                    catch (Exception e)
                                    {
                                        print("EXCEPTION!");
                                        print(e);
                                    }
                                }

                                if (midPThumbIndex.y != old_midPThumbIndex[i].y)
                                {
                                    try
                                    {
                                        asar.SendMessage(yAxis[i], (int)sent_data[1]);
                                    }
                                    catch (Exception e)
                                    {
                                        print("EXCEPTION!");
                                        print(e);
                                    }
                                }

                                if (midPThumbIndex.z != old_midPThumbIndex[i].z)
                                {
                                    try
                                    {
                                        asar.SendMessage(zAxis[i], (int)sent_data[2]);
                                    }
                                    catch (Exception e)
                                    {
                                        print("EXCEPTION!");
                                        print(e);
                                    }
                                }
                            }
                        }
                    }

                    /*
                    else if (gesture == "x point" || gesture == "y point" || gesture == "z point")
                    {
                        if (applyKalmanFiltering)
                        {
                            filtered_x = kalmanFilter(x_axis_index, itr, fingerTipV_x, old_pos_x_index[i], old_p_x_xaxis[i], (float)timeElapsed);
                            old_filtered_x[i] = filtered_x[0];
                            old_p_x_xaxis[i] = filtered_x[1];

                            filtered_y = kalmanFilter(y_axis_index, itr, fingerTipV_y, old_pos_y_index[i], old_p_x_yaxis[i], (float)timeElapsed);
                            old_filtered_y[i] = filtered_y[0];
                            old_p_x_yaxis[i] = filtered_y[1];

                            filtered_z = kalmanFilter(z_axis_index, itr, fingerTipV_z, old_pos_z_index[i], old_p_x_zaxis[i], (float)timeElapsed);
                            old_filtered_z[i] = filtered_z[0];
                            old_p_x_zaxis[i] = filtered_z[1];


                            float[] sent_data_array = { filtered_x[0], filtered_y[0], filtered_z[0] };
                            sent_data = normalizedData(sent_data_array);

                            if ((old_pos_x_index[i] != x_axis_index) && !toggleHaptics && gesture == "x point")
                            {
                                try
                                {
                                    asar.SendMessage(xAxis[i], (int)sent_data[0]);
                                }
                                catch (Exception e)
                                {
                                    print("EXCEPTION!");
                                    print(e);
                                }
                            }

                            if ((old_pos_y_index[i] != y_axis_index) && !toggleHaptics && gesture == "y point")
                            {
                                try
                                {
                                    asar.SendMessage(yAxis[i], (int)sent_data[1]);
                                }
                                catch (Exception e)
                                {
                                    print("EXCEPTION!");
                                    print(e);
                                }
                            }

                            if ((old_pos_z_index[i] != z_axis_index) && !toggleHaptics && gesture == "z point")
                            {
                                try
                                {
                                    asar.SendMessage(zAxis[i], (int)sent_data[2]);
                                }
                                catch (Exception e)
                                {
                                    print("EXCEPTION!");
                                    print(e);
                                }
                            }
                        }
                        else
                        {
                            // Normalize data for length of fader axis before sending
                            float[] sent_data_array = { midPThumbIndex.x, midPThumbIndex.y, midPThumbIndex.z };
                            sent_data = normalizedData(sent_data_array);
                            //UnityEngine.Debug.Log("In loop");
                            //print(sent_data);

                            if (!toggleHaptics && gesture == "x_point")
                            {
                                try
                                {
                                    asar.SendMessage(xAxis[i], (int)sent_data[0]);
                                }
                                catch (Exception e)
                                {
                                    print("EXCEPTION!");
                                    print(e);
                                }
                            }

                            if (!toggleHaptics && gesture == "y_point")
                            {
                                try
                                {
                                    asar.SendMessage(yAxis[i], (int)sent_data[1]);
                                }
                                catch (Exception e)
                                {
                                    print("EXCEPTION!");
                                    print(e);
                                }
                            }

                            if (!toggleHaptics && gesture == "z_point")
                            {
                                try
                                {
                                    asar.SendMessage(zAxis[i], (int)sent_data[2]);
                                    print("In z point");
                                }
                                catch (Exception e)
                                {
                                    print("EXCEPTION!");
                                    print(e);
                                }
                            }
                        }
                    }
                    */
                }
            }
            else
            {
                // Empty arrays used in Kalman filter once hands are removed from the frame
                filtered_x = new float[2];
                filtered_y = new float[2];
                filtered_z = new float[2];
                fingerTipV_x = new float[2, 5];
                fingerTipV_y = new float[2, 5];
                fingerTipV_z = new float[2, 5];
                thumbTipV = new float[2, 5];

                indexThumbMidV_X = new float[2, 5];
                indexThumbMidV_Y = new float[2, 5];
                indexThumbMidV_Z = new float[2, 5];

                // Ensure time elapsed doesn't keep growing (could cause the position prediction in the Kalman function to be inflated)
                timeElapsed = 0;

                // Reset hand in frame time
                handInFrameT = 0;
                old_filtered_x = new float[2];
                old_filtered_y = new float[2];
                old_filtered_z = new float[2];
                old_p_x_xaxis = new float[2];
                old_p_x_yaxis = new float[2];
                old_p_x_zaxis = new float[2];
                hands_out_of_view = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (freeze == true)
            {
                freeze = false;
            }
            else
            {
                freeze = true;
            }
        }

        // Print time elapsed between frames to the Unity console
        //print(timeElapsed);

        // Iterate variable
        itr++;

        // Terminate serial communication if 'E' is pressed by the user in play mode
        if (Input.GetKeyDown(KeyCode.E))
        {
            asar.Terminate();
            Application.Quit();
        }

        // print(timeElapsed);
    }
}
#endif