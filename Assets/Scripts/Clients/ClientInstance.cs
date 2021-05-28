/***If you want to know a connectionToClient you need to use base.connectionToClient FROM THE SERVER (e.g. like how we spawned the player object OnStartServer)
    If you want to know a connectionToServer you need to use base.connectionToServer FROM THE CLIENT
    Engine Code works on BOTH CLIENT AND SERVER So it executes TWICE*/

/* Attached to the ClientInstance object that is spawned on connecting to server
 * Contains a Singleton that comes in handy for returning a representative of a client, be it local or remote.
 * **/


using Mirror;

namespace GettingStartedWithMirror.Clients
{
    
    public class ClientInstance : NetworkBehaviour
    {
        #region VAR
        /// <summary>
        /// Singleton reference to the clientInstance. Referenced value will be for LocalPlayer. 
        /// </summary>
        public static ClientInstance Instance;
        public Respawner Respawner { get; private set; }
        #region ENGINE
        private void Awake()
        {
            Respawner = GetComponent<Respawner>();
        }
        #endregion
        #endregion
        #region NETWORK BEHAVIOUR
        /// <summary>
        /// Runs on the local player, setup singleton and THEN tell server to spawn character
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Instance = this;
        }
        #endregion

        #region STATIC METHODS
        /// <summary>
        /// Returns either the ClientInstance component of a remote client using its connection 
        /// or the local ClientInstance in case of a null connection arguement or an inactive network server.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static ClientInstance ReturnClientInstance(NetworkConnection conn = null) 
        {
            if (NetworkServer.active&&conn!=null)
            {
                NetworkIdentity localPlayer;
                if (GettingStartedNetworkManager.LocalPlayers.TryGetValue(conn,out localPlayer))
                {
                    return localPlayer.GetComponent<ClientInstance>();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return Instance;
            }
        }
        #endregion
    }
}
