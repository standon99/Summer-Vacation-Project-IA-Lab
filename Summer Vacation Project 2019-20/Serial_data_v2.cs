# if false
using System.Collections;
using System.Collections.Generic;
//using System.IO.Ports;
using UnityEngine;
using ArduinoSlidesAndRotary;
using System;
using UnityEngine.UI;
using Leap;
using Leap.Unity;
using Unity;
using LeapInternal;
using System.Diagnostics; // Contains Stopwatch class


/* TO INCLUDE/CHANGE LOG:
 * Smoothing of input (Kalman filter)
 * Only send data if present position is different to previous position --- DONE
 * Fix so both hands can be used --- DONE IO EXCEPTION ERROR --- FIXED
 * PID in Arduino
 * Restrict amount of data sent through serial port (no unnecessary data sends).
 * 
*/

/*
public class Serial_data_v2 : MonoBehaviour
{
    // Add controller object to the program
    Controller controller;


    private bool full_vol = false;
    private bool interaction_box = true;

    //InteractionBox interactionBox;

    public Transform thumb;
    public Transform index;
    public Transform repere;

    private Vector handCenter;
    private Vector fingerTip;

    // Axis along LEAP motion module
    public int pos_x;
    public int old_pos_x_index;

    // Vertical axis from LEAP motion module
    public int pos_y;

    public int sent_data;


    // Arduino COM Port for Serial Communication
    public string COM = "COM6";

    ArduinoSlidesAndRotary.ArduinoReader asar;
    Vector interactionBox;

    int normalizedData(int pos_x)
    {
        float normalized_val = 0;
        if (pos_x > 200)
        {
            pos_x = 200;
        }

        if (pos_x < -200)
        {
            pos_x = -200;
        }

        if (pos_x < 0)
        {
            normalized_val = (float)2.5575 * (200 + (float)pos_x);
            //print(normalized_val);
        }

        if (pos_x >= 0)
        {
            normalized_val = (float)2.5575 * (float)pos_x + (float)511.5;
            //print(normalized_val);
        }

        return (int)normalized_val;
    }

    void Start()
    {
        asar = new ArduinoSlidesAndRotary.ArduinoReader(COM, 250000);
        asar.BeginRead();

        controller = new Controller();
    }

    // Update is called once per frame
    void Update()
    {
        Frame frame = controller.Frame();
        //interactionBox = frame.InteractionBox;

        // if statement to check if there are any hands in the frame
        if (frame.Hands.Count > 0)
        {
            List<Hand> hands = frame.Hands;
            Hand firstHand = hands[0];
            List<Finger> fingers = firstHand.Fingers;
            Finger indexFinger = fingers[1];
            fingerTip = indexFinger.TipPosition;
            pos_x = (int)fingerTip.x;
        }

        //Hand hand = frame.Hands[0];
        //handCenter = hand.PalmPosition;
        //pos_x = (int)handCenter.x;


        //handCenter = firstHand.PalmPosition;
        //pos_x = (int)handCenter.x;


        // Normalisation:
     
        sent_data = normalizedData(pos_x);
        //print(sent_data);

        //       if (Input.GetKeyDown(KeyCode.Space))
        //       {
        //print("CALLED");

        try
            {
                asar.SendMessage(1, sent_data);
                print(sent_data);
            }
            catch (Exception e)
            {
                print("EXCEPTION!");
                print(e);
            }
 //       }

        void OnApplicationQuit()
        {
            asar.Terminate();
        }
    }
}
*/

// -----------------------------------------------------------------------------------//

public class Serial_data_v2 : MonoBehaviour
{
    // Add controller object to the program
    Controller controller;

    private bool full_vol = false;
    private bool interaction_box = true;
    private bool hands_out_of_view = false;
    public bool trackFinger = true;
    public bool trackPalm = false;
    public bool applyKalmanFiltering = true;

    //InteractionBox interactionBox;

    public int itr = 0;

    int xNMinusOne;
    int xN;
    int gain;

    // Position vectors for trackable objects
    private Vector handCenter;
    private Vector fingerTip;

    // Velocity for trackable objects ALONG X AXIS ONLY
    private Vector handCenterV;
    private float[] fingerTipV = new float[11];
    private float oldFingerTipV;

    // Elapsed Time
    public float timeElapsed;

    // Axis along LEAP motion module
    //public int[] pos_x;
    public float x_axis_index = 0;
    public float old_pos_x_index;
    public int index = 0;

    public float y_axis;

    public float z_axis;

    // Kalman filtered data value
    public float[] filtered_x = new float[2];
    public float old_filtered_x = 10;
    public float old_p = 10;

    // Vertical axis from LEAP motion module
    //public int pos_y;

    public int sent_data;

    public Stopwatch stopwatch = new Stopwatch();
    public int time;

    // Arduino COM Port for Serial Communication
    public string COM = "COM8";

    ArduinoSlidesAndRotary.ArduinoReader asar;
    //Vector interactionBox;

    // Data normalisation (to slider values) function
    int normalizedData(float x_val)
    {
        float normalized_val = 0;
        if (x_val > 120)
        {
            x_val = 120;
        }

        if (x_val < -120)
        {
            x_val = -120;
        }

        if (x_val < 0)
        {
            normalized_val = (float)4.2625 * (120 + x_val);
            //print(normalized_val);
        }

        if (x_val >= 0)
        {
            normalized_val = (float)4.2625 * x_val + (float)511.5;
            //print(normalized_val);
        }

        return (int)normalized_val;
    }

    // Function to apply Kalman filter to data output
    float[] kalmanFilter(float x_pos, int itr, float[] velocity, float x_prior, float p_prior, float deltaT)
    {
        /*
        float filteredVal = 0;
        int pK = 0;
        int noiseEst = 1; 
        float[] results = new float [3];

        if (itr == 0)
        {
            // Guess Initial values for filter initialisation
            xKMinusOne = 10;
            filteredVal = 10;
            pKMinusOne = 0;
            vMinusOne = 0;
            UnityEngine.Debug.Log("In IF Statement");
        }

        pK = pKMinusOne;
        filteredVal = xKMinusOne;
        //gain = (filteredVal - xKMinusOne) / (x_pos - xKMinusOne);

        gain = pKMinusOne / (pKMinusOne + noiseEst);
        filteredVal = filteredVal + gain * (x_pos - filteredVal);

        pK = (1 - gain) * pK;

        */

        /*
        filteredVal = xKMinusOne + gain * (x_pos - xKMinusOne);
        //filteredVal = (float)filteredVal;
        filteredVal = filteredVal + (time * filteredVal);
        gain = pKMinusOne / (pKMinusOne + noiseEst);
        pK = (1 - gain) * pK;


        results[0] = filteredVal;
        results[1] = pK;
        results[2] = gain;
        */

        /*  Key Assumptions:
         * 
         * 
         * 
         */

        float xt = 0; // Filtered value
        float gain;
        float noiseEst = (float)100000000000000;
        float[] results = new float[3];
        float x_predict;
        float q = (float)0.000000000000000000000000000000000000001;
        float p_predict;
        float pt;
        float velocityAvg = 0;
        float velocitySum = 0;
        int arrayMax = 10;


        // Predict Stage:

        p_predict = p_prior + q;


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

        if ((x_prior - (velocityAvg * deltaT)) < 0)
        {
            UnityEngine.Debug.Log("ERROR");
        }
        /*
        ((x_prior - (velocityAvg * deltaT)) > 0)
        {
            x_predict = x_prior + (velocityAvg * deltaT);// velocity * deltaT;
        }
        else
        {
            velocityAvg = 0;
            x_predict = x_prior + (velocityAvg * deltaT);
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

    float fingerTipVelocity_x(float x_axis_index, float old_pos_x_index, float timeSpan)
    {
        float fingerTipVelocity = 0;
        if (timeSpan == 0)
        {
            fingerTipVelocity = 0;
        }
        else
        {
            fingerTipVelocity = (x_axis_index - old_pos_x_index) / timeSpan;
            //print(fingerTipVelocity);
        }
        return fingerTipVelocity;
    }

    void Start()
    {
        /*
        if (!open_port)
        {
            asar = new ArduinoSlidesAndRotary.ArduinoReader(COM, 9600);
            asar.BeginRead();
            open_port = false;

        }
       */

        asar = new ArduinoSlidesAndRotary.ArduinoReader(COM, 9600);
        asar.BeginRead();

        //controller = new Controller();
        //pos_x = new int[2] { 1, 0 };
    }

    // Update is called once per frame
    void Update()
    {
        if (trackFinger)
        {
            trackPalm = false;
        }

        controller = new Controller();
        Frame frame = controller.Frame();
        //interactionBox = frame.InteractionBox;

        // if statement to check if there are any hands in the frame
        if (frame.Hands.Count > 0)
        {
            List<Hand> hands = frame.Hands;
            Hand firstHand = hands[0];

            if (trackFinger)
            {
                stopwatch.Stop();
                timeElapsed = (int)stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
                // print(timeElapsed);
                List<Finger> fingers = firstHand.Fingers;
                Finger indexFinger = fingers[1];
                fingerTip = indexFinger.TipPosition;
                stopwatch.Start();

                // Call function to compute the velocity of finger tips
                print(itr % 11);
                index = itr % 11;
                fingerTipV[index] = fingerTipVelocity_x(x_axis_index, old_pos_x_index, timeElapsed);
                
                // Tracking thee position of the tip of the index finger
                old_pos_x_index = x_axis_index;
                x_axis_index = fingerTip.x;
                trackPalm = false;
            }

            if (trackPalm)
            {
                // Tracking palm of hand
                handCenter = firstHand.PalmPosition;
                handCenterV = firstHand.PalmVelocity;
                old_pos_x_index = x_axis_index;
                x_axis_index = handCenter.x;
            }

            if (x_axis_index != old_pos_x_index) // x_axis_index > (old_pos_x_index + 10) || x_axis_index < (old_pos_x_index - 10)
            {
                if (applyKalmanFiltering)
                {
                    //filtered_x = kalmanFilter(x_axis_index, itr, old_filtered_x, pKMinusOne);
                    filtered_x = kalmanFilter(x_axis_index, itr, fingerTipV, old_filtered_x, old_p, (float)timeElapsed);
                    old_filtered_x = filtered_x[0];
                    old_p = filtered_x[1];
                    sent_data = normalizedData(filtered_x[0]);
                }
                else
                {
                    sent_data = normalizedData(x_axis_index);
                    //UnityEngine.Debug.Log("Ïn loop");
                    print(sent_data);
                }
            }

            /*  OLD METHOD OF ASSESSING LAST TWO VALUES USING ARRAYS
            if ((itr % 2) == 0)
            {
                pos_x[0] = (int)fingerTip.x;

                if (pos_x[0] != pos_x[1])
                {
                    sent_data = normalizedData(pos_x[0]);
                }
            }
            else
            {
                pos_x[1] = (int)fingerTip.x;
                if (pos_x[0] != pos_x[1])
                {
                    sent_data = normalizedData(pos_x[1]);
                }
            }
            */

        }
        else
        {
            filtered_x = new float[2];
            fingerTipV = new float[11];
            timeElapsed = 0;
            old_filtered_x = 0;
            old_p = 0;
            hands_out_of_view = true;
        }

        //print(sent_data);

        //       if (Input.GetKeyDown(KeyCode.Space))
        //       {
        //print("CALLED");

        try
        {
            asar.SendMessage(1, sent_data);
            //print(sent_data);
        }
        catch (Exception e)
        {
            print("EXCEPTION!");
            print(e);
        }
        //       }

        void OnApplicationQuit()
        {
            asar.Terminate();
        }

        itr++;
    }
}
#endif