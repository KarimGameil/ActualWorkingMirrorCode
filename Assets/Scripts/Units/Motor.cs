/* Attached to the player object owned by ClientInstance
 * Handles player object movement and rotation while also signalling the blendtree float to the animator attached to it.
 * **/
using UnityEngine;
using Mirror;
namespace GettingStartedWithMirror.Units
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Motor : NetworkBehaviour
    {
        #region VAR
        [SerializeField] CharacterController characterController = null;
        [Tooltip("How Quick Can Object Move")]
        [SerializeField] float moveRate = 3f;
        [Tooltip("How Quick Can Object Rotate")]
        [SerializeField] float rotateRate = 10f;
        /// <summary>
        /// Animator for this object
        /// </summary>
        Animator animator;
        #endregion
        #region ENGINE
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }
        void Update()
        {
            //This works better for player-owned objects and spawnables like pets or rts units. DO NOT use IsLocalPlayer to query ownership.
            if (base.hasAuthority)
            {
                Move();
            }
        }
        #endregion
        #region NETWORK BEHAVIOUR
        public override void OnStartClient()
        {
            base.OnStartClient();
            characterController.enabled = base.hasAuthority;//active for owner, inactive for spectators.
        }
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            animator = GetComponent<Animator>();
        }
        #endregion
        #region LOCAL METHODS
        private void Move()
        {
            //vector calculation
            float forward = Input.GetAxisRaw("Vertical");
            float rotation = Input.GetAxisRaw("Horizontal");
            Vector3 next = new Vector3(0f, 0f, forward * Time.deltaTime * moveRate);
            next += Physics.gravity * Time.deltaTime;
            //Actual movement and rotation
            transform.Rotate(new Vector3(0f, rotation * Time.deltaTime * rotateRate, 0f));
            characterController.Move(transform.TransformDirection(next));
            //Let the animator operate with 'forward' as the blendtree float 'Forward'
            animator.SetFloat("Forward", forward);
        }
        #endregion
    }
}