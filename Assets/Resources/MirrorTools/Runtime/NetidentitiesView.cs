using Mirror;
using TMPro;
using UnityEngine;

namespace MirrorTools
{
    public class NetidentitiesView : MonoBehaviour
    {
        public TextMeshProUGUI identitiesText;

        public void UpdateIdentitiesList(string[] names)
        {
            string identitiesList = $"({names.Length} net identities)\n";
            int identityCount = 1;

            foreach (string name in names)
            {
                identitiesList += $"{identityCount++}. {name}\n";
            }
            
            identitiesText.text = identitiesList;
        }
    }
}