/*Attached to the PlayerScore UI object and Manages/handles the data displayed on this object
 * **/
using UnityEngine;
using Mirror;
using TMPro;

namespace GettingStartedWithMirror.Clients
{
    public class PlayerScore : MonoBehaviour
    {
        #region VAR
        public uint NetId { get; private set; }

        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI scoreText;

        int score = 0;
        /// <summary>
        /// Namer for client instance belonging to NetId
        /// </summary>
        Namer namer;
        #endregion
        #region ACCESSORS
        //GETTERS
        public int GetScore() { return score; }
        //SETTERS
        public void SetNetId(uint value) 
        {
            NetId = value;
            if (NetworkIdentity.spawned.TryGetValue(NetId,out NetworkIdentity networkIdentity))
            {
                namer = networkIdentity.GetComponent<Namer>();
                namer.Relay_OnNameUpdated += Namer_Relay_OnNameUpdated;
            }
        }
        public void SetPlayerName(string name) 
        {
            nameText.text = name;
        }
        public void AddScore(int value) 
        {
            score += value;
            scoreText.text = score.ToString();
        }
        #endregion
        #region HANDLERS
        //HANDLERS
        /// <summary>
        /// Received when the name updates for this client instance
        /// </summary>
        /// <param name="name"></param>
        private void Namer_Relay_OnNameUpdated(string name)
        {
            SetPlayerName(name);
        }
        #endregion
        #region ENGINE
        //ENGINE
        private void OnDestroy()
        {
            if (namer)
            {
                namer.Relay_OnNameUpdated -= Namer_Relay_OnNameUpdated;
            }
        }
        #endregion
    }
}