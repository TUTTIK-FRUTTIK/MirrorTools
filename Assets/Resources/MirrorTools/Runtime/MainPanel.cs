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
        [HideInInspector] public RectTransform panel;
        [HideInInspector] public InterfaceLinker interfaceLinker;

        private UIDragHandler dragHandler;
        private bool prevClientActiveState;
        private bool prevServerActiveState;
        private float lastRequestTime;
        private int prevPanelIndex;
        private bool windowMaximized;

        private void Awake()
        {
            if (!singleton && config.showPanel) singleton = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            panel = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
            interfaceLinker = transform.GetChild(0).GetComponent<InterfaceLinker>();
            dragHandler = GetComponent<UIDragHandler>();
            interfaceLinker.gameObject.SetActive(false);
            InitializeWindow();
            InitializeButtons();
            OpenLastPanel();
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

        private void InitializeButtons()
        {
            if (!config.enabledModules.HasFlag(PanelModule.General)) interfaceLinker.moduleButtons[0].SetActive(false);
            if (!config.enabledModules.HasFlag(PanelModule.Players)) interfaceLinker.moduleButtons[1].SetActive(false);
            if (!config.enabledModules.HasFlag(PanelModule.Netidentities)) interfaceLinker.moduleButtons[2].SetActive(false);
            if (!config.enabledModules.HasFlag(PanelModule.Logs)) interfaceLinker.moduleButtons[3].SetActive(false);
            if (!config.enabledModules.HasFlag(PanelModule.Console)) interfaceLinker.moduleButtons[4].SetActive(false);
        }

        private void InitializeWindow()
        {
            if (!PlayerPrefs.HasKey("WindowMaximized")) PlayerPrefs.SetString("WindowMaximized", "0");
            if (!PlayerPrefs.HasKey("WindowPositionX")) PlayerPrefs.SetFloat("WindowPositionX", 0f);
            if (!PlayerPrefs.HasKey("WindowPositionY")) PlayerPrefs.SetFloat("WindowPositionY", 0f);
            PlayerPrefs.Save();
            
            windowMaximized = PlayerPrefs.GetString("WindowMaximized") == "1";
            
            float x = PlayerPrefs.GetFloat("WindowPositionX");
            float y = PlayerPrefs.GetFloat("WindowPositionY");
            panel.anchoredPosition = new Vector2(x, y);
            dragHandler.lastAnchoredPosition = new Vector2(x, y);

            ConfigureWindowSize();
        }

        private void OpenLastPanel()
        {
            if (!PlayerPrefs.HasKey("LastOpenedPanel"))
            {
                OpenPanel(0);
                return;
            }
            
            OpenPanel(PlayerPrefs.GetInt("LastOpenedPanel"));
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

        public void ChangePanelState()
        {
            panel.gameObject.SetActive(!panel.gameObject.activeSelf); 
            SecurityManager.TryOpenAdminPanel(panel.gameObject.activeSelf);

            if (panel.gameObject.activeSelf) MTools.onPanelActivate?.Invoke();
            else MTools.onPanelDeactivate?.Invoke();
        }

        private void OnDestroy()
        {
            if (NetworkServer.active) Init.ResetServer();
            if (NetworkClient.active) Init.ResetClient();
        }

        public void OpenPanel(int index)
        {
            interfaceLinker.modulePanels[prevPanelIndex].SetActive(false);
            interfaceLinker.modulePanels[index].SetActive(true);
            prevPanelIndex = index;
            PlayerPrefs.SetInt("LastOpenedPanel", index);
        }

        public void ChangeMaximizeWindowState()
        {
            windowMaximized = !windowMaximized;
            PlayerPrefs.SetString("WindowMaximized", windowMaximized ?  "1" : "0");
            PlayerPrefs.Save();
            
            ConfigureWindowSize();
        }

        private void ConfigureWindowSize()
        {
            if (windowMaximized)
            {
                panel.anchorMin = new Vector2(0, 0);
                panel.anchorMax = new Vector2(1, 1);

                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;

                panel.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                panel.anchoredPosition = dragHandler.lastAnchoredPosition;
                panel.anchorMin = new Vector2(0.5f, 0.5f);
                panel.anchorMax = new Vector2(0.5f, 0.5f);
                panel.pivot = new Vector2(0.5f, 0.5f);
                panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1400);
                panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 750);
            }
            
            dragHandler.canBeMoved = !windowMaximized;
        }

        public void RequestData()
        {
            if (!NetworkClient.isConnected || !panel.gameObject.activeSelf) return;
            if (interfaceLinker.generalView.gameObject.activeSelf) GeneralManager.RequestGeneralInfo();
            else if (interfaceLinker.playersView.gameObject.activeSelf) PlayersManager.RequestPlayersInfo();
            else if (interfaceLinker.netidentitiesView.gameObject.activeSelf) NetidentitiesManager.RequestIdentitiesInfo();
            else if (interfaceLinker.logView.gameObject.activeSelf) LogManager.RequestLogs();
        }
    }
}