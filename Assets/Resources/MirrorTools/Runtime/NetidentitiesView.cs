using Mirror;
using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class NetidentitiesView : MonoBehaviour
    {
        public TextMeshProUGUI identitiesText;

        public void UpdateIdentitiesList(string text)
        {
            identitiesText.text = text;
        }
    }
}