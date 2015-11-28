using UnityEngine;
using UnityEngine.Networking;

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

    [SerializeField]
    private float countdownLength;
    [SerializeField]
    private float gameLength;
    [SerializeField]
    private float endOfGameLength;

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
                    this.numPlayers = NetworkManager.singleton.numPlayers;
                    if (this.numPlayers == 3)
                    {
                        this.EnterState(State.Countdown);
                        return;
                    }
                    break;

                case State.Countdown:
                    {
                        if (NetworkManager.singleton.numPlayers < 3)
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

    private void EnterState(State state)
    {
        this.currentState = state;

        switch (state)
        {
            case State.Countdown:
                this.countdownTimer = this.countdownLength;
                break;

            case State.InGame:
                this.gameTimer = this.gameLength;
                break;

            case State.EndOfGame:
                this.endOfGameTimer = this.endOfGameLength;
                break;
        }
    }
}
