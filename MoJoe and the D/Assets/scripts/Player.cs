using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
    //MOVING
    public float m_startPos;
	public float m_speed;
	public float m_jumpSpeed;
	Rigidbody m_playerRigidbody;

    public KeyCode m_LeftKey;
    public KeyCode m_rightKey;
    public KeyCode m_jumpKey;
    public KeyCode m_rockKey;
    public KeyCode m_paperKey;
    public KeyCode m_scissorsKey;

    //MAGIC
    public enum state {rock, paper, scissors,none};
    private state m_state;
    private GameObject m_magic;

    public GameObject magicObject;
    private MaterialPropertyBlock magicMaterial;
    public Transform magicSpawn;
    public float magicRate;
    private float nextMagic;

    //POINTS
    private int m_points;

    // Use this for initialization
    void Start () 
	{
		m_playerRigidbody = GetComponent <Rigidbody> ();
        m_state = state.none;
	}

    // Update is called once per frame
    void Update()
    {
        Fire ();
        checkCollisions();
    }

    //Fixed update for rigid body
    void FixedUpdate () 
	{
		Move ();
	}
		
	void Move ()
	{
		// Set the movement vector based on the input.
		Vector3 force = new Vector3 ();
		Vector3 movement = new Vector3 ();
        Quaternion rotation = new Quaternion();
       
        if (Input.GetKey(m_LeftKey))
        {
            movement += new Vector3(-1.0f,0,0);
            bool grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.4f, 0));
            if (grounded) { m_playerRigidbody.AddForce(new Vector3(0, 2000, 0));}
            rotation = Quaternion.Euler(new Vector3(0.0f, -90.0f, 0.0f));
            m_playerRigidbody.MoveRotation(rotation);
        }

        if (Input.GetKey(m_rightKey))
        {
            movement += new Vector3(1.0f,0,0);
            bool grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.4f, 0));
            if (grounded) { m_playerRigidbody.AddForce(new Vector3(0, 2000, 0));}
            rotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));
            m_playerRigidbody.MoveRotation(rotation);
        }

        if (Input.GetKeyDown(m_jumpKey))
        {
            bool grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 1.0f, 0));
            if (grounded) { force += new Vector3(0, 1.0f, 0); }
        }

        if (Input.anyKey == false && Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.4f, 0)))
        {
            m_playerRigidbody.velocity *= 0.5f;
        }
		
		// Normalise the movement vector and make it proportional to the speed per second.
		movement = movement.normalized * m_speed * Time.deltaTime;
		force = force.normalized * m_jumpSpeed;

        // Move the player to it's current position plus the movement.
        m_playerRigidbody.MovePosition(transform.position + movement);
        m_playerRigidbody.AddForce (force);
        if (m_state!=state.none) {m_magic.transform.position = m_playerRigidbody.transform.position;}

        //make sure never any angular velociy or z motion
        m_playerRigidbody.angularVelocity = new Vector3();
        m_playerRigidbody.position.Set(m_playerRigidbody.position.x, m_playerRigidbody.position.y, 0);
    }

	void Fire()
	{
		//Firing
		if (Input.GetKey (m_rockKey)) {/*CHARGE*/}

        /*ROCK*/
        if (Input.GetKey(m_rockKey) && m_state!=state.rock)
        {
            m_state = state.rock;
            Color colour = new Color(0, 0.87f, 1);

            //Create magic sphere
            DestroyObject(m_magic);
            m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
            m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
            Magic magicscript = m_magic.GetComponent<Magic>();
            magicscript.m_magicker = this.gameObject;
            magicscript.m_playerState = this.m_state;

        }

        if (Input.GetKeyUp(m_rockKey))
        {
            m_state = state.none;
            DestroyObject(m_magic);
        }

        /*PAPER*/
        if (Input.GetKey(m_paperKey) && m_state != state.paper)
        {
            m_state = state.paper;
            Color colour = new Color(1, 0.81f, 0);

            DestroyObject(m_magic);
            m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
            m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
            Magic magicscript = m_magic.GetComponent<Magic>();
            magicscript.m_magicker = this.gameObject;
            magicscript.m_playerState = this.m_state;
        }

        if (Input.GetKeyUp(m_paperKey))
        {
            m_state = state.none;
            DestroyObject(m_magic);
        }

        /*SCISSORS*/
        if (Input.GetKey(m_scissorsKey) && m_state != state.paper)
        {
            m_state = state.scissors;
            Color colour = new Color(1, 0, 0.6f);

            DestroyObject(m_magic);
            m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
            m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
            Magic magicscript = m_magic.GetComponent<Magic>();
            magicscript.m_magicker = this.gameObject;
            magicscript.m_playerState = this.m_state;
        }

        if (Input.GetKeyUp(m_scissorsKey))
        {
            m_state = state.none;
            DestroyObject(m_magic);
        }
    }

    void checkCollisions()
    {
        
    }

    public state getState()
    {
        return m_state;
    }

}