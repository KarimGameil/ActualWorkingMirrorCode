/*Attached to the ClientInstance object that is spawned as a player prefab in the GettingStartedWithMirrorNetworkManager
 *Responsible for Naming the player on local client and on server for all clients to see
 *requested by LeaderboardCanvas and NameCanvas**/
using UnityEngine;
using Mirror;
using System;
using GettingStartedWithMirror.Units;

namespace GettingStartedWithMirror.Clients
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Namer : NetworkBehaviour
    {
        #region VAR
        /// <summary>
        /// Dispatched when the player name is updated
        /// </summary>
        public event Action<string> Relay_OnNameUpdated;
        /// <summary>
        /// Current name for the player on server
        /// </summary>
        [SyncVar(hook = nameof(OnNameUpdated))]
        string serverCurrentName = String.Empty;
        public string ServerCurrentName { get { return serverCurrentName; } } //Getter for the one above it
        Respawner respawner = null;
        #endregion
        #region MEMBER METHODS
        /// <summary>
        /// Set playerName for the local client
        /// </summary>
        /// <param name="name"></param>
        [Client]
        public void SetPlayerName(string name)
        {
            CmdSetName(name);
        }
        #endregion
        #region ENGINE
        private void Awake()
        {
            Respawner.OnOwnerCharacterSpawned += ClientInstance_Relay_OnOwnerCharacterSpawned;
        }
        private void OnDestroy()
        {
            Respawner.OnOwnerCharacterSpawned -= ClientInstance_Relay_OnOwnerCharacterSpawned;
        }
        #endregion
        #region HANDLERS
        /// <summary>
        /// This can be used as an initializer across the network instead of Start/Awake
        /// </summary>
        /// <param name="gameObject"></param>
        void ClientInstance_Relay_OnOwnerCharacterSpawned(GameObject gameObject)
        {
            if (gameObject)
            {
                UpdateCharacterPlayerName(serverCurrentName);
            }
        }
        /// <summary>
        /// Hook for the SyncVar serverCurrentName
        /// </summary>
        /// <param name="oldPlayerName"></param>
        /// <param name="newPlayerName"></param>
        void OnNameUpdated(string oldPlayerName, string newPlayerName)
        {
            UpdateCharacterPlayerName(newPlayerName);
            Relay_OnNameUpdated?.Invoke(newPlayerName);
        }
        #endregion
        #region LOCAL METHODS
        /// <summary>
        /// Sets the name for this character on server
        /// </summary>
        /// <param name="name"></param>
        [Command]
        void CmdSetName(string name)
        {
            serverCurrentName = name;
            UpdateCharacterPlayerName(name);
        }
        /// <summary>
        /// Updates the name tag above the player object of this ClientInstance
        /// </summary>
        /// <param name="name"></param>
        void UpdateCharacterPlayerName(string name)
        {
            if (!respawner)
            {
                respawner = GetComponent<Respawner>();
            }
            if (respawner.CurrentlySpawnedCharacter)
            {
                PlayerName playerName = respawner.CurrentlySpawnedCharacter.GetComponent<PlayerName>();
                playerName.SetName(name);
            }
        }
        #endregion
    }
}
