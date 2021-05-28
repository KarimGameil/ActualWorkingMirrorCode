/*Utility component for VFX to self-destroy after a specified amount of time
 * **/
using UnityEngine;

public class DelayedDestroy : MonoBehaviour
{
    #region VAR
    [SerializeField] float delay = 1f;
    float destroyTime = -1f;
    #endregion
    #region ENGINE
    private void Awake()
    {
        destroyTime = Time.time + delay;
    }
    private void Update()
    {
        if (destroyTime!=-1&&Time.time>=destroyTime)
        {
            destroyTime = -1f;
            Destroy(gameObject);
        }
    }
    #endregion
}
