using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MirrorTools
{
    public class ConfigData : ScriptableObject
    {
        [Header("General")] [Tooltip("Enables/disables the display of the Mirror Tools panel.")]
        public bool showPanel = true;

        [Tooltip("The shortcut that required to activate/deactivate the Mirror Tools panel. The order of the keys matters!")]
        public KeyCode[] activationKeys = { KeyCode.BackQuote };

        [Tooltip("The password that the server will request from the client before transmitting the data to it. If the line is empty, then the password is not required.")]
        public string password;

        [Tooltip("The time between the client's requests. In seconds.")] [Min(0.1f)]
        public float autoRequestInterval = 1f;

        [Tooltip("Modules that will be available in runtime.")]
        public PanelModule enabledModules = (PanelModule)~0;
    }
}