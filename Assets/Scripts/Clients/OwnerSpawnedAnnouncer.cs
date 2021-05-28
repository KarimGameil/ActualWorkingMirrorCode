/* Attached to the player object owned by the Client Instance
 * feeds the spawned player object to the Respawner attached to the client Instance
 * **/
using UnityEngine;
using Mirror;

namespace GettingStartedWithMirror.Clients
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class OwnerSpawnedAnnouncer : NetworkBehaviour
    {
        #region NETWORK BEHAVIOUR
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            AnnounceSpawned();
        }
        #endregion
        #region LOCAL METHODS
        void AnnounceSpawned() 
        {
            ClientInstance clientInstance = ClientInstance.ReturnClientInstance();//no connectionToClient passed because this is run on client
            Respawner respawner = clientInstance.GetComponent<Respawner>();
            if (respawner)
            {
                respawner.InvokeOwnerCharacterSpawned(gameObject);
            }
        }
        #endregion
    }
}