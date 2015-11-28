using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System;
using System.Collections.Generic;

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
    private float lerpDelay;
    [SerializeField]
    private TextMesh nameTextMesh;

    // Game stuff
    private enum MagicType
    {
        None = 0,
        Rock = 1,
        Paper = 2,
        Scissors = 3
    }
    private static MagicType[] CreateKillTable()
    {
        MagicType[] killTable = new MagicType[4];
        killTable[(int)MagicType.None] = (MagicType)(-1);
        killTable[(int)MagicType.Rock] = MagicType.Scissors;
        killTable[(int)MagicType.Paper] = MagicType.Rock;
        killTable[(int)MagicType.Scissors] = MagicType.Paper;

        return killTable;
    }
    private static MagicType[] killTable = CreateKillTable();

    private Rigidbody rigidbody;
    private Material material;
    private MagicType magic;

    // Network stuff
    struct SynchedPosition
    {
        public float t;
        public Vector3 position;

        public SynchedPosition(float t, Vector3 position)
        {
            this.t = t;
            this.position = position;
        }
    }

    [SyncVar(hook="OnSynchName")]
    private String synchedName;
    [SyncVar(hook = "OnSynchPosition")]
    private SynchedPosition synchedPosition;
    private Queue<SynchedPosition> synchedPositions = new Queue<SynchedPosition>();
    private SynchedPosition interpolateFromSynchedPosition;
    private float targetLerpTime;

    [SyncVar]
    private MagicType synchedMagic;

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
            this.interpolateFromSynchedPosition.position = this.transform.position;
            this.nameTextMesh.text = this.synchedName;
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

            this.CmdUpdatePosition(new SynchedPosition(Time.time, this.transform.position));
        }
    }

    private void Update()
    {
        if (this.isLocalPlayer)
        {
            this.magic = TestPlayer.MagicType.None;

            if (Input.GetKey(this.rockKey))
            {
                this.magic = MagicType.Rock;
            }
            else if (Input.GetKey(this.paperKey))
            {
                this.magic = MagicType.Paper;
            }
            else if (Input.GetKey(this.scissorsKey))
            {
                this.magic = MagicType.Scissors;
            }

            CmdUpdateMagic(this.magic);
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
            }
            else
            {
                this.transform.position = this.interpolateFromSynchedPosition.position;
            }

            this.targetLerpTime += Time.deltaTime;

            // Magic
            this.magic = this.synchedMagic;
        }

        switch (this.magic)
        {
            case MagicType.None:
                this.material.color = Color.white;
                break;

            case MagicType.Paper:
                this.material.color = this.paperColour;
                break;

            case MagicType.Rock:
                this.material.color = this.rockColour;
                break;

            case MagicType.Scissors:
                this.material.color = this.scissorsColour;
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if(this.isLocalPlayer == false)
        {
            foreach (SynchedPosition synchedPosition in this.synchedPositions)
            {
                Gizmos.DrawSphere(synchedPosition.position, 0.2f);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TestPlayer otherPlayer = collision.gameObject.GetComponent<TestPlayer>();

        if (otherPlayer == null)
        {
            return;
        }

        if (this.magic != MagicType.None)
        {
            bool kill = otherPlayer.magic == MagicType.None;
            if (kill == false)
            {
                // See if we have the corresponding magic type to kill the opponent
                kill = otherPlayer.magic == TestPlayer.killTable[(int)this.magic];
            }

            if (kill)
            {
                this.CmdKill(otherPlayer.netId);
            }
        }
    }

    private void OnSynchName(String name)
    {
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

    [Command]
    private void CmdSetPlayerName(String name)
    {
        this.synchedName = name;
    }

    [Command]
    private void CmdUpdatePosition(SynchedPosition position)
    {
        this.synchedPosition = position;
    }

    [Command]
    private void CmdUpdateMagic(MagicType magic)
    {
        this.synchedMagic = magic;
    }

    [Command]
    private void CmdKill(NetworkInstanceId netId)
    {
        GameObject playerGameObject = NetworkServer.FindLocalObject(netId);
        if (playerGameObject != null)
        {
            TestPlayer player = playerGameObject.GetComponent<TestPlayer>();
            player.RpcKill();
        }
    }

    [ClientRpc]
    private void RpcKill()
    {
        if (this.isLocalPlayer)
        {
            this.rigidbody.velocity = new Vector3();
            this.transform.position = NetworkManager.singleton.startPositions[UnityEngine.Random.Range(0, NetworkManager.singleton.startPositions.Count)].position;
        }
    }
}
