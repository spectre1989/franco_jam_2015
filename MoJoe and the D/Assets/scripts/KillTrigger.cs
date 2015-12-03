using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class KillTrigger : MonoBehaviour 
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Pizza>() != null)
        {
            if (NetworkServer.active)
            {
                GameInfo.Instance.PizzaFellOffSide();
            }
        }
        else if (other.GetComponent<PlayerMP>() != null)
        {
            if (other.GetComponent<PlayerMP>().isLocalPlayer)
            {
                other.GetComponent<PlayerMP>().FellOffSide();
            }
        }
    }
}
