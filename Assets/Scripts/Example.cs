using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    public GameObject bar;
    bool isOn = true;

    public void isClick()
    {
        if (isOn)
        {
            bar.SetActive(false);
        }
        else
        {
            bar.SetActive(true);
        }
    }
}
