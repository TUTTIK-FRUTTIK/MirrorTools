using Mirror;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MirrorTools
{
    public class MainPanel : MonoBehaviour
    {
        public ConfigData config;
        public static MainPanel singleton;
        [HideInInspector] public GameObject panel;
        [HideInInspector] public InterfaceLinker interfaceLinker;

        private bool prevClientActiveState;
        private bool prevServerActiveState;
        private float lastRequestTime;

        private void Awake()
        {
            if (!singleton && config.showPanel) singleton = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            panel = transform.GetChild(0).gameObject;
            interfaceLinker = transform.GetChild(0).GetComponent<InterfaceLinker>();
            interfaceLinker.gameObject.SetActive(false);
        }

        private void Update()
        {
            
#if !UNITY_SERVER
            if (ShortcutIsPressed()) ChangePanelState();
#endif

            if (NetworkServer.active != prevServerActiveState)
            {
                prevServerActiveState = NetworkServer.active;
                if (NetworkServer.active) Init.InitializeServer();
                else Init.ResetServer();
            }

            if (NetworkClient.active != prevClientActiveState)
            {
                prevClientActiveState = NetworkClient.active;
                if (NetworkClient.active) Init.InitializeClient();
                else Init.ResetClient();
            }

            if (NetworkClient.isConnected && SecurityManager.isAuthenticated &&
                Time.time > lastRequestTime + config.autoRequestInterval)
            {
                lastRequestTime = Time.time;
                RequestData();
            }
        }

        private bool ShortcutIsPressed()
        {
            KeyCode[] keys = config.activationKeys;
            
#if !ENABLE_INPUT_SYSTEM
            for (int i = 0; i < keys.Length-1; i++)
            {
                if (!Input.GetKey(keys[i])) return false;
            }
            
            return Input.GetKeyDown(keys[^1]);
#else
            for (int i = 0; i < keys.Length-1; i++)
            {
                if (!Keyboard.current[keys[i].ConvertToKey()].isPressed) return false;
            }

            return Keyboard.current[keys[^1].ConvertToKey()].wasPressedThisFrame;
#endif
        }

        private void ChangePanelState()
        {
            panel.SetActive(!panel.activeSelf); 
            SecurityManager.TryOpenAdminPanel(panel.activeSelf);

            if (panel.activeSelf) MTools.onPanelActivate?.Invoke();
            else MTools.onPanelDeactivate?.Invoke();
        }

        private void OnDestroy()
        {
            if (NetworkServer.active) Init.ResetServer();
            if (NetworkClient.active) Init.ResetClient();
        }

        public void RequestData()
        {
            if (!NetworkClient.isConnected) return;
            if (interfaceLinker.generalView.gameObject.activeSelf) GeneralManager.RequestGeneralInfo();
            else if (interfaceLinker.playersView.gameObject.activeSelf) PlayersManager.RequestPlayersInfo();
            else if (interfaceLinker.netidentitiesView.gameObject.activeSelf) NetidentitiesManager.RequestIdentitiesInfo();
            else if (interfaceLinker.logView.gameObject.activeSelf) LogManager.RequestLogs();
        }
    }
}