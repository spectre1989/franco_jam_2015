using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Sockets;

public class MenuGUI : MonoBehaviour 
{
    private CustomNetworkManager networkManager;
    private State state = null;

    private void Start()
    {
        this.networkManager = this.GetComponent<CustomNetworkManager>();
        this.state = new JoinOrHostState(this.networkManager);
    }

    private void Update()
    {
        if (this.state.NextState != null)
        {
            this.state = this.state.NextState;
        }

        this.state.Update();
    }

    private void OnGUI()
    {
        if (this.state != null)
        {
            this.state.OnGUI();
        }
    }

    private abstract class State
    {
        protected CustomNetworkManager networkManager = null;
        protected State nextState = null;

        public State NextState { get { return this.nextState; } }

        public State(CustomNetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        public abstract void Update();
        public abstract void OnGUI();
    }

    private class JoinOrHostState : State
    {
        public JoinOrHostState(CustomNetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name:");
            this.networkManager.playerName = GUILayout.TextField(this.networkManager.playerName, 20, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (GUILayout.Button("Host Game", GUILayout.Width(100)))
            {
                GameInfo.Instance.Init();
                this.networkManager.StartHost();
                this.nextState = new HostInGameState(this.networkManager);
            }

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            this.networkManager.networkAddress = GUILayout.TextField(this.networkManager.networkAddress, GUILayout.Width(100));
            if (GUILayout.Button("Join Game", GUILayout.Width(100)))
            {
                this.networkManager.StartClient();
                this.nextState = new WaitForJoinGameState(this.networkManager);
            }
            GUILayout.EndHorizontal();
        }

        public override void Update()
        {
        }
    }

    private class WaitForJoinGameState : State
    {
        public WaitForJoinGameState(CustomNetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            if (this.networkManager.isNetworkActive)
            {
                if (this.networkManager.IsClientConnected())
                {
                    //ClientScene.Ready(this.networkManager.client.connection);
                    ClientScene.AddPlayer(0);
                    this.nextState = new ClientInGameState(this.networkManager);
                }
            }
            else
            {
                GUILayout.Label("Failed to connect :(");
                if (GUILayout.Button("Back", GUILayout.Width(100)))
                {
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }
        }

        public override void Update()
        {
            
        }
    }

    private class InGameState : State
    {
        protected bool isClientConnected = false;

        public InGameState(CustomNetworkManager networkManager)
            : base(networkManager)
        {}

        public override void Update()
        {
            this.isClientConnected = this.networkManager.client != null && this.networkManager.client.isConnected;
        }

        public override void OnGUI()
        {
            if (this.isClientConnected)
            {
                switch (GameInfo.Instance.CurrentState)
                {
                    case GameInfo.State.WaitingForPlayers:
                        GUILayout.Label(String.Format("Waiting for players - {0}/3", GameInfo.Instance.NumPlayers));
                        break;

                    case GameInfo.State.Countdown:
                        GUILayout.Label(String.Format("STARTING IN {0}", Mathf.CeilToInt(GameInfo.Instance.Countdown)));
                        break;

                    case GameInfo.State.InGame:
                        int seconds = Mathf.CeilToInt(GameInfo.Instance.GameTimer);
                        int minutes = seconds / 60;
                        seconds = seconds % 60;
                        GUILayout.Label(String.Format("{0}:{1:00}", minutes, seconds));
                        break;

                    case GameInfo.State.EndOfGame:
                        
                        break;
                }
            }
        }
    }

    private class ClientInGameState : InGameState
    {
        public ClientInGameState(CustomNetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            if (this.isClientConnected == false)
            {
                GUILayout.Label("Host has disconnected");
                if (GUILayout.Button("Back", GUILayout.Width(100)))
                {
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }
            else
            {
                if (GUILayout.Button("Disconnect", GUILayout.Width(100)))
                {
                    this.networkManager.StopClient();
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }

            GUILayout.Space(20);

            base.OnGUI();
        }
    }

    private class HostInGameState : InGameState
    {
        public HostInGameState(CustomNetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            if (NetworkServer.active)
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        GUILayout.Label("Your IP is: " + ip);
                    }
                }

                if (GUILayout.Button("Stop Hosting", GUILayout.Width(100)))
                {
                    this.networkManager.StopHost();
                    this.nextState = new JoinOrHostState(this.networkManager);
                }

                GUILayout.Space(20);

                base.OnGUI();
            }
            else
            {
                GUILayout.Label("Hosting failed, are you already hosting in another instance?");
                if (GUILayout.Button("Back", GUILayout.Width(100)))
                {
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }
        }
    }
}
