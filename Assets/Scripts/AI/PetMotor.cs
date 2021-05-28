/* Attached to the player object owned by ClientInstance
 * Handles player object movement and rotation while also signalling the blendtree float to the animator attached to it.
 * **/
using UnityEngine;
using Mirror;
using GettingStartedWithMirror.Units;
using GettingStartedWithMirror.Clients;
namespace GettingStartedWithMirror.AI
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PetMotor : NetworkBehaviour
    {
        #region VAR
        [SerializeField] CharacterController characterController = null;
        [Tooltip("How Quick Can Object Move")]
        [SerializeField] float moveRate = 5f;
        

        PetCombat petCombat;
        ClientInstance clientInstance=null;
        GameObject owner=null;
        /// <summary>
        /// How close pet can be to Target
        /// </summary>
        const float TARGET_PROXIMITY = 1f;
        /// <summary>
        /// How close pet can be to Owner
        /// </summary>
        const float OWNER_PROXIMITY = 3f;
        #endregion
        #region ENGINE
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }
        void Update()
        {
            //This works better for player-owned objects and spawnables like pets or rts units. DO NOT use IsLocalPlayer to query ownership.
            if (base.isServer)
            {
                MoveTowardsTarget();
                MoveTowardsOwner();
            }
        }
        #endregion
        #region NETWORK BEHAVIOUR
        public override void OnStartServer()
        {
            base.OnStartServer();
            petCombat = GetComponent<PetCombat>();//beacause it's only used on the server, we initialize it on the start of the server
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.isClientOnly)
            {
                characterController.enabled = false;
            }
        }
        #endregion
        #region LOCAL METHODS
        /// <summary>
        /// Moves the AI towards the closest target
        /// </summary>
        [Server]
        void MoveTowardsTarget()
        {   //If no owner, exit
            if (base.connectionToClient==null)
            {
                return;
            }
            if (!petCombat.Target)
            {
                return;
            }
            //Close enough, no need to move
            if (Vector3.Distance(transform.position, petCombat.Target.transform.position) < TARGET_PROXIMITY)
            {
                return;
            }
            Vector3 direction = (petCombat.Target.transform.position - transform.position).normalized * (moveRate * Time.deltaTime);
            direction += Physics.gravity * Time.deltaTime;

            characterController.Move(direction);
            transform.LookAt(petCombat.Target.transform);
        }
        [Server]
        void MoveTowardsOwner() 
        {
            //If no owner, exit
            if (base.connectionToClient == null)
            {
                return;
            }
            //If there's a target, exit. Favor moving towards the target
            if (petCombat.Target)
            {
                return;
            }
            //Initilize clientInstance and Owner only ONCE (better performance)
            if (!clientInstance)
            {
                clientInstance = ClientInstance.ReturnClientInstance(base.connectionToClient);//done on server so we need the connection
            }
            if (clientInstance)
            {
                if (!owner)
                {
                    owner = clientInstance.Respawner.CurrentlySpawnedCharacter; //This spares us a lot of processing time wasted on Getting and Finding and assigning
                }
            }
            if (owner)
            {
                //Close enough, no need to move
                if (Vector3.Distance(transform.position, owner.transform.position) < OWNER_PROXIMITY)
                {
                    return;
                }
                Vector3 direction = (owner.transform.position - transform.position).normalized * (moveRate * Time.deltaTime);
                direction += Physics.gravity * Time.deltaTime;

                characterController.Move(direction);
                transform.LookAt(owner.transform);
            }
           
        }
        #endregion
    }
}