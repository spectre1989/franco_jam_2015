using UnityEngine;
using System.Collections;

public class Magic : MonoBehaviour
{
    Player.state[] canKill = new Player.state[] { Player.state.scissors, Player.state.rock, Player.state.paper };

    //Casters variables
    public GameObject m_magicker;
    public Player.state m_playerState;

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
                }

                else if (otherPlayerState == m_playerState)
                {
                    // bounce
                    Vector3 centroid = (other.transform.position + m_magicker.transform.position - new Vector3(0, 1, 0)) / 2.0f;
                    other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
                    m_magicker.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
                    SoundManager.Instance.CreateSound(SoundManager.PlayerSoundType.Bump, m_magicker.GetComponent<Player>().m_playerNum, this.transform.position);
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
                        magicker.Kill(other.gameObject);

                        SoundManager.SoundType soundType = SoundManager.SoundType.Paper;
                        switch (m_playerState)
                        {
                            case Player.state.paper:
                                soundType = SoundManager.SoundType.Paper;
                                break;

                            case Player.state.rock:
                                soundType = SoundManager.SoundType.Rock;
                                break;

                            case Player.state.scissors:
                                soundType = SoundManager.SoundType.Scissors;
                                break;

                            default:
                                Debug.LogWarning("player killed by " + m_playerState + "????");
                                break;
                        }

                        magicker.CreateNetworkSound(soundType, this.transform.position);
                    }
                    else if (otherPlayer.getState() == m_playerState)
                    {
                        // bounce
                        Vector3 centroid = (other.transform.position + m_magicker.transform.position - new Vector3(0, 1, 0)) / 2.0f;
                        m_magicker.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);

                        magicker.CreateNetworkSound(SoundManager.PlayerSoundType.Bump);
                    }
                }
            }
        }
    }

}
