/* Attached to the player object owned by Client Instance
 * Called by the Namer component attached to the Client Instance object to echo the changes in the NameCanvas text input field
 * Manages and updates the Playername UI tag above the player object 
 * **/
using UnityEngine;
using Mirror;
using TMPro;
namespace GettingStartedWithMirror.Units
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PlayerName : NetworkBehaviour
    {
        #region VAR
        [SyncVar(hook = nameof(UpdatePlayerNameText))]
        string synchronizedName = string.Empty;
        [Tooltip("The player name UI above the player object")]
        [SerializeField] TextMeshProUGUI text;
        #endregion
        #region MEMBER METHODS
        /// <summary>
        /// Sets the player name for owner
        /// </summary>
        /// <param name="name"></param>
        [Client]
        public void SetName(string name)
        {
            synchronizedName = name;
        }
        #endregion
        #region HANDLERS
        /// <summary>
        /// SyncVar hook for synchronizedName
        /// </summary>
        /// <param name="oldPlayerName">synchronizedName value before change</param>
        /// <param name="newPlayerName">synchronizedName value after change</param>
        void UpdatePlayerNameText(string oldPlayerName, string newPlayerName)
        {
            text.text = newPlayerName;
        }
        #endregion
    }
}