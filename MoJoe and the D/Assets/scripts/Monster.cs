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
    bool m_end;

	// Use this for initialization
	void Start ()
    {
        m_timeLeft = 120;
        m_anim = GetComponent<Animator>();
        m_source = GetComponent<AudioSource>();
        m_end = false;
        m_anim.SetTrigger("Idle");
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GameInfo.Instance != null && GameInfo.Instance.CurrentState == GameInfo.State.EndOfGame && !m_end)
        {
            m_anim.SetTrigger("End");
            m_anim.SetTrigger("Idle");
            m_source.PlayOneShot(m_killSound);
            m_end = true;
        }
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
            m_anim.SetTrigger("Eat");
            m_source.PlayOneShot(m_eatSound);
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

            m_anim.SetTrigger("Idle");
        }
    }
}
