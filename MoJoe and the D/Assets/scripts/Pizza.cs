using UnityEngine;
using System.Collections;

public class Pizza : MonoBehaviour
{
    Vector3 m_startPos;
    Rigidbody m_rigidBody;

	// Use this for initialization
	void Start ()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_startPos = m_rigidBody.transform.position;
	}

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 getStartPos()
    {
        return m_startPos;
    }
}
