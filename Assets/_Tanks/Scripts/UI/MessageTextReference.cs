using TMPro;
using UnityEngine;

public class MessageTextReference : MonoBehaviour
{
    public TextMeshProUGUI Text
    {
        get { return GetComponent<TextMeshProUGUI>(); }
    }
}
