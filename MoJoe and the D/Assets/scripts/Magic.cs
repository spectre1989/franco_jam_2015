using UnityEngine;
using System.Collections;

public class Magic : MonoBehaviour
{
    Player.state[] canKill = new Player.state[] { Player.state.scissors, Player.state.rock, Player.state.paper };

    //Casters variables
    public GameObject m_magicker;
    public Player.state m_playerState;

    //AUDIO
    AudioSource m_source;
    public AudioClip m_rockSound;
    public AudioClip m_paperSound;
    public AudioClip m_scissorSound;
    public AudioClip m_bouceSound;
    AudioClip[] m_winSounds = new AudioClip[3];

    private float m_volLowRange = 0.1f;
    private float m_volHighRange = 0.5f;
    float m_vol;

    // Use this for initialization
    void Start()
    {
        m_source = GetComponent<AudioSource>();
        m_winSounds[0] = m_rockSound;
        m_winSounds[1] = m_paperSound;
        m_winSounds[2] = m_scissorSound;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject != m_magicker)
        {
            if (other.GetComponent<Player>() != null)
            {
                Player.state otherPlayerState = other.GetComponent<Player>().getState();

                if (otherPlayerState == canKill[(int)m_playerState] || otherPlayerState == Player.state.none)
                {
                    /* KILL */
                    other.transform.position = other.gameObject.GetComponent<Player>().getStartPos();
                    m_vol = Random.Range(m_volLowRange, m_volHighRange);
                    m_source.PlayOneShot(m_winSounds[(int)m_playerState], m_vol);
                }

                else if (otherPlayerState == m_playerState)
                {
                    // bounce
                    Vector3 centroid = (other.transform.position + m_magicker.transform.position - new Vector3(0, 1, 0)) / 2.0f;
                    other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
                    m_magicker.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
                }
            }
            else if (other.GetComponent<PlayerMP>() != null)
            {
                PlayerMP magicker = m_magicker.GetComponent<PlayerMP>();
                PlayerMP otherPlayer = other.GetComponent<PlayerMP>();

                if (magicker.isLocalPlayer)
                {
                    if (otherPlayer.getState() == canKill[(int)m_playerState] || otherPlayer.getState() == Player.state.none || otherPlayer.getState() == Player.state.pizza)
                    {
                        // kill
                        Debug.Log("THEIR STATE:" + otherPlayer.getState() + "OUR STATE:" + m_playerState);
                        magicker.Kill(other.gameObject);
                        m_vol = Random.Range(m_volLowRange, m_volHighRange);
                        m_source.PlayOneShot(m_winSounds[(int)m_playerState], m_vol);
                    }
                    else if (otherPlayer.getState() == m_playerState)
                    {
                        // bounce
                        Debug.Log("BOUNCE!! THEIR STATE:" + otherPlayer.getState() + "OUR STATE:" + m_playerState);
                        Vector3 centroid = (other.transform.position + m_magicker.transform.position - new Vector3(0, 1, 0)) / 2.0f;
                        m_magicker.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
                    }
                }
            }
        }
    }

}
