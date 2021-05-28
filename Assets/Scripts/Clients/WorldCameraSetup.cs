using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GettingStartedWithMirror.Clients
{
    public class WorldCameraSetup : MonoBehaviour
    {
        [Tooltip("Offset to be form the Character")]
        [SerializeField] Vector3 positionOffset = new Vector3(0f, 2f, -4f);
        Transform target=null;
        private void Awake()
        {
            Respawner.OnOwnerCharacterSpawned += ClientInstance_OnOwnerCharacterSpawned;
        }
        private void Update()
        {
            if (!target)
            {
                return;
            }
            //transform.rotation = target.rotation;
            transform.position = target.position + positionOffset;
        }
        private void OnDestroy()
        {
            Respawner.OnOwnerCharacterSpawned -= ClientInstance_OnOwnerCharacterSpawned;
        }
        void ClientInstance_OnOwnerCharacterSpawned(GameObject gameObject) 
        {
            if (gameObject!=null)
            {
                target = gameObject.transform;
            }
        }
        //Is executed only if you're the owner, unlike OnStartClient which executes for all clients
        /*public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            cameraObject.gameObject.SetActive(true);
        }*/
    }
}