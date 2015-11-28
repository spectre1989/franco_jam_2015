using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class GameInfo : NetworkBehaviour
{
    private static GameInfo intance;
    public static GameInfo Instance { get { return GameInfo.intance; } }

    public enum State
    {
        WaitingForPlayers,
        Countdown,
        InGame,
        EndOfGame
    }

    public struct PlayerInfo
    {
        public String name;
        public int score;
    }
    public class SyncListPlayerInfo : SyncListStruct<PlayerInfo>
    {}

    private struct RespawnRequest
    {
        public NetworkConnection networkConnection;
        public float time;
    }

    [SerializeField]
    private float countdownLength;
    [SerializeField]
    private float gameLength;
    [SerializeField]
    private float endOfGameLength;
    [SerializeField]
    private float respawnTime;

    [SyncVar]
    private State currentState;
    [SyncVar]
    private float countdownTimer;
    [SyncVar]
    private float gameTimer;
    [SyncVar]
    private float endOfGameTimer;
    [SyncVar]
    private int numPlayers;

    private Queue<RespawnRequest> respawnQueue;

    public State CurrentState { get { return this.currentState; } }
    public float Countdown { get { return this.countdownTimer; } }
    public float GameTimer { get { return this.gameTimer; } }
    public int NumPlayers { get { return this.numPlayers; } }

    private void Awake()
    {
        GameInfo.intance = this;
        this.currentState = State.WaitingForPlayers;
    }
    
    private void Update()
    {
        if (this.isServer)
        {
            switch (this.currentState)
            {
                case State.WaitingForPlayers:
                    this.numPlayers = this.NumConnectedPlayers;
                    if (this.numPlayers == 3)
                    {
                        this.EnterState(State.Countdown);
                        return;
                    }
                    break;

                case State.Countdown:
                    {
                        if (this.NumConnectedPlayers < 3)
                        {
                            this.EnterState(State.WaitingForPlayers);
                            return;
                        }

                        this.countdownTimer -= Time.deltaTime;
                        if (this.countdownTimer <= 0.0f)
                        {
                            this.EnterState(State.InGame);
                        }
                    }
                    break;

                case State.InGame:
                    {
                        this.gameTimer -= Time.deltaTime;
                        if (this.gameTimer <= 0.0f)
                        {
                            this.EnterState(State.EndOfGame);
                        }

                        while (this.respawnQueue.Count > 0 && this.respawnQueue.Peek().time <= Time.time)
                        {
                            GameObject player = Instantiate(NetworkManager.singleton.playerPrefab) as GameObject;
                            player.transform.position = NetworkManager.singleton.startPositions[UnityEngine.Random.Range(0, NetworkManager.singleton.startPositions.Count)].position;
                            NetworkServer.AddPlayerForConnection(this.respawnQueue.Peek().networkConnection, player, 0);
                            this.respawnQueue.Dequeue();
                        }
                    }
                    break;

                case State.EndOfGame:
                    {
                        this.endOfGameTimer -= Time.deltaTime;
                        if (this.endOfGameTimer <= 0.0f)
                        {
                            this.EnterState(State.WaitingForPlayers);
                        }
                    }
                    break;
            }
        }
    }

    public void Init()
    {
        this.EnterState(State.WaitingForPlayers);
    }

    private void EnterState(State state)
    {
        this.currentState = state;

        switch (state)
        {
            case State.Countdown:
                this.countdownTimer = this.countdownLength;
                break;

            case State.InGame:
                this.respawnQueue = new Queue<RespawnRequest>();
                this.gameTimer = this.gameLength;

                List<NetworkConnection> connections = this.AllConnections;
                for(int i = 0; i < connections.Count; ++i)
                {
                    GameObject player = Instantiate(NetworkManager.singleton.playerPrefab) as GameObject;
                    player.transform.position = NetworkManager.singleton.startPositions[i % NetworkManager.singleton.startPositions.Count].position;
                    NetworkServer.AddPlayerForConnection(connections[i], player, 0);
                }

                break;

            case State.EndOfGame:
                this.endOfGameTimer = this.endOfGameLength;
                break;
        }
    }

    private List<NetworkConnection> AllConnections
    {
        get
        {
            List<NetworkConnection> connections = new List<NetworkConnection>(NetworkServer.connections);
            connections.AddRange(NetworkServer.localConnections);
            while(connections.Remove(null));
            return connections;
        }
    }

    private int NumConnectedPlayers
    {
        get
        {
            int n = 0;

            foreach(NetworkConnection networkConnection in this.AllConnections)
            {
                if (networkConnection != null && networkConnection.isReady)
                {
                    ++n;
                }
            }

            return n;
        }
    }

    public void AddToRespawnQueue(NetworkConnection connection)
    {
        this.respawnQueue.Enqueue(new RespawnRequest { networkConnection = connection, time = Time.time + this.respawnTime });
    }
}
