using UnityEngine;
using System.Collections;

public class Pizza : MonoBehaviour
{
    Vector3 m_startPos;
    Rigidbody m_rigidBody;
    Player m_holder;
    Player m_lastHolder;

    // Use this for initialization
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_startPos = m_rigidBody.transform.position;
        m_lastHolder = null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 getStartPos()
    {
        return m_startPos;
    }

    public void setHolder(Player _holder)
    {
        m_lastHolder = m_holder;
        m_holder = _holder;
    }
    public Player getHolder() { return m_holder; }
    public Player getLastHolder() { return m_lastHolder; }
}
