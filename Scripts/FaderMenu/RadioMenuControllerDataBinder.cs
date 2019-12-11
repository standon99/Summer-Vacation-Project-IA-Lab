using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioMenuControllerDataBinder : MonoBehaviour
{
    [SerializeField]
    public TEST ArduinoFaderAxes;

    [SerializeField]
    public GameObject RadioMenuController0;
    [SerializeField]
    public GameObject RadioMenuController1;
    [SerializeField]
    public GameObject RadioMenuController2;

    [SerializeField]
    public GameObject LinearMenuControllerX0;
    [SerializeField]
    public GameObject LinearMenuControllerX1;
    [SerializeField]
    public GameObject LinearMenuControllerY0;
    [SerializeField]
    public GameObject LinearMenuControllerY1;
    [SerializeField]
    public GameObject LinearMenuControllerZ0;
    [SerializeField]
    public GameObject LinearMenuControllerZ1;

    [SerializeField]
    public float radioMenuLoading;

    [SerializeField]
    public float stepSmoother;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ArduinoFaderAxes != null && RadioMenuController0.GetComponent<RadioMenuController>() != null)
            RadioMenuController0.GetComponent<RadioMenuController>().FillingValue = ArduinoFaderAxes.rotary1 / stepSmoother;

        if (ArduinoFaderAxes != null && RadioMenuController1.GetComponent<RadioMenuController>() != null)
            RadioMenuController1.GetComponent<RadioMenuController>().FillingValue = ArduinoFaderAxes.rotary2 / stepSmoother;

        if (ArduinoFaderAxes != null && RadioMenuController2.GetComponent<RadioMenuController>() != null)
            RadioMenuController2.GetComponent<RadioMenuController>().FillingValue = ArduinoFaderAxes.rotary3 / stepSmoother;

        if (LinearMenuControllerX0 != null && LinearMenuControllerX0.GetComponent<LinearMenuController>() != null)
            LinearMenuControllerX0.GetComponent<LinearMenuController>().FillingValue = ArduinoFaderAxes.x0 / 1023f;

        if (LinearMenuControllerX1 != null && LinearMenuControllerX1.GetComponent<LinearMenuController>() != null)
            LinearMenuControllerX1.GetComponent<LinearMenuController>().FillingValue = ArduinoFaderAxes.x1 / 1023f;

        if (LinearMenuControllerY0 != null && LinearMenuControllerY0.GetComponent<LinearMenuController>() != null)
            LinearMenuControllerY0.GetComponent<LinearMenuController>().FillingValue = ArduinoFaderAxes.y0 / 1023f;

        if (LinearMenuControllerY1 != null && LinearMenuControllerY1.GetComponent<LinearMenuController>() != null)
            LinearMenuControllerY1.GetComponent<LinearMenuController>().FillingValue = ArduinoFaderAxes.y1 / 1023f;

        if (LinearMenuControllerZ0 != null && LinearMenuControllerZ0.GetComponent<LinearMenuController>() != null)
            LinearMenuControllerZ0.GetComponent<LinearMenuController>().FillingValue = ArduinoFaderAxes.z0 / 1023f;

        if (LinearMenuControllerZ1 != null && LinearMenuControllerZ1.GetComponent<LinearMenuController>() != null)
            LinearMenuControllerZ1.GetComponent<LinearMenuController>().FillingValue = ArduinoFaderAxes.z1 / 1023f;


        //if (ArduinoFaderAxes.press2 > 0)
        //    RadioMenuController.active = false;
        //else RadioMenuController.active = true;
    }
}
