/* Attached to the player object owned by Client Instance
 * Manages Health points and Dealing damage to self
 * Also Manages post-death behaviour
 * **/
using UnityEngine;
using Mirror;
using GettingStartedWithMirror.Clients;

namespace GettingStartedWithMirror.Units
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Health : NetworkBehaviour
    {
        #region VAR
        [SyncVar]
        float health = 100f;
        /// <summary>
        /// Current Health of the unit
        /// </summary>
        public float CurrentHealth { get { return health; } private set { health = value; } }
        
        
        /// <summary>
        /// Maximum possible health
        /// </summary>
        public const float MAX_HEALTH= 100f;
        #endregion
        #region MEMBER METHODS
        /// <summary>
        /// Deals damage to the current health
        /// </summary>
        /// <param name="damage"></param>
        [Server]
        public void DealDamage(float damage,NetworkConnection attackerConn=null) 
        {
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0f, CurrentHealth);
            //Player needs to be respawned
            if (CurrentHealth==0f)
            {
                //Give score points to the instigator
                LeaderboardCanvas.Instance.KillSecured(attackerConn);
                //Destroy this player object
                NetworkConnection ownerConn = base.connectionToClient;
                NetworkServer.Destroy(gameObject);
                //Spawn another player object with full health (respawn) for this connection 
                ClientInstance clientInstance = ClientInstance.ReturnClientInstance(ownerConn);//(we're on server so we need to know the local client's network connection)
                if (clientInstance)
                {
                    Respawner respawner = clientInstance.GetComponent<Respawner>();
                    respawner.NetworkSpawnPlayer();
                }
            }
        }
        #endregion
    }
}