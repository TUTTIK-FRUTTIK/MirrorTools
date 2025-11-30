using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

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
            if (Input.GetKeyDown(config.activationKey))
            {
                panel.SetActive(!panel.activeSelf); 
                SecurityManager.TryOpenAdminPanel(panel.activeSelf);

                if (panel.activeSelf) MTools.onPanelActivate?.Invoke();
                else MTools.onPanelDeactivate?.Invoke();
            }

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