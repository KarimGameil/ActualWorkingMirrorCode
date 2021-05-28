
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using GettingStartedWithMirror.Units;

namespace GettingStartedWithMirror.AI
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PetCombat : NetworkBehaviour
    {
        #region VAR
        [Tooltip("How far the pet can look for enemies")]
        [SerializeField] float detectionRange = 50f;
        [Tooltip("How often pet can attack enemies")]
        [SerializeField] float attackInterval = 0.5f;
        /// <summary>
        /// next time this unit can attack
        /// </summary>
        float nextAttackTime = 0f;
        /// <summary>
        /// Current Target to attack
        /// </summary>
        public NetworkIdentity Target { get; private set; }
        /// <summary>
        /// How long before pet scans the area for enemies again
        /// </summary>
        float scanInterval = 1f;
        /// <summary>
        /// Next time pet can scan for enemies
        /// </summary>
        float nextScanTime = 0f;
        /// <summary>
        /// The distance whithin which pet can attack Target
        /// </summary>
        const float ATTACK_DISTANCE = 1.5f;
        
        #endregion
        #region ENGINE
        private void Update()
        {
            if (base.isServer)
            {
                TraceForTargets();
                CheckAttack();
            }
        }
        #endregion
        #region LOCAL METHODS

        /// <summary>
        /// Manages the attacking process (Input, Behaviour and Animation)
        /// </summary>
        [Server]
        void CheckAttack() 
        {
            //if no player owns this object, return
            if (base.connectionToClient==null)
            {
                return;
            }
            //no target to attack
            if (!Target)
            {
                return;
            }
            //not close enough to attack
            if (Vector3.SqrMagnitude(Target.transform.position-transform.position )>(ATTACK_DISTANCE*ATTACK_DISTANCE))
            {
                return;
            }
            //if cannot attack yet
            if (!FireTimeMet())
            {
                return;
            }
            //Actually attack the target and deal damage to him
            Health health = Target.GetComponent<Health>();
            if (health)
            {
                health.DealDamage(10f, base.connectionToClient);
            }
        }
        /// <summary>
        /// Traces for targets in the area
        /// </summary>
        /// <returns></returns>
        [Server]
        void TraceForTargets() 
        {
            if (Time.time<nextAttackTime)
            {
                return;
            }
            nextScanTime = Time.time + scanInterval;

            Health closestTarget = null;
            float closestDistance = -1f;
            
            //Only check for target if AI has an Owner
            if (base.connectionToClient!=null)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
                for (int i = 0; i < hits.Length; i++)
                {
                    Health health = hits[i].GetComponent<Health>();
                    if (!health)
                    {
                        continue;
                    }
                    /*If has an owner, and owner is not the same as who owns this AI
                     *In other words, if it is an enemy player*/
                    if (health.connectionToClient != null && health.connectionToClient != connectionToClient)
                    {

                        float distance = Vector3.SqrMagnitude(health.transform.position - transform.position);//Executes quicker than Vector3.Distance()
                        if (closestDistance == -1f || distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTarget = health;
                        }
                    }
                }
            }
            
            if (closestTarget)
            {
                Target = closestTarget.netIdentity;
            }
            else
            {
                Target = null;
            }
            
        }
        /// <summary>
        /// returns true if fire time has been met, otherwise increases the time until it does.
        /// </summary>
        /// <param name="resetTime"></param>
        /// <returns></returns>
        [Server]
        bool FireTimeMet(bool resetTime = true) 
        {
            bool result = (Time.time >= nextAttackTime);
            if (result&&resetTime)
            {
                nextAttackTime = Time.time + attackInterval;
            }
            return result;
        }
        
        #endregion

    }
}