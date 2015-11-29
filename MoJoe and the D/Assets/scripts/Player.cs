using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    //Player Number
    public int m_playerNum;

    //MOVING
    private Vector3 m_startPos;
    public float m_speed;
    public float m_jumpSpeed;
    public float m_bounceForce;
    Rigidbody m_playerRigidbody;
    bool m_canDoubleJump;
    float m_lastDir;
    float m_nextThrow;
    bool m_jumping;

    //MAGIC
    public enum state { rock, paper, scissors, none, pizza };
    private state m_state;
    private GameObject m_magic;

    public GameObject magicObject;
    private MaterialPropertyBlock magicMaterial;
    public Transform magicSpawn;
    public float magicRate;
    private float nextMagic;

    //POINTS
    private GameObject m_item;
    private int m_points;

    //AUDIO
    AudioClip m_boop;
    AudioClip m_jump;
    AudioClip m_land;
    AudioClip m_deathSound;
    AudioClip m_winSound;

    private AudioSource m_source;
    private float m_volLowRange = 0.1f;
    private float m_volHighRange = 0.5f;
    float m_vol;

    bool m_booped;
    bool m_landing;
    float m_landWait;


    // Use this for initialization
    void Start()
    {
        m_playerRigidbody = GetComponent<Rigidbody>();
        m_state = state.none;
        m_startPos = m_playerRigidbody.position;

        m_canDoubleJump = true;
        m_jumping = false;

        m_lastDir = 0;

        //AUDIO
        m_source = GetComponent<AudioSource>();
        m_landWait = Time.time;

        m_source = GetComponent<AudioSource>();
        m_landWait = Time.time;

        string[] names = new string[] { "Dave", "Joe", "Mohrag" };
        m_boop = (AudioClip)Resources.Load("audio/SFX_Walk_" + (m_playerNum + 1) as string);
        m_jump = (AudioClip)Resources.Load("audio/VO_" + names[m_playerNum] + "_Gulp_1");
        m_land = (AudioClip)Resources.Load("audio/VO_" + names[m_playerNum] + "_Oof_1");
        m_deathSound = (AudioClip)Resources.Load("audio/VO_" + names[m_playerNum] + "_Ugh_1");
        m_winSound = (AudioClip)Resources.Load("audio/VO_" + names[m_playerNum] + "_Rah_1");
        Debug.Log(m_boop);

    }

    // Update is called once per frame
    void Update()
    {
        Fire();
    }

    //Fixed update for rigid body
    void FixedUpdate()
    {
        // Set the movement vector based on the input.
        Vector3 force = new Vector3();
        Vector3 bounce = new Vector3();
        Vector3 movement = new Vector3();
        Quaternion rotation = new Quaternion();
        bool grounded;

        m_speed = 5 + (Input.GetAxis("LTrigger") * 10);
        movement = Input.GetAxis("Horizontal") * new Vector3(1.0f, 0, 0);

        if (movement.magnitude > 0)
        {
            grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.4f, 0));
            if (grounded)
            {
                bounce += new Vector3(0, 1.0f, 0);
                m_canDoubleJump = true;

                if (!m_booped)
                {
                    m_vol = Random.Range(m_volLowRange, m_volHighRange);
                    m_source.PlayOneShot(m_boop, m_vol);
                    m_booped = true;
                }
            }

            if (!grounded) { m_booped = false; }

            rotation = Quaternion.Euler(Input.GetAxis("Horizontal") * new Vector3(0.0f, -90.0f, 0.0f) + new Vector3(0, 180, 0));
            m_playerRigidbody.MoveRotation(rotation);
            m_lastDir = Input.GetAxis("Horizontal");
        }

        if (Input.GetButton("Jump"))
        {
            if (!m_jumping)
            {
                grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.8f, 0));
                if (grounded)
                {
                    m_playerRigidbody.velocity = new Vector3();
                    force = new Vector3(0, 1, 0);
                    force = force.normalized * m_jumpSpeed;
                    m_playerRigidbody.AddForce(force);
                    m_canDoubleJump = true;
                    m_jumping = true;
                    m_landing = true;

                    //Audio
                    m_vol = Random.Range(m_volLowRange, m_volHighRange);
                    m_source.PlayOneShot(m_jump, m_vol);
                    m_landWait = Time.time + 0.3f;
                }

                else
                {
                    if (m_canDoubleJump)
                    {
                        force = new Vector3(0, 1, 0);
                        force = force.normalized * m_jumpSpeed;
                        m_playerRigidbody.AddForce(force);
                        m_canDoubleJump = false;
                        m_jumping = true;
                        m_landing = true;

                        //Audio
                        m_vol = Random.Range(m_volLowRange, m_volHighRange);
                        m_source.PlayOneShot(m_jump, m_vol);
                        m_landWait = Time.time + 0.3f;
                    }
                }

            }
        }

        if (m_landing)
        {
            if (Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.4f, 0)) && Time.time > m_landWait)
            {
                m_vol = Random.Range(m_volLowRange, m_volHighRange);
                m_source.PlayOneShot(m_land, m_vol);
                m_landing = false;
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            m_jumping = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            /*grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.8f, 0));
            if (grounded)
            {
                force += new Vector3(0, 1, 0);
                m_canDobuleJump = true;
                Debug.Log(m_canDobuleJump);
            }
            else
            {
                if (m_canDobuleJump)
                {
                    Debug.Log("DOUBLE JUMP BABY");
                    force += new Vector3(0, 1, 0);
                    m_canDobuleJump = false;
                }
            }*/
        }

        // Normalise the movement vector and make it proportional to the speed per second.
        movement = movement.normalized * m_speed * Time.deltaTime;
        bounce = bounce.normalized * m_bounceForce;
        // Move the player to it's current position plus the movement.
        m_playerRigidbody.MovePosition(transform.position + movement);
        m_playerRigidbody.AddForce(bounce);
        if ((int)m_state < 3) { m_magic.transform.position = m_playerRigidbody.transform.position; }
    }

    void Fire()
    {
        /*ROCK*/
        if (Input.GetButton("Fire1") && m_state != state.rock)
        {
            if (m_item != null)
            {
                dropItem(new Vector3());
                m_nextThrow = Time.time + 5.0f;
            }
            m_state = state.rock;

            Color colour = new Color(0, 0.87f, 1);

            //Create magic sphere
            DestroyObject(m_magic);
            transform.Find("scissors").gameObject.SetActive(false);
            transform.Find("paper").gameObject.SetActive(false);

            m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
            m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
            Magic magicscript = m_magic.GetComponent<Magic>();
            magicscript.m_magicker = this.gameObject;
            magicscript.m_playerState = this.m_state;

            //Show arms and rock
            transform.Find("wizard_arm").gameObject.SetActive(true);
            transform.Find("wizard_arm_2").gameObject.SetActive(true);
            transform.Find("stone").gameObject.SetActive(true);

        }

        if (Input.GetButtonUp("Fire1"))
        {
            m_state = state.none;
            DestroyObject(m_magic);

            //Hide arms and rock
            transform.Find("wizard_arm").gameObject.SetActive(false);
            transform.Find("wizard_arm_2").gameObject.SetActive(false);
            transform.Find("stone").gameObject.SetActive(false);
        }

        /*PAPER*/
        if (Input.GetButton("Fire2") && m_state != state.paper)
        {
            if (m_item != null)
            {
                dropItem(new Vector3());
                m_nextThrow = Time.time + 5.0f;
            }
            m_state = state.paper;
            Color colour = new Color(1, 0.81f, 0);

            DestroyObject(m_magic);
            transform.Find("scissors").gameObject.SetActive(false);
            transform.Find("stone").gameObject.SetActive(false);

            m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
            m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
            Magic magicscript = m_magic.GetComponent<Magic>();
            magicscript.m_magicker = this.gameObject;
            magicscript.m_playerState = this.m_state;

            //Show arms and rock
            transform.Find("wizard_arm").gameObject.SetActive(true);
            transform.Find("wizard_arm_2").gameObject.SetActive(true);
            transform.Find("paper").gameObject.SetActive(true);
        }

        if (Input.GetButtonUp("Fire2"))
        {
            m_state = state.none;
            DestroyObject(m_magic);

            //Hide arms and rock
            transform.Find("wizard_arm").gameObject.SetActive(false);
            transform.Find("wizard_arm_2").gameObject.SetActive(false);
            transform.Find("paper").gameObject.SetActive(false);
        }

        /*SCISSORS*/
        if (Input.GetButton("Fire3") && m_state != state.paper)
        {
            if (m_item != null)
            {
                dropItem(new Vector3());
                m_nextThrow = Time.time + 5.0f;
            }

            m_state = state.scissors;
            Color colour = new Color(1, 0, 0.6f);

            DestroyObject(m_magic);
            m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
            m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
            Magic magicscript = m_magic.GetComponent<Magic>();
            magicscript.m_magicker = this.gameObject;
            magicscript.m_playerState = this.m_state;

            //Show arms and rock
            transform.Find("wizard_arm").gameObject.SetActive(true);
            transform.Find("wizard_arm_2").gameObject.SetActive(true);
            transform.Find("scissors").gameObject.SetActive(true);
        }

        if (Input.GetButtonUp("Fire3"))
        {
            m_state = state.none;
            DestroyObject(m_magic);

            //Hide arms and rock
            transform.Find("wizard_arm").gameObject.SetActive(false);
            transform.Find("wizard_arm_2").gameObject.SetActive(false);
            transform.Find("scissors").gameObject.SetActive(false);
        }

        /*PIZZA!*/
        if ((Input.GetAxis("RTrigger") == 1) && m_state != state.pizza && Time.time > m_nextThrow)
        {
            m_state = state.pizza;

        }

        if ((Input.GetAxis("RTrigger") == 0) && m_state == state.pizza)
        {
            if (m_item != null)
            {
                dropItem(new Vector3());
            }
            m_state = state.none;
        }

        /*THROW*/
        if (Input.GetButton("RButton"))
        {
            if (m_state == state.pizza && m_item != null)
            {
                m_nextThrow = Time.time + 5.0f;
                dropItem(new Vector3(m_lastDir * 500, 0, 0));
            }
        }


    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Item") && m_state == state.pizza)
        {
            Debug.Log("PICKING UP MA PIZ");
            m_item = other.gameObject;
            other.GetComponent<Pizza>().setHolder(this);
            other.gameObject.SetActive(false);

            //Show arms and pizza
            transform.Find("wizard_arm").gameObject.SetActive(true);
            transform.Find("wizard_arm_2").gameObject.SetActive(true);
            transform.Find("pizza").gameObject.SetActive(true);
        }
    }

    public state getState()
    {
        Debug.Log(name + "OUR STATE IS " + m_state);
        return m_state;
    }

    public void setState(state _state)
    {
        m_state = _state;
    }

    public Vector3 getStartPos()
    {
        return m_startPos;
    }

    public GameObject getItem()
    {
        return m_item;
    }

    public void dropItem(Vector3 _force)
    {
        m_item.GetComponent<Rigidbody>().velocity = new Vector3();
        m_item.transform.position = m_playerRigidbody.position + new Vector3(m_lastDir * 2, 1.0f, 0);
        m_item.GetComponent<Pizza>().setHolder(null);
        m_item.SetActive(true);
        m_item.GetComponent<Rigidbody>().AddForce(_force);
        m_item = null;
        m_state = state.none;

        //Hide arms and rock
        transform.Find("wizard_arm").gameObject.SetActive(false);
        transform.Find("wizard_arm_2").gameObject.SetActive(false);
        transform.Find("pizza").gameObject.SetActive(false);
    }

    public void resetItem()
    {
        m_item.GetComponent<Rigidbody>().velocity = new Vector3();
        m_item.transform.position = m_item.GetComponent<Pizza>().getStartPos();
        m_item.SetActive(true);
        m_item = null;
        m_state = state.none;
    }

    public int getPoints() { return m_points; }
    public void addPoints(int _points) { m_points += _points; }
    public void removePoints(int _points) { m_points -= _points; }

    public void die()
    {
        m_source.PlayOneShot(m_deathSound, 1F);
        transform.position = m_startPos;
    }

    public void win()
    {
        m_source.PlayOneShot(m_winSound, 1F);
    }

}