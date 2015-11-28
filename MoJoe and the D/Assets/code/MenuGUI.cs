using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.Sockets;

public class MenuGUI : MonoBehaviour 
{
    private NetworkManager networkManager;
    private State state = null;

    private void Start()
    {
        this.networkManager = this.GetComponent<NetworkManager>();
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
        protected NetworkManager networkManager = null;
        protected State nextState = null;

        public State NextState { get { return this.nextState; } }

        public State(NetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        public virtual void OnGUI() { }
    }

    private class JoinOrHostState : State
    {
        public JoinOrHostState(NetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            if (GUILayout.Button("Host Game"))
            {
                this.networkManager.StartHost();
                this.nextState = new HostInGameState(this.networkManager);
            }

            GUILayout.BeginHorizontal();
            this.networkManager.networkAddress = GUILayout.TextField(this.networkManager.networkAddress);
            if (GUILayout.Button("Join Game"))
            {
                this.networkManager.StartClient();
                this.nextState = new WaitForJoinGameState(this.networkManager);
            }
            GUILayout.EndHorizontal();
        }
    }

    private class WaitForJoinGameState : State
    {
        public WaitForJoinGameState(NetworkManager networkManager)
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
                if (GUILayout.Button("Back"))
                {
                    this.nextState = new JoinOrHostState(this.networkManager);
                }
            }
        }
    }

    private class ClientInGameState : State
    {
        public ClientInGameState(NetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            GUILayout.Label("connected");
        }
    }

    private class HostInGameState : State
    {
        public HostInGameState(NetworkManager networkManager)
            : base(networkManager)
        {
        }

        public override void OnGUI()
        {
            GUILayout.Label("hosting");

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
