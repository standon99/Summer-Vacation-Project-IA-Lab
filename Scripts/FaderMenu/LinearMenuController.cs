using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearMenuController : MonoBehaviour
{
    [SerializeField]
    public bool Enabled;

    [SerializeField]
    [Range(0, 1)]
    public float FillingValue;

    [SerializeField]
    public Material LinearMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material = new Material(LinearMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().sharedMaterial.SetFloat("_Loading", FillingValue);
        //if (LinearMaterial != null)
        //    LinearMaterial.SetFloat("_Loading", FillingValue);
    }
}
