using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class sliderManipulation : MonoBehaviour
{
    public int baud = 2000000;
    public SerialPort port;
    public SerialPort port2;
    // Start is called before the first frame update
    void Start()
    {
        port = new SerialPort("COM21", baud);
        port.ReadTimeout = 10;
        port.WriteTimeout = 10;

        port2 = new SerialPort("COM24", baud);
        port2.ReadTimeout = 10;
        port2.WriteTimeout = 10;
        port2.Open();
        port.Open();
    }


    // Update is called once per frame
    void Update()
    {
//port.Open();
        string message = "Hello World";
        port.WriteLine(message);
        //port.Close();
        print("MESSAGE SENT");

       // port2.Open();
        port2.Write(message);
        //port2.Close();
       // print("MESSAGE BT SENT");

    }
}
