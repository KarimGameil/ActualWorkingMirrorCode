
using System.Collections;
using UnityEngine;
using Mirror;
namespace GettingStartedWithMirror.Units
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Combat : NetworkBehaviour
    {
        #region VAR
        [Tooltip("VFX for projectiles")]
        [SerializeField] GameObject projectileVFX=null;
        [Tooltip("VFX for Hitting")]
        [SerializeField] GameObject hitVFX=null;
        /// <summary>
        /// next time this unit can attack
        /// </summary>
        float nextAttackTime = 0f;
        /// <summary>
        /// Colliders within this object
        /// </summary>
        Collider[] colliders;

        NetworkAnimator networkAnimator;
        #endregion
        #region ENGINE
        private void Awake()
        {
            //To set triggers over the network ,a NetworkAnimator is required instead of the regular Animator
            networkAnimator = GetComponent<NetworkAnimator>();
            //We need to turn colliders off not to hit ourselves with our own projectiles
            colliders = GetComponentsInChildren<Collider>();
        }

        private void Update()
        {
            if (base.hasAuthority)
            {
                CheckAttack();
            }
        }
        #endregion
        #region LOCAL METHODS
        /// <summary>
        /// Manages the attacking process (Input, Behaviour and Animation)
        /// </summary>
        void CheckAttack() 
        {
            if (!Input.GetKeyDown(KeyCode.Space))
            {
                return;
            }
            if (!FireTimeMet())
            {
                return;
            }

            StartCoroutine(SpawnProjectiles(transform.position,transform.forward));
            networkAnimator.SetTrigger("Attack");
            //Tell the server to attack from my position and in my forward direction (gonna raycast in that direction)
            CmdAttack(transform.position, transform.forward);
        }
        /// <summary>
        /// returns true if fire time has been met, otherwise increases the time until it does.
        /// </summary>
        /// <param name="resetTime"></param>
        /// <returns></returns>
        bool FireTimeMet(bool resetTime = true) 
        {
            bool result = (Time.time >= nextAttackTime);
            if (result && resetTime)
            {
                nextAttackTime = Time.time + 0.5f;
            }
            return result;
        }
        /// <summary>
        /// Tells Server to fire projectile (Raycast from given position and in given direction) from Owner
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        [Command]
        void CmdAttack(Vector3 pos,Vector3 dir) 
        {
            if (!FireTimeMet())
            {
                return;
            }
            dir = dir.normalized;
            //if position Client used is too far from pos on server then cap position (anti-cheat)
            float maxPositionOffset=1f;
            if (Vector3.Distance(pos,transform.position)>maxPositionOffset)
            {
                Vector3 posDirection = pos - transform.position;
                pos = transform.position + (posDirection * maxPositionOffset);
            }

            ///Do it here then tell others to replicate it
           
            //Spawn projectile on server
            StartCoroutine(SpawnProjectiles(pos, dir));
            //Tell all clients to have their version of my owned player spawn projectiles
            RpcAttack(pos, dir);
        }

        /// <summary>
        /// Received on clients to spawn projectile from the sender's owned player object
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        [ClientRpc]
        void RpcAttack(Vector3 pos, Vector3 dir) 
        {
            //We only want spectators to execute this so we can't have ourselves or the server do it.
            if (base.hasAuthority||base.isServer)
            {
                return;
            }
            StartCoroutine(SpawnProjectiles(pos, dir));
        }
        /// <summary>
        /// Traces for raycast Hit from fire position and direction
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        void TraceForHits(Vector3 pos,Vector3 dir) 
        {
            SetColliders(false);

            Ray ray = new Ray(pos, dir);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit, 100f))
            {
                Debug.Log(hit.collider.name);
                if (base.isClient)
                {
                    Instantiate(hitVFX, hit.point, Quaternion.identity);
                }
                //Seems we can access the other collider and their components ONLY ON SERVER without the need to use their NetworkConnection
                if (base.isServer)
                {
                    Health hitHealth = hit.collider.transform.root.GetComponent<Health>();
                    if (hitHealth && hitHealth.connectionToClient!=base.connectionToClient)
                    {
                        //Deal damage AND pass ourselves as the instigator
                        hitHealth.DealDamage(20f, base.connectionToClient);
                    }
                }
            }
            SetColliders(true);
        }
        /// <summary>
        /// Sets the enabled state on Colliders
        /// </summary>
        /// <param name="enabled"></param>
        void SetColliders(bool enabled) 
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = enabled;
            }
        }

        IEnumerator SpawnProjectiles(Vector3 pos, Vector3 dir) 
        {
            //Adjust the position of the raycast to offset a little bit from the origin/pivot of the player object
            pos += new Vector3(0f, 0.5f, 0f);
            TraceForHits(pos, dir);
            if (base.isClient)
            {
                //Instanitiate the projectile VFX and move it in the given direction to simulate bullet motion.
                GameObject vfx = Instantiate(projectileVFX, pos, Quaternion.identity);
                float moveRate = 300f;
                WaitForEndOfFrame wait = new WaitForEndOfFrame();
                while (vfx)
                {
                    vfx.transform.position += (dir * moveRate * Time.deltaTime);
                    yield return wait;
                }
            }
        }
        #endregion

    }
}