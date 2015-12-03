using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;
using System.Collections.Generic;

public class MatchmakerGUI : MonoBehaviour 
{
    private NetworkManager networkManager;
    private NetworkMatch networkMatch;
    private State state;

    private void Start()
    {
        this.networkManager = this.GetComponent<NetworkManager>();
        this.networkMatch = this.gameObject.AddComponent<NetworkMatch>();
        this.state = new CreateOrJoinGameState(this.networkManager, this.networkMatch);
    }

    private void OnGUI()
    {
        if (this.state.nextState != null)
        {
            this.state = this.state.nextState;
        }
        this.state.OnGUI();
    }

    private class State
    {
        protected NetworkManager networkManager;
        protected NetworkMatch networkMatch;
        public State nextState = null;

        public State(NetworkManager networkManager, NetworkMatch networkMatch)
        {
            this.networkManager = networkManager;
            this.networkMatch = networkMatch;
        }

        public virtual void OnGUI() { }
    }

    private class CreateOrJoinGameState : State
    {
        private String gameName = "game_name";
        private List<MatchDesc> matches = null;
        private String errorMessage = "";
        private bool listingMatches = false;
        private float lastMatchListingTime = 0;

        public CreateOrJoinGameState(NetworkManager networkManager, NetworkMatch networkMatch) : base(networkManager, networkMatch)
        {
            this.UpdateMatchList();
        }

        private void UpdateMatchList()
        {
            if (this.listingMatches)
            {
                Debug.LogWarning("Already listing matches!");
                return;
            }

            this.networkMatch.ListMatches(0, 20, "", this.OnMatchList);
            this.listingMatches = true;
        }

        private void OnMatchList(ListMatchResponse response)
        {
            if (response.success)
            {
                this.matches = response.matches;
                this.errorMessage = "";
            }
            else
            {
                this.matches = null;
                this.errorMessage = "Error getting match list";
            }

            this.listingMatches = false;
            this.lastMatchListingTime = Time.time;
        }

        public override void OnGUI()
        {
            if (this.listingMatches == false && (Time.time - this.lastMatchListingTime) > 5)
            {
                this.UpdateMatchList();
            }

            if (this.matches != null)
            {
                if (this.matches.Count == 0)
                {
                    GUILayout.Label("No games currently hosted, you should create one!");
                }
                else
                {
                    foreach (MatchDesc match in this.matches)
                    {
                        GUILayout.Label(match.name);
                        if (GUILayout.Button("Join"))
                        {
                            WaitForJoinGameState waitForJoinGameState = new WaitForJoinGameState(this.networkManager, this.networkMatch);
                            this.networkMatch.JoinMatch(match.networkId, "", waitForJoinGameState.OnJoinMatch);
                            this.nextState = waitForJoinGameState;
                        }
                    }
                }
            }
            GUILayout.Label(this.errorMessage);

            int maxLength = 32;
            this.gameName = GUILayout.TextField(this.gameName, maxLength);
            if(GUILayout.Button("Create Game"))
            {
                CreateMatchRequest createMatchRequest = new CreateMatchRequest();
                createMatchRequest.name = this.gameName;
                createMatchRequest.size = 3;
                createMatchRequest.advertise = true;
                createMatchRequest.password = "";

                WaitForCreateGameState waitForCreateGameState = new WaitForCreateGameState(this.networkManager, this.networkMatch);
                this.networkMatch.CreateMatch(createMatchRequest, waitForCreateGameState.OnCreateMatch);
                this.nextState = waitForCreateGameState;
            }
        }
    }

    private class WaitForCreateGameState : State
    {
        private CreateMatchResponse response = null;

        public WaitForCreateGameState(NetworkManager networkManager, NetworkMatch networkMatch)
            : base(networkManager, networkMatch)
        {

        }

        public void OnCreateMatch(CreateMatchResponse response)
        {
            this.response = response;

            if (this.response.success)
            {
                this.networkManager.OnMatchCreate(response);
                this.nextState = new InGameHostState(this.networkManager, this.networkMatch);
            }
        }

        public override void OnGUI()
        {
            if (this.response != null )
            {
                if (this.response.success == false)
                {
                    GUILayout.Label("Failed to create game :(");
                    if (GUILayout.Button("Back"))
                    {
                        this.nextState = new CreateOrJoinGameState(this.networkManager, this.networkMatch);
                    }
                }
            }
            else
            {
                GUILayout.Label("Computing interweb coefficient");
            }
        }
    }

    private class WaitForJoinGameState : State
    {
        private JoinMatchResponse response = null;

        public WaitForJoinGameState(NetworkManager networkManager, NetworkMatch networkMatch)
            : base(networkManager, networkMatch)
        {

        }

        public void OnJoinMatch(JoinMatchResponse response)
        {
            this.response = response;

            if (this.response.success)
            {
                this.networkManager.OnMatchJoined(response);
                this.nextState = new InGameClientState(this.networkManager, this.networkMatch);
            }
        }

        public override void OnGUI()
        {
            if(this.response != null)
            {
                if (this.response.success == false)
                {
                    GUILayout.Label("Failed to join game :(");
                    if (GUILayout.Button("Back"))
                    {
                        this.nextState = new CreateOrJoinGameState(this.networkManager, this.networkMatch);
                    }
                }
            }
            else
            {
                GUILayout.Label("Shifting parity bits interfrastically through pericombobulator");
            }
        }
    }

    private class InGameHostState : State
    {
        public InGameHostState(NetworkManager networkManager, NetworkMatch networkMatch)
            : base(networkManager, networkMatch)
        {

        }

        // Todo gui to shut down game
    }

    private class InGameClientState : State
    {
        public InGameClientState(NetworkManager networkManager, NetworkMatch networkMatch)
            : base(networkManager, networkMatch)
        {

        }

        // Todo gui to shut down game
    }
}
