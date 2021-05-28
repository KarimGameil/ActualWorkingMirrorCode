/* Attached to the LeaderboardCanvas UI object in the Gameplay Scene.
 * Manages a local list of PlayerScore components belonging to all clients joined to the server (Addition, Removal, Clearance)
 * Manages the Leaderboard UI by Creating/Destroying PlayerScore UI Entries depending on the current state of the local list.
 * ALso updates the entries in case of a change in either name or score for both the local client and the spectators(remote clients)
 * **/
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace GettingStartedWithMirror.Clients
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class LeaderboardCanvas : NetworkBehaviour
    {
        #region VAR
        [SerializeField] Transform content;
        [SerializeField] PlayerScore playerScorePrefab;

        List<PlayerScore> addedPlayerScores = new List<PlayerScore>();

        /// <summary>
        /// Lazy unnecessary Singleton
        /// </summary>
        public static LeaderboardCanvas Instance { get; private set; }
        #endregion
        #region MEMBER METHODS
        /// <summary>
        /// Called when an attacker makes a kill
        /// </summary>
        /// <param name="attackerConn"></param>
        [Server]
        public void KillSecured(NetworkConnection attackerConn)
        {
            ClientInstance attackerClientInstance = ClientInstance.ReturnClientInstance(attackerConn);
            if (attackerClientInstance)
            {
                AddScore(attackerClientInstance.netId, 1);
            }
        }
        #endregion
        #region ENGINE
        private void Awake()
        {
            Instance = this;
            GettingStartedNetworkManager.Relay_OnStopClient += GettingStartedNetworkManager_Relay_OnStopClient;
            GettingStartedNetworkManager.Relay_OnStopServer += GettingStartedNetworkManager_Relay_OnStopServer;
        }
        private void OnDestroy()
        {
            GettingStartedNetworkManager.Relay_OnStopClient -= GettingStartedNetworkManager_Relay_OnStopClient;
            GettingStartedNetworkManager.Relay_OnStopServer -= GettingStartedNetworkManager_Relay_OnStopServer;
        }
        #endregion
        #region NETWORK BEHAVIOUR
        public override void OnStartServer()
        {
            base.OnStartServer();
            GettingStartedNetworkManager.Relay_OnServerAddPlayer += GettingStartedNetworkManager_Relay_OnServerAddPlayer;
            GettingStartedNetworkManager.Relay_OnServerDisconnect += GettingStartedNetworkManager_Relay_OnServerDisconnect;
        }
        public override void OnStopServer()
        {
            base.OnStopServer();
            GettingStartedNetworkManager.Relay_OnServerAddPlayer -= GettingStartedNetworkManager_Relay_OnServerAddPlayer;
            GettingStartedNetworkManager.Relay_OnServerDisconnect -= GettingStartedNetworkManager_Relay_OnServerDisconnect;
        }
        #endregion
        #region HANDLERS
        //LOCAL ENGINE
        private void GettingStartedNetworkManager_Relay_OnStopServer()
        {
            ClearPlayerScores();
        }

        private void GettingStartedNetworkManager_Relay_OnStopClient()
        {
            ClearPlayerScores();
        }

        //NETWORK BEHAVIOUR
        /// <summary>
        /// This is run on the server to handle the removal of leaderboard entries on server disconnection
        /// </summary>
        /// <param name="conn"></param>
        private void GettingStartedNetworkManager_Relay_OnServerDisconnect(NetworkConnection conn)
        {
            ///Tell all players this player left
            if (GettingStartedNetworkManager.LocalPlayers.TryGetValue(conn, out NetworkIdentity networkIdentity))
            {
                RpcPlayerDisconnected(networkIdentity.netId);
                RemovePlayer(netId);
            }
        }
        /// <summary>
        /// This is run on the server to handle the addition of newly spawned players entry in the leaderboard
        /// </summary>
        /// <param name="conn"></param>
        private void GettingStartedNetworkManager_Relay_OnServerAddPlayer(NetworkConnection conn)
        {
            //Tell all players this player joined
            if (GettingStartedNetworkManager.LocalPlayers.TryGetValue(conn, out NetworkIdentity newPlayerNetworkIdentity))
            {
                foreach (PlayerScore playerScore in addedPlayerScores)
                {
                    if (NetworkIdentity.spawned.TryGetValue(playerScore.NetId, out NetworkIdentity existingPlayerNetworkIdentity))
                    {
                        NetworkConnection existingPlayerConnection = existingPlayerNetworkIdentity.connectionToClient;

                        int score = playerScore.GetScore();
                        ClientInstance clientInstance = ClientInstance.ReturnClientInstance(existingPlayerConnection);
                        Namer namer = clientInstance.GetComponent<Namer>();
                        string name = namer.ServerCurrentName;
                        //Send to new joiner all current players
                        TargetPlayerConnected(conn, playerScore.NetId, name, score);
                    }
                }
                //send to all players that a player joined
                RpcPlayerConnected(newPlayerNetworkIdentity.netId);
                //add locally
                AddPlayer(newPlayerNetworkIdentity.netId, "New Player", 0);
            }
        }
        #endregion
        #region LOCAL METHODS
        /// <summary>
        /// Clears all playerScore entries in the list
        /// </summary>
        void ClearPlayerScores() 
        {
            for (int i = 0; i < addedPlayerScores.Count; i++)
            {
                Destroy(addedPlayerScores[i].gameObject);
            }
            addedPlayerScores.Clear();
        }
        /// <summary>
        /// Instantiates a Playerscore entry and initializes it using the passed values
        /// </summary>
        /// <param name="netId">the netId of the NetworkConnection of the added player</param>
        /// <param name="name"></param>
        /// <param name="score"></param>
        void AddPlayer(uint netId, string name, int score)
        {
            PlayerScore playerScore = Instantiate(playerScorePrefab, content);
            playerScore.SetNetId(netId); playerScore.SetPlayerName(name); playerScore.AddScore(score);
            addedPlayerScores.Add(playerScore);
        }
        /// <summary>
        /// Destroys the PlayerScore entry with a matching netId
        /// </summary>
        /// <param name="netId">the netId entered when this player was added in the first place</param>
        void RemovePlayer(uint netId)
        {
            int index = addedPlayerScores.FindIndex(x => x.NetId == netId);
            if (index != -1)
            {
                Destroy(addedPlayerScores[index].gameObject);
                addedPlayerScores.RemoveAt(index);
            }
        }
        /// <summary>
        /// updates a player with a matching netId in the list in terms of score
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="value"></param>
        void AddScore(uint netId, int value)
        {
            int index = addedPlayerScores.FindIndex(x => x.NetId == netId);
            if (index != -1)
            {
                addedPlayerScores[index].AddScore(value);
                if (base.isServer)
                {
                    RpcAddScore(netId, value);
                }
            }
        }
        /// <summary>
        /// Called on a specific client when it connects to the server
        /// </summary>
        /// <param name="conn">the NetworkConnection of the newly joined client</param>
        [TargetRpc]
        void TargetPlayerConnected(NetworkConnection conn,uint netId,string name, int score) 
        {
            if (base.isServer)
            {
                return;
            }
            AddPlayer(netId, name, score);
        }
        /// <summary>
        /// Called on all clients to have them add the newly joined client in their lists and manage UI based on said lists
        /// </summary>
        /// <param name="netId"></param>
        [ClientRpc]
        void RpcPlayerConnected(uint netId) 
        {
            if (base.isServer)
            {
                return;
            }
            AddPlayer(netId, "New Player", 0);
        }
        /// <summary>
        /// Called on add clients to have them remove the recently disconnected client from their lists and Manage UI based on said lists
        /// </summary>
        [ClientRpc]
        void RpcPlayerDisconnected(uint netId) 
        {
            if (base.isServer)
            {
                return;
            }
            RemovePlayer(netId);
        }
        /// <summary>
        /// Called on all clients to have them update the score of a player with a matching netId in their lists and then their UI
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="value"></param>
        [ClientRpc]
        void RpcAddScore(uint netId,int value) 
        {
            //To prevent an endless callback loop between Addscore and RpcAddScore
            if (base.isServer)
            {
                return;
            }
            AddScore(netId, value);
        }

        #endregion
    }
}