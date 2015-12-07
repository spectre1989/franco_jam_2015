using UnityEngine;
using UnityEngine.Networking;

public class Monster : MonoBehaviour
{
    //ANIM
    Animator m_anim;

    //AUDIO
    bool m_end;

    // Use this for initialization
    void Start()
    {
        m_anim = GetComponent<Animator>();
        m_end = false;
        m_anim.SetTrigger("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameInfo.Instance != null && GameInfo.Instance.CurrentState == GameInfo.State.EndOfGame && !m_end)
        {
            m_anim.SetTrigger("End");
            m_anim.SetTrigger("Idle");
            SoundManager.Instance.CreateSound(SoundManager.SoundType.MonsterBwargh, this.transform.position);
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
                }
                other.GetComponent<Rigidbody>().transform.position = other.GetComponent<Player>().getStartPos();
            }
            else if (other.GetComponent<PlayerMP>() != null)
            {
                SoundManager.Instance.CreateSound(SoundManager.SoundType.MonsterNom, this.transform.position);

                if (other.GetComponent<PlayerMP>().isLocalPlayer)
                {
                    other.GetComponent<PlayerMP>().EatenByMonster();
                }
            }
        }

        if (other.gameObject.CompareTag("Item"))
        {
            m_anim.SetTrigger("Eat");
            SoundManager.Instance.CreateSound(SoundManager.SoundType.MonsterNom, this.transform.position);
            if (NetworkServer.active)
            {
                GameInfo.Instance.PizzaEaten();
            }
            else if (NetworkManager.singleton == null)
            {
                Player player = other.GetComponent<Pizza>().getLastHolder();
                other.GetComponent<Rigidbody>().velocity = new Vector3();
                other.transform.position = other.GetComponent<Pizza>().getStartPos();
                other = null;
            }

            m_anim.SetTrigger("Idle");
        }
    }
}