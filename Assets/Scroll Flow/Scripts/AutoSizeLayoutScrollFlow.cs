using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scroll_Flow.Scripts
{
    [ExecuteInEditMode]
    public class AutoSizeLayoutScrollFlow : MonoBehaviour
    {
        public bool isLoopUpdate; 
        
        public bool isVertical = true; 
        public bool isResizeSelf = true; 
          
        public float topPad; 
        public float bottomPad; 
        public float leftPad; 
        public float rightPad; 

        public float spacing; 

        public int repeatFrames = 2;

        private Coroutine _updateRoutine;

        private RectTransform _ownRectTransform;
        private readonly List<RectTransform> _childrenRects = new();

        private bool _isInitialized;
        
        public void Init()
        {
            _ownRectTransform = GetComponent<RectTransform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var rect = transform.GetChild(i).GetComponent<RectTransform>();
                _childrenRects.Add(rect);
            }

            _isInitialized = true;
        }

        private void Update() {
            
            if (!_isInitialized) return;
            if (isLoopUpdate) {
                UpdateLayout(false);
            }
        }

        public void UpdateLayout(bool isRepeat = true) {
            UpdateAllRect();
            if (!isRepeat) return;
            if(_updateRoutine != null) {
                StopCoroutine(_updateRoutine);
            }
            if (gameObject.activeInHierarchy) {
                _updateRoutine = StartCoroutine(UpdateRepeat());
            }
        }

        void UpdateAllRect() {
            if (isVertical) {
                float sizeTotal = topPad;
                foreach (var rect in _childrenRects)
                {
                    rect.anchoredPosition = new Vector2(leftPad - rightPad, -rect.sizeDelta.y * (1 - rect.pivot.y) - sizeTotal);
                    sizeTotal += rect.sizeDelta.y + spacing;
                }
                sizeTotal -= spacing;
                sizeTotal += bottomPad;
                if (isResizeSelf) {
                    _ownRectTransform.sizeDelta = new Vector2(_ownRectTransform.sizeDelta.x, sizeTotal);
                }
            } else {
                float sizeTotal = leftPad;
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (!transform.GetChild(i).gameObject.activeSelf) continue;
                    var rect = _childrenRects[i];
                    rect.anchoredPosition = new Vector2(rect.sizeDelta.x * (1 - rect.pivot.x) + sizeTotal, topPad - bottomPad);
                    sizeTotal += rect.sizeDelta.x + spacing;
                }
                sizeTotal -= spacing;
                sizeTotal += rightPad;
                if (isResizeSelf) {
                   _ownRectTransform.sizeDelta = new Vector2(sizeTotal, _ownRectTransform.sizeDelta.y);
                }
            }
        }

        IEnumerator UpdateRepeat() {
            for(int i = 0; i < repeatFrames; i++) {
                yield return new WaitForEndOfFrame();
                UpdateAllRect();
            }
        }
    }
}
