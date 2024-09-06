using System.Collections.Generic;
using UnityEngine;

namespace Scroll_Flow.Scripts
{
    public class ExampleScrollBootstrap : MonoBehaviour
    {
        [SerializeField] private ScrollMechanic scrollMechanic;
        [SerializeField] private List<string> data;
        [SerializeField] private bool isInfinite;
        [SerializeField] private int startIndex;

        private void Start()
        {
            scrollMechanic.Initialize(data, isInfinite, startIndex);
        }

        [ContextMenu("Get current item index")]
        private void GetCurrentItemIndex()
        {
            Debug.Log(scrollMechanic.GetCurrentValue());
        }
    }
}