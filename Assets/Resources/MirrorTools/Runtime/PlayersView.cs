using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class PlayersView : MonoBehaviour
    {
        public TextMeshProUGUI playersText;

        public void UpdatePlayersList(string text)
        {
            playersText.text = text;
        }
    }
}
