/*This is attached to the HealthCanvas UI object in the GameplayScene
 *Monitors the Healthbar UI and updates it according to the Health component attached to the player object owned by this ClientInstance **/
using GettingStartedWithMirror.Units;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace GettingStartedWithMirror.Clients
{
    public class HealthCanvas : MonoBehaviour
    {
        #region VAR
        [SerializeField] Image healthImage;
        /// <summary>
        /// Health script on the unit which this client owns
        /// </summary>
        Health health;
        #endregion
        #region ENGINE
        private void Awake()
        {
            Respawner.OnOwnerCharacterSpawned += ClientInstance_OnOwnerCharacterSpawned;
        }
        private void OnDestroy()
        {
            Respawner.OnOwnerCharacterSpawned -= ClientInstance_OnOwnerCharacterSpawned;
        }
        private void Update()
        {
            CheckHealth();
        }
        #endregion
        #region HANDLERS
        /// <summary>
        /// This can be used as an initializer across the network instead of Start/Awake
        /// </summary>
        /// <param name="gameObject"></param>
        void ClientInstance_OnOwnerCharacterSpawned(GameObject gameObject)
        {
            if (gameObject)
            {
                health = gameObject.GetComponent<Health>();
            }
        }

        #endregion
        #region LOCAL METHODS
        /// <summary>
        /// updates the fill amount of the healthbar image to display the correct/current health ratio
        /// </summary>
        void UpdateHealthImage()
        {
            if (!health)
            {
                return;
            }
            float percentage = health.CurrentHealth / Health.MAX_HEALTH;
            healthImage.fillAmount = percentage;
        }


        void CheckHealth()
        {
            if (!NetworkClient.active)
            {
                return;
            }
            UpdateHealthImage();
        }
        #endregion
    }
}