using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class LogView : MonoBehaviour
    {
        public TextMeshProUGUI logText;

        public void UpdateLogs(string logs)
        {
            logText.text = logs;
        }
    }
}

