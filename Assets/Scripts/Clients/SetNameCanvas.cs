/*Attached to the NameCanvas UI object in the GameplayScene
 *Listens to the changes in the TextMeshPro_InputField text input
 *Then tells the Namer component on this Client's ClientInstance to set player name to be the new text input
 ***/
using Mirror;
using TMPro;
using UnityEngine;

namespace GettingStartedWithMirror.Clients
{
    public class SetNameCanvas : MonoBehaviour
    {
        #region VAR
        [SerializeField] TMP_InputField input;
        string lastValue = string.Empty;
        #endregion
        #region ENGINE
        private void Update()
        {
            CheckSetName();
        }
        #endregion
        #region LOCAL METHODS
        void CheckSetName()
        {
            if (!NetworkClient.active)
            {
                return;
            }
            ClientInstance clientInstance = ClientInstance.ReturnClientInstance();
            if (!clientInstance)
            {
                lastValue = string.Empty;
                return;
            }
            if (input.text != lastValue)
            {
                lastValue = input.text;
                Namer namer = clientInstance.GetComponent<Namer>();
                namer.SetPlayerName(input.text);
            }
        }
        #endregion
    }
}