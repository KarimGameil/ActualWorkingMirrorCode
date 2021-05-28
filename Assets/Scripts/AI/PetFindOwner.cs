using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using GettingStartedWithMirror.Units;

namespace GettingStartedWithMirror.AI
{
    public class PetFindOwner : NetworkBehaviour
    {
        //This is run ONLY ON SERVER
        private void OnTriggerEnter(Collider other)
        {
            if (!base.isServer)
            {
                return;
            }
            CheckAssignOwner(other);
        }
        /// <summary>
        /// Checks if can be assigned to an owner
        /// </summary>
        /// <param name="other"></param>
        void CheckAssignOwner(Collider other)
        {
            //Already has an owner
            if (base.connectionToClient!=null)
            {
                return; 
            }
            //if we get a health script, we know for sure it's a player object
            Health health = other.GetComponent<Health>();
            if (health&&health.connectionToClient!=null)
            {
                base.netIdentity.AssignClientAuthority(health.connectionToClient);
            }
        }
    }
}