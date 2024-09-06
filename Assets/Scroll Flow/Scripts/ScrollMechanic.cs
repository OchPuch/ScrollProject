using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scroll_Flow.Scripts
{
    public class ScrollMechanic : MonoBehaviour, IDropHandler, IBeginDragHandler, IPointerExitHandler, IDragHandler,
        IPointerEnterHandler
    {
        [Header("Text prefab")] [SerializeField]
        private TemplateValueHolder templateValueHolderPrefab;

        [Header("Required objects")] [SerializeField]
        private Camera canvasCamera;

        [SerializeField] private RectTransform targetCanvas;

        [SerializeField] private RectTransform contentTarget;
        [SerializeField] private AutoSizeLayoutScrollFlow contentSize;

        [Header("Settings")] [SerializeField] private Color startColor;
        [Space(20)] [SerializeField] private float heightTemplate = 27;

        [SerializeField] private AnimationCurve scrollShapeCurve;
        [SerializeField] private AnimationCurve textOffsetControlCurve;

        [SerializeField] private float speedLerp = 5;
        [SerializeField] private float minVelocity = 0.2f;

        [SerializeField] private float shiftUp = 32;
        [SerializeField] private float shiftDown = 32;
        [SerializeField] private float padding;
        [Range(0, 1)] [SerializeField] private float colorPad = 0.115f;
        [SerializeField] private float maxFontSize = 48.2f;

        [SerializeField] private bool isElastic = true;
        [SerializeField] private float maxElastic = 50;

        [SerializeField] private float inertiaSense = 4;

        [Header("Mouse Wheel and Touchpad scroll methods")] [SerializeField]
        private bool isCanUseMouseWheel;

        [SerializeField] private bool isInvertMouseWheel;
        [SerializeField] private float mouseWheelSensibility = 0.5f;
        [SerializeField] private float touchpadSensibility = 0.5f;

        private RectTransform _ownRectTransform;
        private List<TemplateValueHolder> _templateValueHolders = new List<TemplateValueHolder>();

        private bool _isInfinite;

        private bool _isDragging;
        private bool _holdingMouse;
        private bool _mouseReleased;
        private float _inertia;

        private float _startPosContent;
        private float _startPosMouse;
        private float _middle;
        private float _heightText = 27;

        private int _countCheck = 4;
        private int _currentCenter;
        private bool _isInitialized;
        private int _countTotal;
        private int _padCount;
        private bool _isInArea;

        private float _padScroll;

        private int _lastValue;

        public event Action<int> ValueChanged;

        private float MouseScroll
        {
            get
            {
                var mouseScroll = Input.mouseScrollDelta.y;
                return mouseScroll != 0 ? mouseScroll : _padScroll;
            }
        }

        private Color _color;
        public Color Color
        {
            set
            {
                _color = value;
                foreach (var templateValueHolder in _templateValueHolders)
                {
                    templateValueHolder.TextMeshProUGUI.color = value;
                }
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.ScrollWheel)
                _padScroll = (-Event.current.delta.y / 10) * touchpadSensibility;
            else
                _padScroll = 0;
        }

        private void Awake()
        {
            _ownRectTransform = GetComponent<RectTransform>();
            _heightText = heightTemplate / 2;
            _middle = _ownRectTransform.sizeDelta.y / 2;
            contentSize.topPad = _middle - _heightText;
            contentSize.bottomPad = _middle - _heightText;
            _countCheck = Mathf.CeilToInt((_middle * 2) / heightTemplate);
        }

        /// <summary>
        /// Initialization method
        /// </summary>
        /// <param name="dataToInit"> List of texts to show </param>
        /// <param name="isInfinite"> Is scroll will be infinite </param>
        /// <param name="firstTarget"> Which text in list will be first </param>
        public void Initialize(List<string> dataToInit, bool isInfinite = false, int firstTarget = 0)
        {
            _templateValueHolders = new List<TemplateValueHolder>();
            _countTotal = dataToInit.Count;
            for (int i = 0; i < contentTarget.childCount; i++)
            {
                Destroy(contentTarget.GetChild(i).gameObject);
            }

            _isInfinite = isInfinite;

            if (isInfinite)
            {
                int half = _countCheck / 2 + 1;

                if (dataToInit.Count > half)
                {
                    _padCount = half;
                    for (int i = dataToInit.Count - half; i < dataToInit.Count; i++)
                    {
                        CreateValueHolder(dataToInit[i], i);
                    }
                }
                else
                {
                    _padCount = dataToInit.Count;
                    for (int j = 0; j < Mathf.CeilToInt(half / (float)dataToInit.Count); j++)
                    {
                        for (int i = 0; i < dataToInit.Count; i++)
                        {
                            CreateValueHolder(dataToInit[i], i);
                        }
                    }
                }

                isElastic = false;
                contentTarget.anchoredPosition = new Vector2(0, (firstTarget + _padCount) * (_heightText * 2));
            }
            else
            {
                _padCount = _countCheck / 2 + 1;
                contentTarget.anchoredPosition = new Vector2(0, firstTarget * (_heightText * 2));
            }

            for (int i = 0; i < dataToInit.Count; i++)
            {
                CreateValueHolder(dataToInit[i], i);
            }

            if (isInfinite)
            {
                int half = _countCheck / 2 + 1;
                if (dataToInit.Count > half)
                {
                    for (int i = 0; i < half; i++)
                    {
                        CreateValueHolder(dataToInit[i], i);
                    }
                }
                else
                {
                    for (int j = 0; j < Mathf.CeilToInt(half / (float)dataToInit.Count); j++)
                    {
                        for (int i = 0; i < dataToInit.Count; i++)
                        {
                            CreateValueHolder(dataToInit[i], i);
                        }
                    }
                }
            }

            Color = startColor;
            contentSize.Init();
            contentSize.UpdateLayout();
            _isInitialized = true;
        }

        private void CreateValueHolder(string text, int i)
        {
            var templateValueHolder = Instantiate(templateValueHolderPrefab, contentTarget.transform);
            templateValueHolder.TextMeshProUGUI.text = text;
            templateValueHolder.value = i;
            templateValueHolder.RectTransform.sizeDelta = new Vector2(_ownRectTransform.sizeDelta.x, heightTemplate);
            _templateValueHolders.Add(templateValueHolder);
        }

        /// <summary>
        /// Return list ID of current concentration
        /// </summary>
        /// <returns></returns>
        public int GetCurrentValue()
        {
            return _templateValueHolders[_currentCenter].value;
        }

        public void SetCurrentTarget(int firstTarget)
        {
            _lastValue = firstTarget;
            contentTarget.anchoredPosition = _isInfinite
                ? new Vector2(0, (firstTarget + _padCount) * (_heightText * 2))
                : new Vector2(0, firstTarget * (_heightText * 2));
        }

        private void UpdateInput()
        {
            _holdingMouse = Input.GetMouseButton(0);
            _mouseReleased = Input.GetMouseButtonUp(0);
            if (_mouseReleased)
            {
                _isDragging = false;
            }
            
            if (isCanUseMouseWheel && _isInArea && Input.mouseScrollDelta.y != 0)
            {
                _isDragging = true;
            }
            else if (!_holdingMouse)
            {
                _isDragging = false;
            }

        }

        private void Update()
        {
            if (!_isInitialized) return;
            
            UpdateInput();
            if (!_isDragging)
            {
                if (contentTarget.anchoredPosition.y + _inertia < 0)
                {
                    if (isElastic)
                    {
                        contentTarget.anchoredPosition = new Vector2(0, contentTarget.anchoredPosition.y + _inertia);
                        _inertia *= Mathf.Clamp(1 - Mathf.Abs(contentTarget.anchoredPosition.y) /
                            maxElastic, 0, 1);
                    }
                    else
                    {
                        contentTarget.anchoredPosition = new Vector2(0, 0);
                        _inertia = 0;
                    }
                }
                else if (contentTarget.anchoredPosition.y + _inertia > contentTarget.sizeDelta.y - _middle * 2)
                {
                    if (isElastic)
                    {
                        contentTarget.anchoredPosition = new Vector2(0, contentTarget.anchoredPosition.y + _inertia);
                        _inertia *= Mathf.Clamp(1 - Mathf.Abs((contentTarget.sizeDelta.y - _middle * 2) -
                                                              contentTarget.anchoredPosition.y) /
                            maxElastic, 0, 1);
                    }
                    else
                    {
                        contentTarget.anchoredPosition = new Vector2(0, contentTarget.sizeDelta.y - _middle * 2);
                        _inertia = 0;
                    }
                }
                else
                {
                    contentTarget.anchoredPosition = new Vector2(0, contentTarget.anchoredPosition.y + _inertia);
                    _inertia = Mathf.Lerp(_inertia, 0, inertiaSense * Time.deltaTime);
                }
            }
            else
            {
                if (isCanUseMouseWheel && _isInArea && MouseScroll != 0)
                {
                    if (isElastic)
                    {
                        if (contentTarget.anchoredPosition.y < 0)
                        {
                            _inertia = 0;
                            contentTarget.anchoredPosition = new Vector2(0,
                                contentTarget.anchoredPosition.y +
                                ((isInvertMouseWheel ? -1 : 1) * MouseScroll * mouseWheelSensibility)
                                * Mathf.Clamp(1 - Mathf.Abs(contentTarget.anchoredPosition.y) /
                                    maxElastic, 0, 1));
                        }
                        else if (contentTarget.anchoredPosition.y > contentTarget.sizeDelta.y - _middle * 2)
                        {
                            _inertia = 0;
                            contentTarget.anchoredPosition = new Vector2(0,
                                contentTarget.anchoredPosition.y +
                                ((isInvertMouseWheel ? -1 : 1) * MouseScroll * mouseWheelSensibility)
                                * Mathf.Clamp(1 - Mathf.Abs((contentTarget.sizeDelta.y - _middle * 2) -
                                                            contentTarget.anchoredPosition.y) /
                                    maxElastic, 0, 1));
                        }
                        else
                        {
                            _inertia += ((isInvertMouseWheel ? -1 : 1) * MouseScroll
                                                                       * mouseWheelSensibility);
                            contentTarget.anchoredPosition = new Vector2(0,
                                contentTarget.anchoredPosition.y +
                                ((isInvertMouseWheel ? -1 : 1) * MouseScroll * mouseWheelSensibility));
                        }
                    }
                    else
                    {
                        _inertia += ((isInvertMouseWheel ? -1 : 1) * MouseScroll
                                                                   * mouseWheelSensibility);
                        contentTarget.anchoredPosition = new Vector2(0, Mathf.Clamp(
                            contentTarget.anchoredPosition.y +
                            ((isInvertMouseWheel ? -1 : 1) * MouseScroll * mouseWheelSensibility),
                            0, contentTarget.sizeDelta.y - _middle * 2));
                    }
                }
                else
                {
                    if (isElastic)
                    {
                        if (contentTarget.anchoredPosition.y < 0)
                        {
                            _inertia = 0;
                            contentTarget.anchoredPosition = new Vector2(0,
                                _startPosContent + (-_startPosMouse + (Input.mousePosition.y / canvasCamera.pixelHeight)
                                    * targetCanvas.sizeDelta.y) * Mathf.Clamp(1 -
                                                                              Mathf.Abs(
                                                                                  contentTarget.anchoredPosition.y) /
                                                                              maxElastic, 0, 1));
                        }
                        else if (contentTarget.anchoredPosition.y > contentTarget.sizeDelta.y - _middle * 2)
                        {
                            _inertia = 0;
                            contentTarget.anchoredPosition = new Vector2(0,
                                _startPosContent + (-_startPosMouse + (Input.mousePosition.y / canvasCamera.pixelHeight)
                                    * targetCanvas.sizeDelta.y) * Mathf.Clamp(1 - Mathf.Abs(
                                        (contentTarget.sizeDelta.y - _middle * 2) -
                                        contentTarget.anchoredPosition.y) /
                                    maxElastic, 0, 1));
                        }
                        else
                        {
                            _inertia = _startPosContent + (-_startPosMouse +
                                                           (Input.mousePosition.y / canvasCamera.pixelHeight) *
                                                           targetCanvas.sizeDelta.y) -
                                       contentTarget.anchoredPosition.y;
                            contentTarget.anchoredPosition = new Vector2(0,
                                _startPosContent + (-_startPosMouse + (Input.mousePosition.y /
                                                                       canvasCamera.pixelHeight) *
                                    targetCanvas.sizeDelta.y));
                        }

                        _startPosMouse = (Input.mousePosition.y / canvasCamera.pixelHeight) * targetCanvas.sizeDelta.y;
                        _startPosContent = contentTarget.anchoredPosition.y;
                    }
                    else
                    {
                        _inertia = _startPosContent + (-_startPosMouse +
                                                       (Input.mousePosition.y / canvasCamera.pixelHeight) *
                                                       targetCanvas.sizeDelta.y) -
                                   contentTarget.anchoredPosition.y;
                        contentTarget.anchoredPosition = new Vector2(0, Mathf.Clamp(
                            _startPosContent + (-_startPosMouse + (Input.mousePosition.y /
                                                                   canvasCamera.pixelHeight) *
                                targetCanvas.sizeDelta.y), 0,
                            contentTarget.sizeDelta.y - _middle * 2));
                    }
                }
            }

            if (_isInfinite)
            {
                if (contentTarget.anchoredPosition.y < _middle)
                {
                    contentTarget.anchoredPosition = new Vector2(0, contentTarget.anchoredPosition.y +
                                                                    (_padCount + (_countTotal - _padCount)) *
                                                                    (_heightText * 2));
                    for (int i = 0; i < (_padCount + (_countTotal - _padCount)); i++)
                    {
                        _templateValueHolders[i].TextMeshProUGUI.fontSize = 0;
                    }

                    _startPosMouse = (Input.mousePosition.y / canvasCamera.pixelHeight) * targetCanvas.sizeDelta.y;
                    _startPosContent = contentTarget.anchoredPosition.y;
                }
                else if (contentTarget.anchoredPosition.y > contentTarget.sizeDelta.y - _middle * 3)
                {
                    contentTarget.anchoredPosition = new Vector2(0, contentTarget.anchoredPosition.y -
                                                                    (_padCount + (_countTotal - _padCount)) *
                                                                    (_heightText * 2));
                    for (int i = contentTarget.childCount - 1;
                         i >= contentTarget.childCount -
                         (_padCount + (_countTotal - _padCount));
                         i--)
                    {
                        _templateValueHolders[i].TextMeshProUGUI.fontSize = 0;
                    }

                    _startPosMouse = (Input.mousePosition.y / canvasCamera.pixelHeight) * targetCanvas.sizeDelta.y;
                    _startPosContent = contentTarget.anchoredPosition.y;
                }
            }

            float contentPos = contentTarget.anchoredPosition.y;

            int startPoint = Mathf.CeilToInt((contentPos - (_middle + _heightText)) / (_heightText * 2));
            int minID = Mathf.Max(0, startPoint);
            int maxID = Mathf.Min(contentTarget.transform.childCount, startPoint + _countCheck + 1);
            minID = Mathf.Clamp(minID, 0, int.MaxValue);
            maxID = Mathf.Clamp(maxID, 0, int.MaxValue);

            _currentCenter = Mathf.Clamp(Mathf.RoundToInt(contentPos / (_heightText * 2)), 0,
                contentTarget.childCount - 1);

            if (maxID > minID)
            {
                for (int i = minID; i < maxID; i++)
                {
                    var currentRect = _templateValueHolders[i].RectTransform;
                    var currentText = _templateValueHolders[i].TextMeshProUGUI;
                    var currentTextTransform = _templateValueHolders[i].TextMeshProRectTransform;
                    var ratio = Mathf.Clamp(
                        1 - Mathf.Abs(contentPos + currentRect.anchoredPosition.y + _middle) / (_middle - padding),
                        0,
                        1);
                    currentTextTransform.anchoredPosition = contentPos + currentRect.anchoredPosition.y + _middle > 0
                        ? new Vector2(0, -textOffsetControlCurve.Evaluate(1 - ratio) * shiftUp)
                        : new Vector2(0, textOffsetControlCurve.Evaluate(1 - ratio) * shiftDown);
                    currentText.fontSize = maxFontSize * scrollShapeCurve.Evaluate(ratio);
                    currentText.color = new Vector4(currentText.color.r, currentText.color.g, currentText.color.b,
                        Mathf.Clamp((ratio - colorPad) / (1 - colorPad), 0, _color.a));
                }
            }

            if (Mathf.Abs(_inertia) < minVelocity && !_holdingMouse)
            {
                _inertia = 0;
                contentTarget.anchoredPosition = new Vector2(0,
                    Mathf.Lerp(contentTarget.anchoredPosition.y,
                        -_templateValueHolders[_currentCenter].RectTransform.anchoredPosition
                            .y - _middle, speedLerp * Time.deltaTime));
            }
            else
            {
                CheckValue();
            }
        }

        private void CheckValue()
        {
            var currentValue = GetCurrentValue();
            if (currentValue != _lastValue)
            {
                ValueChanged?.Invoke(currentValue);
                _lastValue = currentValue;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            _isDragging = false;
            CheckValue();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _startPosMouse = (Input.mousePosition.y / canvasCamera.pixelHeight) * targetCanvas.sizeDelta.y;
            _startPosContent = contentTarget.anchoredPosition.y;
        }

        public ScrollMechanic(float startPosMouse)
        {
            _startPosMouse = startPosMouse;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isInArea = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isInArea = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            CheckValue();
        }
    }
}