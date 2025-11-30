using UnityEngine;

namespace MirrorTools
{
    public class ConfigData : ScriptableObject
    {
        [Header("General")]
        [Tooltip("Enables/disables the display of the Mirror Tools panel.")] 
        public bool showPanel = true;
        
        [Tooltip("The key required to activate/deactivate the Mirror Tools panel.")]
        public KeyCode activationKey = KeyCode.BackQuote;
        
        [Tooltip("The password that the server will request from the client before transmitting the data to it. If the line is empty, then the password is not required.")]
        public string password;
        
        [Tooltip("The time between the client's requests. In seconds.")]
        [Min(0.1f)] public float autoRequestInterval = 1f;
    }
}

