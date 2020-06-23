using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPathLR : MonoBehaviour
{
    LineRenderer Lr;

    void Start()
    {
        Lr = GetComponent<LineRenderer>();
        Lr.SetWidth(0.03f, 0.03f);
        Lr.SetColors(Color.green, Color.green);
        Lr.transform.SetParent(transform);
    }

    void Update()
    {
   
    }

    void OnGUI()
    {
        if (Lr != null)
        {
            Lr.positionCount = 2;


            Lr.SetPosition(0, new Vector3(0, 0, -0.5f));
            Lr.SetPosition(1, new Vector3(200, 200, -0.5f));
        }
    }
}
