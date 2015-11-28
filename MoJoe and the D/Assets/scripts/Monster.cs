using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {
    float m_timeLeft;


	// Use this for initialization
	void Start ()
    {
        m_timeLeft = 120;
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Kill player
            if (other.GetComponent<Player>().getItem() != null)
            {
                other.GetComponent<Player>().resetItem();
                m_timeLeft += 30.0f;
            }
            other.GetComponent<Rigidbody>().transform.position = other.GetComponent<Player>().getStartPos();
        }

        if (other.gameObject.CompareTag("Item"))
        {
            other.GetComponent<Rigidbody>().transform.position = other.GetComponent<Pizza>().getStartPos();
            other.GetComponent<Player>().setState(Player.state.none);
            m_timeLeft += 30.0f;
        }
    }
}
