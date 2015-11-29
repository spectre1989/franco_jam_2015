using UnityEngine;
using UnityEngine.Networking;

public class Monster : MonoBehaviour {
    float m_timeLeft;
    //ANIM
    Animator m_anim;

    //AUDIO
    public AudioClip m_eatSound;
    public AudioClip m_killSound;

    public AudioSource m_source;

	// Use this for initialization
	void Start ()
    {
        m_timeLeft = 120;
        m_anim = GetComponent<Animator>();
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    //if gameinfo.instance.state == 
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<Player>() != null)
            {
                //Kill player
                if (other.GetComponent<Player>().getItem() != null)
                {
                    other.GetComponent<Player>().resetItem();
                    m_timeLeft += 30.0f;
                }
                other.GetComponent<Rigidbody>().transform.position = other.GetComponent<Player>().getStartPos();
            }
            else if (other.GetComponent<PlayerMP>() != null)
            {
                if (other.GetComponent<PlayerMP>().isLocalPlayer)
                {
                    other.GetComponent<PlayerMP>().EatenByMonster();
                }
            }
        }

        if (other.gameObject.CompareTag("Item"))
        {
            Debug.Log("CHOMP");
            m_anim.SetTrigger("Eat");
            if (NetworkServer.active)
            {
                m_anim.SetTrigger("Eat");
                GameInfo.Instance.PizzaEaten();
            }
            else if (NetworkManager.singleton == null)
            {
                Player player = other.GetComponent<Pizza>().getLastHolder();
                other.GetComponent<Rigidbody>().velocity = new Vector3();
                other.transform.position = other.GetComponent<Pizza>().getStartPos();
                other = null;
                m_timeLeft += 30.0f;
            }
        }
    }
}
