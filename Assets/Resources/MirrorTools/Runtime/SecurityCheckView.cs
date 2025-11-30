using UnityEngine;
using TMPro;

namespace MirrorTools
{
    public class SecurityCheckView : MonoBehaviour
    {
        public TMP_InputField inputField;

        public void EnterPassword()
        {
            SecurityManager.EnterPassword(inputField.text);
        }
    }
}

