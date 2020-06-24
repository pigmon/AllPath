using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TruckScript : MonoBehaviour
{
    public Text TruckID;
    void Start()
    {
        TruckID = transform.Find("TruckID").GetComponent<Text>();
    }

    public void SetTruckID(string _id)
    {
        if (TruckID == null)
            TruckID = transform.Find("TruckID").GetComponent<Text>();

        TruckID.text = _id;
    }
}
