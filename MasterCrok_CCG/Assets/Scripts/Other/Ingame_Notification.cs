using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingame_Notification : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
 		gameObject.transform.position = gameObject.transform.position - new Vector3(0f,-1f,0f);       
    }
}
