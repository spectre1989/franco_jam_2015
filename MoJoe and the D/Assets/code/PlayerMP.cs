using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

public class PlayerMP : NetworkBehaviour
{
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
    private Player.state m_state;
    private GameObject m_magic;

    public GameObject magicObject;
    private MaterialPropertyBlock magicMaterial;
    public Transform magicSpawn;
    public float magicRate;
    private float nextMagic;

    //POINTS
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

    // Shoehorned in from TestPlayer
    [SerializeField]
    private float sendInterval;
    [SerializeField]
    private float lerpDelay;
    [SerializeField]
    private TextMesh nameTextMesh;
    [SerializeField]
    private Vector3 cameraOffset;
    [SerializeField]
    private float cameraSmoothing;
    [SerializeField]
    private Vector2 cameraXRotationLimits;
    [SerializeField]
    private Vector2 cameraYRotationLimits;

    // Network stuff
    private struct SynchedPosition
    {
        public float t;
        public Vector3 position;
        public Quaternion rotation;

        public SynchedPosition(float t, Vector3 position, Quaternion rotation)
        {
            this.t = t;
            this.position = position;
            this.rotation = rotation;
        }
    }

    [SyncVar(hook = "OnSynchName")]
    private String synchedName = "";
    [SyncVar(hook = "OnSynchPosition")]
    private SynchedPosition synchedPosition;
    private Queue<SynchedPosition> synchedPositions = new Queue<SynchedPosition>();
    private SynchedPosition interpolateFromSynchedPosition;
    private float targetLerpTime;
    [SyncVar]
    private Player.state synchedState = Player.state.none;
    [SyncVar]
    private bool synchedHasPizza = false;
    [SyncVar]
    public int synchedPlayerNum = 0;

    // Use this for initialization
    void Start()
    {
        m_playerRigidbody = GetComponent<Rigidbody>();
        m_state = Player.state.none;
        m_startPos = m_playerRigidbody.position;

        m_canDoubleJump = true;
        m_jumping = false;

        m_lastDir = 0;

        if (this.isLocalPlayer)
        {
            this.CmdSetPlayerName((NetworkManager.singleton as CustomNetworkManager).playerName);

            //AUDIO
            m_source = GetComponent<AudioSource>();
            m_landWait = Time.time;

            m_source = GetComponent<AudioSource>();
            m_landWait = Time.time;

            string[] names = new string[] { "Dave", "Joe", "Mohrag" };
            m_boop = (AudioClip)Resources.Load("audio/SFX_Walk_" + (synchedPlayerNum + 1) as string);
            m_jump = (AudioClip)Resources.Load("audio/VO_" + names[synchedPlayerNum] + "_Gulp_1");
            m_land = (AudioClip)Resources.Load("audio/VO_" + names[synchedPlayerNum] + "_Oof_1");
            m_deathSound = (AudioClip)Resources.Load("audio/VO_" + names[synchedPlayerNum] + "_Ugh_1");
            m_winSound = (AudioClip)Resources.Load("audio/VO_" + names[synchedPlayerNum] + "_Rah_1");
            Debug.Log(m_boop);
        }
        else
        {
            // For non-local player, set as kinematic so position is just network synchronised
            this.m_playerRigidbody.isKinematic = true;
            this.interpolateFromSynchedPosition.position = this.transform.position;
            this.nameTextMesh.text = this.synchedName;
        }
    }

    private bool isQuitting = false;

    private void OnApplicationQuit()
    {
        this.isQuitting = true;
    }

    private void OnDestroy()
    {
        if (this.isQuitting == true)
        {
            return;
        }

        if (this.m_magic != null)
        {
            Destroy(this.m_magic);
            this.m_magic = null;
        }

        OneShotAudioClip.Create(this.transform.position, this.m_deathSound);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isLocalPlayer)
        {
            Fire();
        }
        else
        {
            // Interpolate position
            while (this.synchedPositions.Count > 0 && this.synchedPositions.Peek().t <= this.targetLerpTime)
            {
                this.interpolateFromSynchedPosition = this.synchedPositions.Dequeue();
            }

            if (this.synchedPositions.Count > 0)
            {
                float t = Mathf.InverseLerp(this.interpolateFromSynchedPosition.t, this.synchedPositions.Peek().t, this.targetLerpTime);
                this.transform.position = Vector3.Lerp(this.interpolateFromSynchedPosition.position, this.synchedPositions.Peek().position, t);
                this.transform.rotation = Quaternion.Slerp(this.interpolateFromSynchedPosition.rotation, this.synchedPositions.Peek().rotation, t);
            }
            else
            {
                this.transform.position = this.interpolateFromSynchedPosition.position;
                this.transform.rotation = this.interpolateFromSynchedPosition.rotation;
            }

            this.targetLerpTime += Time.deltaTime;

            if (this.m_magic != null)
            {
                this.m_magic.transform.position = this.transform.position;
            }

            // State
            UpdateState(this.synchedState);
        }

        if (this.m_state == Player.state.pizza)
        {
            if (this.synchedHasPizza)
            {
                //Show arms and pizza
                transform.Find("wizard_arm").gameObject.SetActive(true);
                transform.Find("wizard_arm_2").gameObject.SetActive(true);
                transform.Find("pizza").gameObject.SetActive(true);
            }
            else
            {
                transform.Find("wizard_arm").gameObject.SetActive(false);
                transform.Find("wizard_arm_2").gameObject.SetActive(false);
                transform.Find("pizza").gameObject.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (this.isLocalPlayer)
        {
            //Vector3 targetPosition = this.transform.position + this.cameraOffset;
            //Vector3 toTarget = targetPosition - Camera.main.transform.position;
            //float smoothing = Mathf.Min(this.cameraSmoothing * Time.deltaTime, 1.0f);
            //Camera.main.transform.position = Camera.main.transform.position + (toTarget * smoothing);
            Camera.main.transform.LookAt(this.transform.position);

            Vector3 euler = Camera.main.transform.rotation.eulerAngles;
            while (euler.x > 180)
            {
                euler.x -= 360;
            }
            while (euler.x < -180)
            {
                euler.x += 360;
            }
            while (euler.y > 180)
            {
                euler.y -= 360;
            }
            while (euler.y < -180)
            {
                euler.y += 350;
            }

            if (euler.x < this.cameraXRotationLimits.x)
            {
                euler.x = this.cameraXRotationLimits.x;
            }
            else if (euler.x > this.cameraXRotationLimits.y)
            {
                euler.x = this.cameraXRotationLimits.y;
            }

            if (euler.y < this.cameraYRotationLimits.x)
            {
                euler.y = this.cameraYRotationLimits.x;
            }
            else if (euler.y > this.cameraYRotationLimits.y)
            {
                euler.y = this.cameraYRotationLimits.y;
            }

            Camera.main.transform.rotation = Quaternion.Euler(euler);
        }
    }

    //Fixed update for rigid body
    void FixedUpdate()
    {
        if (this.isLocalPlayer)
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
                        m_vol = UnityEngine.Random.Range(m_volLowRange, m_volHighRange);
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
                grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0, 0.8f, 0));
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
                        m_vol = UnityEngine.Random.Range(m_volLowRange, m_volHighRange);
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
                            m_vol = UnityEngine.Random.Range(m_volLowRange, m_volHighRange);
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
                    m_vol = UnityEngine.Random.Range(m_volLowRange, m_volHighRange);
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

            this.CmdUpdatePosition(new SynchedPosition(Time.time, this.transform.position, this.transform.rotation));
        }
    }

    void Fire()
    {
        /*ROCK*/
        if (Input.GetButton("Fire1") && m_state != Player.state.rock)
        {
            if (this.synchedHasPizza)
            {
                dropItem(new Vector3());
                m_nextThrow = Time.time + 5.0f;
            }

            UpdateState(Player.state.rock);
        }

        if (Input.GetButtonUp("Fire1"))
        {
            UpdateState(Player.state.none);
        }

        /*PAPER*/
        if (Input.GetButton("Fire2") && m_state != Player.state.paper)
        {
            if (this.synchedHasPizza)
            {
                dropItem(new Vector3());
                m_nextThrow = Time.time + 5.0f;
            }

            UpdateState(Player.state.paper);
        }

        if (Input.GetButtonUp("Fire2"))
        {
            UpdateState(Player.state.none);
        }

        /*SCISSORS*/
        if (Input.GetButton("Fire3") && m_state != Player.state.scissors)
        {
            if (this.synchedHasPizza)
            {
                dropItem(new Vector3());
                m_nextThrow = Time.time + 5.0f;
            }

            UpdateState(Player.state.scissors);
        }

        if (Input.GetButtonUp("Fire3"))
        {
            UpdateState(Player.state.none);
        }

        /*PIZZA!*/
        if ((Input.GetAxis("RTrigger") == 1) && m_state != Player.state.pizza && Time.time > m_nextThrow)
        {
            UpdateState(Player.state.pizza);
        }

        if ((Input.GetAxis("RTrigger") == 0) && m_state == Player.state.pizza)
        {
            if (this.synchedHasPizza)
            {
                dropItem(new Vector3());
            }

            UpdateState(Player.state.none);
        }

        /*THROW*/
        if (Input.GetButton("RButton"))
        {
            if (m_state == Player.state.pizza && this.synchedHasPizza)
            {
                m_nextThrow = Time.time + 5.0f;
                dropItem(new Vector3(m_lastDir * 500, 0, 0));
            }
        }


    }

    private void UpdateState(Player.state state)
    {
        if (m_state == state)
        {
            return;
        }

        Player.state oldState = m_state;
        m_state = state;

        if (this.isLocalPlayer)
        {
            // send to server
            CmdUpdateState(m_state);
        }

        transform.Find("stone").gameObject.SetActive(false);
        transform.Find("paper").gameObject.SetActive(false);
        transform.Find("scissors").gameObject.SetActive(false);
        transform.Find("pizza").gameObject.SetActive(false);

        switch (m_state)
        {
            case Player.state.rock:
                {
                    Color colour = new Color(0, 0.87f, 1);

                    //Create magic sphere
                    DestroyObject(m_magic);

                    m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
                    m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
                    Magic magicscript = m_magic.GetComponent<Magic>();
                    magicscript.m_magicker = this.gameObject;
                    magicscript.m_playerState = this.m_state;

                    //Show arms and rock
                    transform.Find("wizard_arm").gameObject.SetActive(true);
                    transform.Find("wizard_arm_2").gameObject.SetActive(true);
                    transform.Find("stone").gameObject.SetActive(true);
                    break;
                }

            case Player.state.paper:
                {
                    Color colour = new Color(1, 0.81f, 0);

                    DestroyObject(m_magic);

                    m_magic = Instantiate(magicObject, magicSpawn.position, magicSpawn.rotation) as GameObject;
                    m_magic.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Colour", colour);
                    Magic magicscript = m_magic.GetComponent<Magic>();
                    magicscript.m_magicker = this.gameObject;
                    magicscript.m_playerState = this.m_state;

                    //Show arms and rock
                    transform.Find("wizard_arm").gameObject.SetActive(true);
                    transform.Find("wizard_arm_2").gameObject.SetActive(true);
                    transform.Find("paper").gameObject.SetActive(true);
                    break;
                }

            case Player.state.scissors:
                {
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
                    break;
                }

            case Player.state.pizza:
                Destroy(m_magic);

                transform.Find("wizard_arm").gameObject.SetActive(true);
                transform.Find("wizard_arm_2").gameObject.SetActive(true);

                break;

            case Player.state.none:
                DestroyObject(m_magic);

                //Hide arms and rock
                transform.Find("wizard_arm").gameObject.SetActive(false);
                transform.Find("wizard_arm_2").gameObject.SetActive(false);
                break;
        }
        
    }

    void OnTriggerStay(Collider other)
    {
        if (this.isLocalPlayer)
        {
            if (other.gameObject.CompareTag("Item") && m_state == Player.state.pizza)
            {
                this.CmdPickUpPizza();
            }
        }
    }

    public Player.state getState()
    {
        return m_state;
    }
    public void setState(Player.state _state)
    {
        m_state = _state;
    }

    public Vector3 getStartPos()
    {
        return m_startPos;
    }

    public void dropItem(Vector3 _force)
    {
        Vector3 pizzaSpawnPos = this.transform.position + new Vector3(m_lastDir * 2, 1.0f, 0);
        this.CmdDropPizza(pizzaSpawnPos, _force);

        UpdateState(Player.state.none);
    }

    public int getPoints() { return m_points; }
    public void addPoints(int _points) { m_points += _points; }
    public void removePoints(int _points) { m_points -= _points; }

    // Synch Hooks
    private void OnSynchName(String name)
    {
        this.synchedName = name;
        this.nameTextMesh.text = name;
    }

    private void OnSynchPosition(SynchedPosition position)
    {
        if (this.isLocalPlayer == false)
        {
            this.synchedPositions.Enqueue(position);
        }

        this.targetLerpTime = position.t - this.lerpDelay;
    }

    // Commands
    [Command]
    private void CmdSetPlayerName(String name)
    {
        this.synchedName = name;

        GameInfo.Instance.SetPlayerName(this.gameObject, name);
    }

    [Command]
    private void CmdUpdatePosition(SynchedPosition position)
    {
        this.synchedPosition = position;
    }

    [Command]
    private void CmdUpdateState(Player.state state)
    {
        this.synchedState = state;
    }

    [Command]
    private void CmdKill(NetworkInstanceId netId)
    {
        GameObject playerGameObject = NetworkServer.FindLocalObject(netId);
        if (playerGameObject != null)
        {
            GameInfo.Instance.AddToRespawnQueue(playerGameObject.GetComponent<NetworkIdentity>().connectionToClient);
            NetworkServer.Destroy(playerGameObject);
            GameInfo.Instance.ChangeScore(this.gameObject, 1);
        }
    }

    [Command]
    private void CmdDropPizza(Vector3 position, Vector3 force)
    {
        if (this.synchedHasPizza == false)
        {
            return;
        }

        this.synchedHasPizza = false;

        GameInfo.Instance.SpawnPizza(position, force);
    }

    [Command]
    private void CmdPickUpPizza()
    {
        if (this.synchedHasPizza == true)
        {
            return;
        }

        if (GameInfo.Instance.TryPickUpPizza(this.connectionToClient))
        {
            this.synchedHasPizza = true;
        }
    }

    [Command]
    private void CmdRespawnEatenByMonster()
    {
        GameInfo.Instance.AddToRespawnQueue(this.connectionToClient);
        GameInfo.Instance.ChangeScore(this.gameObject, -1);
        NetworkServer.Destroy(this.gameObject);
    }

    [Command]
    private void CmdRespawnFellOffSide()
    {
        GameInfo.Instance.AddToRespawnQueue(this.connectionToClient);
        GameInfo.Instance.ChangeScore(this.gameObject, -1);
        NetworkServer.Destroy(this.gameObject);
    }

    // NetworkBehaviour overrides
    public override float GetNetworkSendInterval()
    {
        return this.sendInterval;
    }

    // The Unity Gizmo, action pumpo
    private void OnDrawGizmos()
    {
        if (this.isLocalPlayer == false)
        {
            foreach (SynchedPosition synchedPosition in this.synchedPositions)
            {
                Gizmos.DrawSphere(synchedPosition.position, 0.2f);
            }
        }
    }

    public void Kill(GameObject player)
    {
        CmdKill(player.GetComponent<NetworkIdentity>().netId);
    }

    public void EatenByMonster()
    {
        CmdRespawnEatenByMonster();
    }

    public void FellOffSide()
    {
        CmdRespawnFellOffSide();
    }
}