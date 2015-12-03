using UnityEngine;
using System.Collections;

public class AlwaysFaceCamera : MonoBehaviour 
{
    private void LateUpdate()
    {
        this.transform.LookAt(Camera.main.transform);
    }
}
