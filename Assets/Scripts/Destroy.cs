using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    public Boss boss;

    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        if (boss.curHealth <= 0)
        {
            Destroy(this.gameObject);
        }

    }
}
