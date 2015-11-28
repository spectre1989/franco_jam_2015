using UnityEngine;
using UnityEngine.Networking;
using System;

public class TestPlayer : NetworkBehaviour
{
    // Stuff for editor
    [SerializeField] 
    private KeyCode leftKey;
    [SerializeField]
    private KeyCode rightKey;
    [SerializeField] 
    private KeyCode jumpKey;
    [SerializeField] 
    private KeyCode rockKey;
    [SerializeField] 
    private KeyCode paperKey;
    [SerializeField] 
    private KeyCode scissorsKey;
    [SerializeField]
    private Color rockColour;
    [SerializeField]
    private Color paperColour;
    [SerializeField]
    private Color scissorsColour;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float jumpImpulse;
    [SerializeField]
    private float groundedRayLength;
    [SerializeField]
    private float sendInterval;
    [SerializeField]
    private float networkSmoothing;
    [SerializeField]
    private TextMesh nameTextMesh;

    // Game stuff
    private enum Magic
    {
        None = 0,
        Rock = 1,
        Paper = 2,
        Scissors = 4
    }

    private Rigidbody rigidbody;
    private Material material;
    private Magic magic;

    // Network stuff - Set Once
    private String synchedName;
    // Network Stuff - Continuous
    [SyncVar]
    private Vector3 synchedPosition;

    private void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();

        if (this.isLocalPlayer)
        {
            this.CmdSetPlayerName((NetworkManager.singleton as CustomNetworkManager).playerName);
        }
        else
        {
            // For non-local player, set as kinematic so position is just network synchronised
            this.rigidbody.isKinematic = true;
        }

        Renderer renderer = this.GetComponent<Renderer>();
        this.material = renderer.material; // make copy of material
        renderer.sharedMaterial = this.material; // assign to renderer
    }

    public override float GetNetworkSendInterval()
    {
        return this.sendInterval;
    }

    private void FixedUpdate()
    {
        if (this.isLocalPlayer)
        {
            // X movement
            bool updateXVelocity = false;
            float xVelocity = 0.0f;

            if (Input.GetKey(this.leftKey))
            {
                updateXVelocity = true;
                xVelocity -= this.moveSpeed;
            }
            if (Input.GetKey(this.rightKey))
            {
                updateXVelocity = true;
                xVelocity += this.moveSpeed;
            }

            if (updateXVelocity)
            {
                Vector3 velocity = this.rigidbody.velocity;
                velocity.x = xVelocity;
                this.rigidbody.velocity = velocity;
            }

            // Jumping
            if (Input.GetKeyDown(this.jumpKey))
            {
                RaycastHit[] raycastHits = Physics.RaycastAll(this.transform.position, Vector3.down, this.groundedRayLength);
                foreach (RaycastHit raycastHit in raycastHits)
                {
                    // ignore hits with self
                    if (raycastHit.collider.gameObject != this.gameObject)
                    {
                        this.rigidbody.AddForce(Vector3.up * this.jumpImpulse, ForceMode.Impulse);
                        break;
                    }
                }
            }

            this.CmdUpdatePosition(this.transform.position);
        }
    }

    private void Update()
    {
        if (this.isLocalPlayer)
        {
            int magicFlags = 0;

            if (Input.GetKey(this.rockKey))
            {
                magicFlags |= (int)Magic.Rock;
            }
            if (Input.GetKey(this.paperKey))
            {
                magicFlags |= (int)Magic.Paper;
            }
            if (Input.GetKey(this.scissorsKey))
            {
                magicFlags |= (int)Magic.Scissors;
            }

            // multiple buttons = NOTHING!
            if (magicFlags == (int)Magic.Rock)
            {
                this.magic = Magic.Rock;
                this.material.color = this.rockColour;
            }
            else if (magicFlags == (int)Magic.Paper)
            {
                this.magic = Magic.Paper;
                this.material.color = this.paperColour;
            }
            else if (magicFlags == (int)Magic.Scissors)
            {
                this.magic = Magic.Scissors;
                this.material.color = this.scissorsColour;
            }
            else
            {
                this.magic = Magic.None;
                this.material.color = Color.white;
            }
        }
        else
        {
            Vector3 toTarget = this.synchedPosition - this.transform.position;
            float smoothing = Mathf.Min(this.networkSmoothing * Time.deltaTime, 1.0f);
            this.transform.position += toTarget * smoothing;
        }
    }

    private void OnDrawGizmos()
    {
        if(this.isLocalPlayer == false)
        {
            Gizmos.DrawSphere(this.synchedPosition, 0.2f);
        }
    }

    [Command]
    private void CmdSetPlayerName(String name)
    {
        this.synchedName = name;
        this.RpcSetPlayerName(name);
    }

    [Command]
    private void CmdUpdatePosition(Vector3 position)
    {
        this.synchedPosition = position;
    }

    [ClientRpc]
    private void RpcSetPlayerName(String name)
    {
        this.synchedName = name;
        this.nameTextMesh.text = name;
    }
}
