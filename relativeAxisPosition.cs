using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Math;

public class relativeAxisPosition : MonoBehaviour
{
    GameObject headset;
    GameObject axis;
    GameObject metaCamera;
    GameObject model;

    public Vector3 headsetPos;
    public Vector3 axisPos;
    public Vector3 camPos;
    public Vector3 distance;
    public Vector3 modelRot;

    // Start is called before the first frame update
    void Start()
    {
        headset = GameObject.Find("Meta2Headset");
        axis = GameObject.Find("Axis");
        metaCamera = GameObject.Find("MetaCameraRig");
        model = GameObject.Find("Model");
    }

    // Update is called once per frame
    void Update()
    {
        headsetPos = headset.transform.position;
        print(headsetPos);
        axisPos = axis.transform.position;
        print(axisPos);
        camPos = metaCamera.transform.position;
        print(camPos);
        /*

        distance.x = -(headsetPos.y - axisPos.y);
        distance.y = -(headsetPos.z - axisPos.z);
        distance.z = -(headsetPos.x - axisPos.x);
        */

        distance.x = (headsetPos.x - axisPos.x);
        distance.y = (headsetPos.y - axisPos.y);
        distance.z = (headsetPos.z - axisPos.z);

        //model.transform.position = new Vector3(camPos.y + distance.y, camPos.z + distance.z, camPos.x + distance.x);
        model.transform.position = new Vector3(camPos.x + distance.x, -(camPos.z + distance.z), -(camPos.y + distance.y));
        modelRot = new Vector3 (-axis.transform.eulerAngles.y, -axis.transform.eulerAngles.z, axis.transform.eulerAngles.x);
        model.transform.eulerAngles = modelRot;
    }

}

/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Math;

public class relativeAxisPosition : MonoBehaviour
{
    GameObject headset;
    GameObject axis;
    GameObject metaCamera;
    GameObject model;

    public Vector3 headsetPos;
    public Vector3 axisPos;
    public Vector3 camPos;
    public Vector3 distance;
    public Vector3 modelRot;

    // Start is called before the first frame update
    void Start()
    {
        headset = GameObject.Find("Meta2Headset");
        axis = GameObject.Find("Axis");
        metaCamera = GameObject.Find("MetaCameraRig");
        model = GameObject.Find("Model");
    }

    // Update is called once per frame
    void Update()
    {
        headsetPos = headset.transform.position;
        print(headsetPos);
        axisPos = axis.transform.position;
        print(axisPos);
        camPos = metaCamera.transform.position;
        print(camPos);
        

        distance.x = -(headsetPos.y - axisPos.y);
        distance.y = -(headsetPos.z - axisPos.z);
        distance.z = -(headsetPos.x - axisPos.x);

        /*
        distance.x = (headsetPos.x - axisPos.x);
        distance.y = (headsetPos.y - axisPos.y);
        distance.z = (headsetPos.z - axisPos.z);
        */
/*
//model.transform.position = new Vector3(camPos.y + distance.y, camPos.z + distance.z, camPos.x + distance.x);
model.transform.position = new Vector3(distance.y, distance.z, distance.x);
//model.transform.localPosition = new Vector3(camPos.x + distance.x, camPos.y + distance.y, camPos.z + distance.z);
modelRot = new Vector3(axis.transform.eulerAngles.z, axis.transform.eulerAngles.x, axis.transform.eulerAngles.y);
model.transform.eulerAngles = modelRot;
    }
    //localEulerAngles

}

 */