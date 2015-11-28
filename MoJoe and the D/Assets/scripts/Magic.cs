﻿using UnityEngine;
using System.Collections;

public class Magic : MonoBehaviour
{
    Player.state[] canKill = new Player.state[] { Player.state.scissors, Player.state.rock, Player.state.paper };

    //Casters variables
    public GameObject m_magicker;
    public Player.state m_playerState;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject != m_magicker)
        {
            Player.state otherPlayerState = other.GetComponent<Player>().getState();
 
            if (otherPlayerState == canKill[(int)m_playerState] || otherPlayerState == Player.state.none)
            {
                /* KILL */
                Debug.Log("THEIR STATE:" + otherPlayerState + "OUR STATE:" + m_playerState);
                other.transform.position = other.gameObject.GetComponent<Player>().getStartPos();
            }

            else if (otherPlayerState == m_playerState)
            {
                // bounce
                Debug.Log("BOUNCE!! THEIR STATE:" + otherPlayerState + "OUR STATE:" + m_playerState);
                Vector3 centroid = (other.transform.position + m_magicker.transform.position - new Vector3(0, 1, 0)) / 2.0f;
                other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
                m_magicker.GetComponent<Rigidbody>().AddExplosionForce(25000.0f, centroid, 1.0f);
            }
        }
    }

}
