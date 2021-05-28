/* Attached to the ClientInstance object
 * Handles the Spawning and Respawning of the player object owned by this Client Instance
 * **/
using UnityEngine;
using Mirror;
using System;

namespace GettingStartedWithMirror.Clients
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Respawner : NetworkBehaviour
    {
        #region VAR
        /// <summary>
        /// Dispatched on the owning player when a character is spawned for them.
        /// </summary>
        public static Action<GameObject> OnOwnerCharacterSpawned;
        /// <summary>
        /// Currently Spawned character for the local player.Exist on server and client
        /// </summary>
        public GameObject CurrentlySpawnedCharacter { get; private set; } = null;

        [Tooltip("Prefab for Player")]
        [SerializeField] NetworkIdentity playerPrefab = null; //declaring it as a NetworkIdentity ensures we have a network object to spawn
        #endregion
        #region NETWORK BEHAVIOUR
        /// <summary>
        /// Runs on the local player, setup singleton and THEN tell server to spawn character
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            CmdRequestSpawn();
        }
        #endregion
        #region LOCAL METHODS
        /// <summary>
        /// Request a Spawn for character.
        /// </summary>
        [Command]
        void CmdRequestSpawn() { NetworkSpawnPlayer(); } //Called by client, works on Server
         /// <summary>
         /// Spawns a character for the player
         /// </summary>
        [Server]
        public void NetworkSpawnPlayer() 
        {
            GameObject go = Instantiate(playerPrefab.gameObject, transform.position, Quaternion.identity);
            CurrentlySpawnedCharacter = go;
            NetworkServer.Spawn(go, base.connectionToClient);
        }
        #endregion
        #region HANDLERS
        public void InvokeOwnerCharacterSpawned(GameObject gameObject) 
        {
            CurrentlySpawnedCharacter = gameObject; 
            OnOwnerCharacterSpawned?.Invoke(gameObject);
        }
        #endregion
    }
}
