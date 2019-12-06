//************************************************************************************
//* Monash University - Faculty of Information Technology - Immersive Analytics Lab  *
//* Version 1.0 - December 2019                                                      *
//************************************************************************************

//-------------------------------------------------------------------------------------------------------------------------------
/*
 *TO DO (SIDDHANT)/CHANGE LOG:
 * Smoothing of input (Kalman filter) --- DONE --- 2/12/19
 * Only send data if present position is different to previous position --- DONE --- 28/11/19
 * Fix so both hands can be used --- DONE IO EXCEPTION ERROR --- FIXED --- 29/11/19
 * PID in Arduino --- MODIFIED VERSION INCORPORATED --- 2/12/19
 * Restrict amount of data sent through serial port (no unnecessary data sends).
 * Map movement distance of fingers to movement of fader in a 1:1 ratio --- DONE (normalization function altered) --- 2/12/19
 * Gesture Recognition --- Pinch is now registered (index finger and thumb tips pushed together) --- 3/12/2019
 * Gesture Recognition --- Can now terminate gesture tracking by closing fist --- 4/12/2019
 * Gesture Recognition --- Tracking center of hand gesture area
 * Gain Function --- Increase precision the higher you go (min 100 and max 300 due to accuracy issues) --- DRAFTED but not functioning optimally (have to address issues) --- 4/12/2019 
*/
// ------------------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArduinoSlidesAndRotary; // Contains asar class
using System;
using Leap; // Library for interfacing with LEAP motion controller
using System.Diagnostics; // Contains Stopwatch class
//using System.IO.Ports;

public class Triple_axis_V1 : MonoBehaviour
{
    // Add controller object to the program
    Controller controller;

    private bool full_vol = false;
    private bool interaction_box = true;
    private bool hands_out_of_view = false;
    public bool trackFinger = true;
    public bool trackPalm = false;
    public bool applyKalmanFiltering = true;
    public bool freeze = false;
    public bool gainFunction = false;
    public bool pinchBool = true;

    //InteractionBox interactionBox;

    public int itr = 0;

    int xNMinusOne;
    int xN;
    int gain;

    // Position vectors for trackable objects
    private Vector handCenter, fingerTip, thumbTip, middleFingerTip, thumbJoint, indexJoint;

    // Vectors describing position/velocity of the midpoint between thumb joint and index finger joint
    private Vector3 midPThumbIndex;
    private Vector3 old_midPThumbIndex;

    // Velocity for trackable objects ALONG X AXIS ONLY
    private Vector handCenterV;
    private float[] fingerTipV, thumbTipV, indexThumbMidV_X, indexThumbMidV_Y, indexThumbMidV_Z = new float[5];
    private float oldFingerTipV;
    private float oldThumbTipV;

    // Elapsed Time
    public float timeElapsed;
    public int index = 0;

    // Along x axis of LEAP motion module
    //public int[] pos_x;
    public float x_axis_index, x_axis_thumb = 0;
    public float old_pos_x_index, old_pos_x_thumb;

    // Vertical axis from LEAP motion module
    public float y_axis_index = 0;
    public float old_pos_y_index;

    public float y_axis_thumb = 0;
    public float old_pos_y_thumb;

    // Z axis from LEAP motion module
    public float z_axis_index = 0;
    public float old_pos_z_index;

    public float z_axis_thumb = 0;
    public float old_pos_z_thumb;

    // Kalman filtered data value
    public float[] filtered_x, filtered_y, filtered_z = new float[2];
    
    public float old_filtered_x, old_filtered_y, old_filtered_z = 10;

    public float old_p_x_xaxis, old_p_x_yaxis, old_p_x_zaxis = 10;

    // Vertical axis from LEAP motion module
    // public int pos_y;

    public int time;
    public float[] sent_data = new float[3];
    // Create new stopwatch object
    public Stopwatch stopwatch = new Stopwatch();
   
    // Arduino COM Port for Serial Communication
    public string COM = "COM3";

    // Variable to store gesture type
    public string gesture;
    public int[] pinchHistory = new int[5];
    public float pinchAvg = 0;
    public float[] pinchDistHistory = new float[5];

    ArduinoSlidesAndRotary.ArduinoReader asar;
    //Vector interactionBox;

    string gestureRecognition(Vector thumbTip, Vector fingerTip, Vector palmPos, Vector middleFingerTip)
    {
        // String to store selected gesture type
        string gesture = "none";
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

        float fingerDist = (float)Math.Sqrt(Math.Pow(((double)thumbTip.x - (double)fingerTip.x), 2) + Math.Pow(((double)thumbTip.y - (double)fingerTip.y), 2) + Math.Pow(((double)thumbTip.z - (double)fingerTip.z), 2));
        //print(fingerDist);
        float middleFingerPalmDist = (float)Math.Sqrt(Math.Pow(((double)middleFingerTip.x - (double)palmPos.x), 2) + Math.Pow(((double)middleFingerTip.y - (double)palmPos.y), 2) + Math.Pow(((double)middleFingerTip.z - (double)palmPos.z), 2));
        //print(middleFingerPalmDist);
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
        if ((fingerDist <= 45 || pinchAvg >= 0.8) && middleFingerPalmDist >= 55)  //t_f_x && t_f_y)// && t_f_z)
        {
            if (fingerDist <= 45)
            {
                pinchHistory[itr % 5] = 1;
                //pinchDistHistory[itr % 5] = fingerDist;
            }
            else
            {
                pinchHistory[itr % 5] = 0;
            }
            gesture = "pinch";
        }
        else
        {
            pinchHistory[itr % 5] = 0;
        }

        // Compute average of pinches
        for (int i = 0; i < 4; i++)
        {
            pinchSum += pinchHistory[i];
        }

        pinchAvg = (float)(pinchSum / 5);

        if (middleFingerPalmDist < 55)
        {
            gesture = "fist";
        }
        //print(fingerDist);
        return gesture;
    }

    // Data normalisation (to slider values) function
    float[] normalizedData(float[] vals)
    {
        float[] normalized_val = new float[3];
        float x_val, y_val, z_val;
        x_val = vals[0];
        y_val = vals[1];
        z_val = vals[0];
        // Set minimum value in interaction box of the motion controller
        if (gainFunction == true)
        {
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

            print("Gain Function does not work in this version yet!");
        }

        
        else
        {
            // Set minimum value in interaction box of the motion controller

            for (int i = 0; i < 3; i++)
            {
                if (vals[i] > 120)
                {
                    vals[i] = 120;
                }

                // Max value in interaction box
                if (vals[i] < -120)
                {
                    vals[i] = -120;
                }

                if (vals[i] < 0)
                {
                    normalized_val[i] = (float)4.2625 * (120 + vals[i]);
                    //print(normalized_val);
                }

                if (vals[i] >= 0)
                {
                    normalized_val[i] = (float)4.2625 * vals[i] + (float)511.5;
                    //print(normalized_val);
                }
            }
        }
        return normalized_val;
    }

    // Function to filter the data output
    float[] kalmanFilter(float x_pos, int itr, float[] velocity, float x_prior, float p_prior, float deltaT)
    {
        /* Key Assumptions:
         * For the predictive step- the Kalman Filter takes the past 11 velocity values, and averages them.
         * The average velocity value is multiplied by teh time taken for the last iteration of code, and, using the kinematic equation
         * s = s0 + vt is used to compute the predicted position of the slider (using the last predicted value as s0). 
         * 
         * NOTE that as the velocity and position values are taken from the same sensor, the velcoity is prone to noise just
         * as the position values are. It is for this reason that smoothing is employed on the velocity values.
         * 
         * NOTE in this function xt, x_predict and x_prior are general variables (not necesarily correlated only with the x axis).
         */

        float xt = 0; // Filtered value
        float gain;
        float noiseEst = (float)1;
        float[] results = new float[3];
        float x_predict;
        float q = (float)0.3; // Reflects confidence in the model used (lower means more accurate)
        float p_predict;
        float pt;
        float velocityAvg = 0;
        float velocitySum = 0;
        int arrayMax = 5;


        // Predict Stage:
        p_predict = p_prior + q;

        // Compute moving mean of velocity values
        for (int i = 0; i < arrayMax; i++)
        {
            velocitySum = velocity[i];
        }
        velocityAvg = velocitySum / arrayMax;

        x_predict = x_prior + (velocityAvg * deltaT);

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

        if (deltaT > 20)
        {
            deltaT = 20;
        }

        /*
        if ((x_prior - (velocityAvg * deltaT)) < 0)
        {
            UnityEngine.Debug.Log("ERROR");
        }
       */

        //print(velocity);
        // Update Stage:
        gain = p_predict / (p_predict + noiseEst);

        xt = x_predict + gain * (x_pos - x_predict);

        pt = (1 - gain) * p_prior;

        //UnityEngine.Debug.Log("Kalman Filter Engaged");
        //print(gain);

        // Results array with filtered value at first index and covariant matrix value (pt) in second position
        results[0] = xt;
        results[1] = pt;
        return results;
    }

    // Function to compute the velocity of a point given its present position, past position, and time between measurements
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

    void Start()
    {
        // Define new serial port, and open it for communication
        asar = new ArduinoSlidesAndRotary.ArduinoReader(COM, 9600);
        asar.BeginRead();

    }

    // Update is called once per frame
    void Update()
    {
        if (freeze == false)
        {
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

            // if statement to check if there are any hands in the frame
            if (frame.Hands.Count > 0)
            {
                hands_out_of_view = false;
                List<Hand> hands = frame.Hands;
                Hand firstHand = hands[0];

                if (trackFinger)
                {
                    stopwatch.Stop();
                    timeElapsed = (int)stopwatch.ElapsedMilliseconds;
                    stopwatch.Reset();
                    // print(timeElapsed);
                    List<Finger> fingers = firstHand.Fingers;

                    // Start timer to find runtime of each frame
                    stopwatch.Start();

                    // Create an object of type finger for the index finger & thumb
                    Finger indexFinger = fingers[1];
                    Finger thumb = fingers[0];
                    Finger middleFinger = fingers[2];

                    // Assign positions vectors for the tips of index fingers and thumbs
                    fingerTip = indexFinger.TipPosition;
                    thumbTip = thumb.TipPosition;
                    middleFingerTip = middleFinger.TipPosition;

                    //Enum.GetValues(typeof(Bone.BoneType));

                    Bone thumbBone = thumb.Bone(Bone.BoneType.TYPE_PROXIMAL); //Next Joint
                    Bone indexBone = indexFinger.Bone(Bone.BoneType.TYPE_PROXIMAL);
                    indexJoint = indexBone.NextJoint;
                    thumbJoint = thumbBone.NextJoint;

                    old_midPThumbIndex = midPThumbIndex;

                    // Find modpoint between ___ Joint of thumb and ___ joint of index finger

                    //print("NEXT");
                    midPThumbIndex.x = ((thumbJoint.x + indexJoint.x) / 2);
                    //print(midPThumbIndex.x);
                    midPThumbIndex.y = ((thumbJoint.y + indexJoint.y) / 2);
                    //print(midPThumbIndex.y);
                    midPThumbIndex.z = ((thumbJoint.z + indexJoint.z) / 2);
                    //print(midPThumbIndex.z);

                    // Call function to compute the velocity of finger tips
                    //print(itr % 11);

                    index = itr % 5;

                    fingerTipV[index] = velocity(x_axis_index, old_pos_x_index, timeElapsed);
                    thumbTipV[index] = velocity(x_axis_thumb, old_pos_x_thumb, timeElapsed);

                    // Velcoity of index finger and thumb midpoint
                    indexThumbMidV_X[index] = velocity(midPThumbIndex.x, old_midPThumbIndex.x, timeElapsed);
                    indexThumbMidV_Y[index] = velocity(midPThumbIndex.x, old_midPThumbIndex.x, timeElapsed);
                    indexThumbMidV_Z[index] = velocity(midPThumbIndex.z, old_midPThumbIndex.z, timeElapsed);

                    // Tracking the position of the tip of the index finger in all three dimensions
                    old_pos_x_index = x_axis_index;
                    x_axis_index = fingerTip.x;

                    old_pos_y_index = y_axis_index;
                    y_axis_index = fingerTip.y;

                    old_pos_z_index = z_axis_index;
                    z_axis_index = fingerTip.z;

                    // Tracking the positions of the tip of the thumb in all three dimensions
                    old_pos_x_thumb = x_axis_thumb;
                    x_axis_thumb = thumbTip.x;

                    old_pos_y_thumb = y_axis_thumb;
                    y_axis_thumb = thumbTip.y;

                    old_pos_z_thumb = z_axis_thumb;
                    z_axis_thumb = thumbTip.z;

                    // Find position of the palm of the hand 
                    handCenter = firstHand.PalmPosition;

                    gesture = gestureRecognition(thumbTip, fingerTip, handCenter, middleFingerTip);
                    if (gesture == "pinch")
                    {
                        UnityEngine.Debug.Log("Pinch Activated");
                    }
                    // If finger tracking is active, thumb tracking should be inactivated
                    trackPalm = false;
                }

                /*
                if (trackPalm)
                {
                    // Tracking palm of hand
                    handCenterV = firstHand.PalmVelocity;
                    old_pos_x_hand = x_axis_index;
                    x_axis_hand = handCenter.x;
                }
                */

                if (gesture == "pinch") // x_axis_index > (old_pos_x_index + 10) || x_axis_index < (old_pos_x_index - 10)
                {
                    if (applyKalmanFiltering)
                    {

                        /* TRACKING INDEX FINGER TIP
                        //filtered_x = kalmanFilter(x_axis_index, itr, old_filtered_x, pKMinusOne);
                        filtered_x = kalmanFilter(x_axis_index, itr, fingerTipV, old_filtered_x, old_p_x, (float)timeElapsed);
                        old_filtered_x = filtered_x[0];
                        old_p_x= filtered_x[1];
                        sent_data = normalizedData(filtered_x[0]);
                        */

                        filtered_x = kalmanFilter(midPThumbIndex.x, itr, indexThumbMidV_X, old_filtered_x, old_p_x_xaxis, (float)timeElapsed);
                        old_filtered_x = filtered_x[0];
                        old_p_x_xaxis = filtered_x[1];
                        
                        filtered_y = kalmanFilter(midPThumbIndex.y, itr, indexThumbMidV_Y, old_filtered_x, old_p_x_yaxis, (float)timeElapsed);
                        old_filtered_y = filtered_y[0];
                        old_p_x_yaxis = filtered_y[1];
                        

                        filtered_z = kalmanFilter(midPThumbIndex.z, itr, indexThumbMidV_Z, old_filtered_z, old_p_x_zaxis, (float)timeElapsed);
                        old_filtered_z = filtered_y[0];
                        old_p_x_zaxis = filtered_y[1];
                        float[] sent_data_array = {filtered_x[0], filtered_y[0], filtered_z[0]};
                        sent_data = normalizedData(sent_data_array);

                        if(midPThumbIndex.x != old_midPThumbIndex.x)
                        {
                            try
                            {
                                asar.SendMessage(2, (int)sent_data[1]);
                                //print(sent_data);
                            }
                            catch (Exception e)
                            {
                                print("EXCEPTION!");
                                print(e);
                            }
                        }

                        if(midPThumbIndex.y != old_midPThumbIndex.y)
                        {
                            try
                            {
                                asar.SendMessage(1, (int)sent_data[2]);
                                //print(sent_data);
                            }
                            catch (Exception e)
                            {
                                print("EXCEPTION!");
                                print(e);
                            }
                        }

                        if(midPThumbIndex.z != old_midPThumbIndex.z)
                        {
                            try
                            {
                                asar.SendMessage(3, (int)sent_data[3]);
                                //print(sent_data);
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
                        float[] sent_data_array = {midPThumbIndex.x , midPThumbIndex.y, midPThumbIndex.z};
                        sent_data = normalizedData(sent_data_array);
                        //UnityEngine.Debug.Log("Ïn loop");
                        //print(sent_data);
                    }
                }
            }
            else
            {
                // Empty arrays used in Kalman filter once hands are removed from the frame
                filtered_x = new float[2];
                filtered_y = new float[2];
                fingerTipV = new float[5];
                thumbTipV = new float[5];

                indexThumbMidV_X = new float[5];
                indexThumbMidV_Y = new float[5];

                // Ensure time elapsed doesnt keep growing (could cause the posiiton prediction in the Kalman function to be inflated)
                timeElapsed = 0;
                old_filtered_x = 0;
                old_filtered_y = 0;
                old_p_x_xaxis = 0;
                old_p_x_yaxis = 0;
                hands_out_of_view = true;
            }

            void OnApplicationQuit()
            {
                asar.Terminate();
            }

            itr++;
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
    }
}
