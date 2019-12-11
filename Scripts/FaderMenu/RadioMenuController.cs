using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadioMenuController : MonoBehaviour
{
    [SerializeField]
    public bool Enabled;

    [SerializeField] [Range(0,1)]
    public float FillingValue;

    [SerializeField]
    public Material RadialMaterial;

    List<GameObject> MenuItems = new List<GameObject>();
    public float radius;

    public void PopulateMenuItems(List<string> ls, float xRotation)
    {
        float twoPis = Mathf.PI * 2f;
        float increment = twoPis / ls.Count;

        float step = 0f;
        foreach (var item in ls)
        {
            GameObject tmp = new GameObject();
            
            tmp.transform.parent = gameObject.transform;
            tmp.transform.rotation = Quaternion.Euler(xRotation, 0f, 0f);
            tmp.transform.position = Vector3.zero;
            tmp.AddComponent<TextMesh>();
            tmp.GetComponent<TextMesh>().text = item;
            tmp.GetComponent<TextMesh>().fontSize = 40;
            tmp.GetComponent<TextMesh>().color = Color.black;

            tmp.transform.localScale = Vector3.one * 0.1f; 
            tmp.transform.localPosition = new Vector3(radius * Mathf.Cos(increment * step), radius * Mathf.Sin(increment * step),0f);

            MenuItems.Add(tmp);
            step += 1f;
        }
        
    }

    public void ResetMenuColor(Color resetColor)
    {
        for (int i = 0; i < MenuItems.Count; i++)
        {
            SelectMenuElement(i, resetColor);
        }
    }

    public void SelectMenuElement(int index, Color selectionColor)
    {
        MenuItems[index].GetComponent<TextMesh>().color = selectionColor;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material = new Material(RadialMaterial);
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Renderer>().sharedMaterial.SetFloat("_Loading", FillingValue);
    }
}
