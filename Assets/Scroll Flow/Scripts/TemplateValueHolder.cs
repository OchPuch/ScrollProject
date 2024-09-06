using TMPro;
using UnityEngine;

namespace Scroll_Flow.Scripts
{
    public class TemplateValueHolder : MonoBehaviour
    {
        public int value;
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        [field: SerializeField] public TextMeshProUGUI TextMeshProUGUI { get; private set; } 
        [field: SerializeField] public RectTransform TextMeshProRectTransform { get; private set; } 
    }
}
