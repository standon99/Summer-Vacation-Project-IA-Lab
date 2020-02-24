using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class receive_test : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM14", 9600);

    void Start()
    {
        sp.Open();
        sp.ReadTimeout = 10;
    }

    void Update()
    {
        try
        {
            print(sp.ReadLine());
        }
        catch (System.Exception)
        {
        }
    }
}
