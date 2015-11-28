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

    private void OnGUI()
    {
        if (this.state != null)
        {
            if (this.state.NextState != null)
            {
                this.state = this.state.NextState;
            }

            this.state.OnGUI();
        }
    }

    private class State
    {
        protected CustomNetworkManager networkManager = null;
        protected State nextState = null;

        public State NextState { get { return this.nextState; } }

        public State(CustomNetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        public virtual void OnGUI() { }
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
    }

    private class InGameState : State
    {
        public InGameState(CustomNetworkManager networkManager)
            : base(networkManager)
        {}

        public override void OnGUI()
        {
            if (this.networkManager.isNetworkActive)
            {
                if (this.networkManager.numPlayers < 3)
                {
                    GUILayout.Label(String.Format("Waiting for players - {0}/3", this.networkManager.numPlayers));
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
            base.OnGUI();

            if (this.networkManager.isNetworkActive == false)
            {
                GUILayout.Label("Host has disconnected");
                if (GUILayout.Button("Back", GUILayout.Width(100)))
                {
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }
            else
            {
                GUILayout.Label("Connected :)");
                if (GUILayout.Button("Disconnect", GUILayout.Width(100)))
                {
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }
        }
    }

    private class HostInGameState : State
    {
        public HostInGameState(CustomNetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            base.OnGUI();

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    GUILayout.Label("Your IP is: " + ip);
                }
            }
        }
    }
}
