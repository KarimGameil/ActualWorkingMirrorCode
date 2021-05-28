/* Attached to the player object owned by Client Instance
 * Activates camera only to the owner and no other spectator
 * **/
using UnityEngine;
using Mirror;

namespace GettingStartedWithMirror.Clients
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class CameraSetup : NetworkBehaviour
    {
        #region VAR
        [Tooltip("Object for the camera within the child of this transform")]
        [SerializeField] Transform cameraObject;
        #endregion
        #region NETWORK BEHAVIOUR
        //Is executed only if you're the owner, unlike OnStartClient which executes for all clients
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            cameraObject.gameObject.SetActive(true);
        }
        #endregion
    }
}