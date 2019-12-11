using System;
using System.Collections;
using ArduinoSlidesAndRotary;
using UnityEngine;
using UnityEngine.UI;

public class TEST : MonoBehaviour
{
    [SerializeField] VirtualFaderAxis _virtualFaderAxis;
    
    ArduinoSlidesAndRotary.ArduinoReader asar;

    public Transform thumb;
    public Transform index;

    public Transform repere;

    //public Text text;
    //public Color col;

    //public int incr = 0;

    public string COM = "COM4";

    //public Color[] couleurs;

    //public float vitesseRotation = 100.0f;
    public int sliderID = 0;

    public float x0;
    public float x1;
    public float y0;
    public float y1;
    public float z0;
    public float z1;
    public int rotary1;
    public int press1;
    public int rotary2;
    public int press2;
    public int rotary3;
    public int press3;

    bool p1 = false;

    //private float r = 0;
    //private float g = 0;
    //private float b = 0;

    //private int newr1 = 0;
    //private int newr2 = 0;
    //private int newr3 = 0;

    void Start()
    {
        asar = new ArduinoSlidesAndRotary.ArduinoReader(COM, 250000);
        asar.BeginRead();

        //col = Color.white;
        //couleurs = new Color[]{ Color.black, Color.blue, Color.clear, Color.cyan, Color.gray, Color.green, Color.grey, Color.magenta, Color.red, Color.white, Color.yellow};
    }


    public void SetSlider(int idx, float value)
    {
        if (idx < 6)
        {
        int target = (int)(1023f * value);
        asar.SendMessage(idx, target);        
        }
    }

    float timeNow = 0;
    void Update()
    {
        if (asar.Press1 == 1 && press1 == 0)
        {
            p1 = !p1;
        }

        if (asar.IsMoving1 == 0)
        {
            if (x0 != asar.Slider1)
            {
                x0 = asar.Slider1;
                float v = x0 / 1023.0f;
                _virtualFaderAxis.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.X_AXIS_MIN, v);
            }
        }
        if (asar.IsMoving2 == 0)
        {
            if (x1 != asar.Slider2)
            {
                x1 = asar.Slider2;
                float v = x1 / 1023.0f;
                _virtualFaderAxis.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.X_AXIS_MAX, v);
            }
        }
        if (asar.IsMoving3 == 0)
        {
            if (y0 != asar.Slider3)
            {
                y0 = asar.Slider3;
                float v = y0 / 1023.0f;
                _virtualFaderAxis.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Y_AXIS_MIN, v);
            }
        }
        if (asar.IsMoving4 == 0)
        {
            if (y1 != asar.Slider4)
            {
                y1 = asar.Slider4;
                float v = y1 / 1023.0f;
                _virtualFaderAxis.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Y_AXIS_MAX, v);
            }
        }
        if (asar.IsMoving5 == 0)
        {
            if (z0 != asar.Slider5)
            {
                z0 = asar.Slider5;
                float v = z0 / 1023.0f;
                _virtualFaderAxis.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Z_AXIS_MIN, v);
            }
        }
        if (asar.IsMoving6 == 0)
        {
            if (z1 != asar.Slider6)
            {
                z1 = asar.Slider6;
                float v = z1 / 1023.0f;
                _virtualFaderAxis.SetSliderFromPhysical(VirtualFaderAxis.AxisSliders.Z_AXIS_MAX, v);
            }
        }

        rotary1 = asar.Rotary1;
        press1 = asar.Press1;
        rotary2 = asar.Rotary2;
        press2 = asar.Press2;
        rotary3 = asar.Rotary3;
        press3 = asar.Press3;

        //select slider ID
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (sliderID < 5)
            {
                sliderID += 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (sliderID > 0)
            {
                sliderID -= 1;
            }
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        if (p1)
        {
            if (timeNow > 0.01)
            {
                snapTo();
                timeNow = 0;
            }
            else timeNow += Time.deltaTime;
            //}
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                asar.SendMessage(6, 0);
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            asar.SendMessage(sliderID, 0);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            asar.SendMessage(sliderID, 100);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            asar.SendMessage(sliderID, 200);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            asar.SendMessage(sliderID, 300);
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            asar.SendMessage(sliderID, 400);
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            asar.SendMessage(sliderID, 500);
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            asar.SendMessage(sliderID, 600);
        }
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            asar.SendMessage(sliderID, 700);
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            asar.SendMessage(sliderID, 800);
        }
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            asar.SendMessage(sliderID, 900);
        }

        //        //reshape
        //        transform.localScale = new Vector3(0.1F * slider1, 0.1F * slider2, 0.1F * slider3);

        //        //col = couleurs[abs(rotary1 % 11)];




        //        if(r < 100 && rotary1 > newr1)
        //        {
        //            r += 1;
        //        }
        //        else if(r>0 && rotary1 < newr1)
        //        {
        //            r -= 1;
        //        }
        //        newr1 = rotary1;
        //        if (g < 100 && rotary2 > newr2)
        //        {
        //            g += 1;
        //        }
        //        else if (g > 0 && rotary2 < newr2)
        //        {
        //            g -= 1;
        //        }
        //        newr2 = rotary2;
        //        if (b < 100 && rotary3 > newr3)
        //        {
        //            b += 1;
        //        }
        //        else if (b > 0 && rotary3 < newr3)
        //        {
        //            b -= 1;
        //        }
        //        newr3 = rotary3;
        //        col.r = r/100;
        //        col.g = g / 100;
        //        col.b = b / 100;
        ////        var comp = GetComponent<Renderer>().material;
        // //       if (comp != null) comp.color = col;




        //        if (press1==1)
        //        {
        //            //snapTo();
        //            transform.Rotate(0, vitesseRotation * Time.deltaTime, 0);
        //        }
        //        if (press2 == 1)
        //        {
        //            transform.Rotate(0, 0, vitesseRotation * Time.deltaTime);
        //        }
        //        if (press3 == 1)
        //        {
        //            transform.Rotate(vitesseRotation * Time.deltaTime, 0, 0);
        //        }
        //        //SetText
        //  prevTime = timeNow;
    }

    //void SetText()
    //{
    //    text.text = " ";
    //}

    //int abs(int val)
    //{
    //    if (val < 0)
    //    {
    //        return -val;
    //    }
    //    else
    //    {
    //        return val;
    //    }
    //}

    void OnApplicationQuit()
    {
        asar.Terminate();
    }

    public void snapTo()
    {
        Vector3 thumbPos = fingerWorldToRepere(thumb.transform.position);
        Vector3 indexPos = fingerWorldToRepere(index.transform.position);

        Vector3 minVector = Vector3.Min(thumbPos, indexPos);
        Vector3 maxVector = Vector3.Max(thumbPos, indexPos);

        minVector = minVector + Vector3.one / 2f;
        maxVector = maxVector + Vector3.one / 2f;

        int xmax = (int)(1023f * (1f - minVector.x));
        int ymin = (int)(1023f * minVector.y);
        int zmin = (int)(1023f * minVector.z);
        int xmin = (int)(1023f * (1f - maxVector.x));
        int ymax = (int)(1023f * maxVector.y);
        int zmax = (int)(1023f * maxVector.z);
        

        asar.SendMessage(0, xmin);
        asar.SendMessage(2, ymin);
        asar.SendMessage(4, zmin);
        asar.SendMessage(1, xmax);
        asar.SendMessage(3, ymax);
        asar.SendMessage(5, zmax);

        //print("min: " + minVector.ToString("G6") + "max " + maxVector.ToString("G6"));
        //print(xmin.ToString() + ymin.ToString() + zmin.ToString());
    }

    //public void snapToTest(int axe,int min, int max)
    //{
    //    asar.SendMessage(axe, min);
    //    asar.SendMessage(axe+1, max);
    //}

    Vector3 fingerWorldToRepere(Vector3 worldpos)
    {
        return repere.transform.InverseTransformPoint(worldpos);
    }
}